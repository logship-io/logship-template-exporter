#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the logship-template-exporter template by replacing template references with a custom name.

.DESCRIPTION
    This script replaces all occurrences of the source template name in the codebase with the specified 
    replacement value, maintaining proper casing. It updates file contents, directory names, and file names.

.PARAMETER NewName
    The new name to replace the source template name with. The script will automatically handle casing.

.PARAMETER SourceName
    The source template name to replace. Defaults to "Template".

.EXAMPLE
    .\template-setup.ps1 -NewName "Plex"
    This will replace "Template" with "Plex" and "template" with "plex" throughout the codebase.

.EXAMPLE
    .\template-setup.ps1 -NewName "NewExporter" -SourceName "Gtfs"
    This will replace "Gtfs" with "NewExporter" and "gtfs" with "newexporter" throughout the codebase.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$NewName,
    
    [Parameter(Mandatory = $false)]
    [string]$SourceName = "Template"
)

# Validate input
if ([string]::IsNullOrWhiteSpace($NewName)) {
    Write-Error "NewName cannot be empty or whitespace"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($SourceName)) {
    Write-Error "SourceName cannot be empty or whitespace"
    exit 1
}

# Generate different casing variations
$NewPascalCase = $NewName
$NewLowerCase = $NewName.ToLower()
$SourcePascalCase = $SourceName
$SourceLowerCase = $SourceName.ToLower()

Write-Host "Setting up logship-$NewLowerCase-exporter template..." -ForegroundColor Green
Write-Host "Replacing '$SourcePascalCase' -> '$NewPascalCase'" -ForegroundColor Yellow
Write-Host "Replacing '$SourceLowerCase' -> '$NewLowerCase'" -ForegroundColor Yellow

# Define file extensions to process
$fileExtensions = @("*.cs", "*.csproj", "*.props", "*.targets", "*.json", "*.yml", "*.yaml", "*.md", "Containerfile")

# Get all files to process (excluding build output directories and git)
$filesToProcess = @()
foreach ($extension in $fileExtensions) {
    $files = Get-ChildItem -Path . -Recurse -Include $extension | Where-Object {
        $_.FullName -notmatch "\\(bin|obj|\\.git)(\\.|\$)" -and
        $_.FullName -notmatch "\\packages\\" -and
        $_.FullName -notmatch "\\.vs\\" -and
        $_.FullName -notmatch "\\TestResults\\"
    }
    $filesToProcess += $files
}

Write-Host "Processing $($filesToProcess.Count) files..." -ForegroundColor Cyan

# Process file contents
foreach ($file in $filesToProcess) {
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    
    # Replace source name with new name - handle both word boundaries and dot-separated identifiers
    # First handle compound identifiers (e.g., Logship.Gtfs.Utility)
    $content = $content -replace "(\W|^)$SourcePascalCase(\W|\$)", "`$1$NewPascalCase`$2"
    $content = $content -replace "(\W|^)$SourceLowerCase(\W|\$)", "`$1$NewLowerCase`$2"
    
    # Also handle cases where it appears in paths or identifiers with dots/separators
    $content = $content -replace "\.$SourcePascalCase\.", ".$NewPascalCase."
    $content = $content -replace "\.$SourceLowerCase\.", ".$NewLowerCase."
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  Updated: $($file.FullName)" -ForegroundColor Green
    }
}

# Process directory names (from deepest to shallowest to avoid path issues)
$directories = Get-ChildItem -Path . -Recurse -Directory | Where-Object {
    $_.FullName -notmatch "\\(bin|obj|\\.git)(\\.|\$)" -and
    $_.FullName -notmatch "\\packages\\" -and
    $_.FullName -notmatch "\\.vs\\" -and
    ($_.Name -like "*$SourcePascalCase*" -or $_.Name -like "*$SourceLowerCase*")
} | Sort-Object { $_.FullName.Length } -Descending

foreach ($dir in $directories) {
    $newName = $dir.Name
    # Handle compound names like Logship.Gtfs.Utility
    $newName = $newName -replace "(\W|^)$SourcePascalCase(\W|\$)", "`$1$NewPascalCase`$2"
    $newName = $newName -replace "(\W|^)$SourceLowerCase(\W|\$)", "`$1$NewLowerCase`$2"
    $newName = $newName -replace "\.$SourcePascalCase\.", ".$NewPascalCase."
    $newName = $newName -replace "\.$SourceLowerCase\.", ".$NewLowerCase."
    
    if ($newName -ne $dir.Name) {
        $newPath = Join-Path $dir.Parent.FullName $newName
        Rename-Item -Path $dir.FullName -NewName $newName
        Write-Host "  Renamed directory: $($dir.FullName) -> $newPath" -ForegroundColor Green
    }
}

# Process file names
$files = Get-ChildItem -Path . -Recurse -File | Where-Object {
    $_.FullName -notmatch "\\(bin|obj|\\.git)(\\.|\$)" -and
    $_.FullName -notmatch "\\packages\\" -and
    $_.FullName -notmatch "\\.vs\\" -and
    ($_.Name -like "*$SourcePascalCase*" -or $_.Name -like "*$SourceLowerCase*") -and
    $_.Name -ne "template-setup.ps1"
}

foreach ($file in $files) {
    $newName = $file.Name
    # Handle compound names like Logship.Gtfs.Utility.csproj
    $newName = $newName -replace "(\W|^)$SourcePascalCase(\W|\$)", "`$1$NewPascalCase`$2"
    $newName = $newName -replace "(\W|^)$SourceLowerCase(\W|\$)", "`$1$NewLowerCase`$2"
    $newName = $newName -replace "\.$SourcePascalCase\.", ".$NewPascalCase."
    $newName = $newName -replace "\.$SourceLowerCase\.", ".$NewLowerCase."
    
    if ($newName -ne $file.Name) {
        $newPath = Join-Path $file.Directory.FullName $newName
        Rename-Item -Path $file.FullName -NewName $newName
        Write-Host "  Renamed file: $($file.FullName) -> $newPath" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Template setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Update the appsettings.json file with your specific configuration"
Write-Host "2. Build the project: dotnet build"
Write-Host "3. Run the exporter: dotnet run --project src/ConsoleHost/Logship.$NewPascalCase.Exporter/Logship.$NewPascalCase.Exporter.ConsoleHost.csproj"
Write-Host ""
Write-Host "To build and publish:" -ForegroundColor Yellow
Write-Host "  dotnet publish src/ConsoleHost/Logship.$NewPascalCase.Exporter/Logship.$NewPascalCase.Exporter.ConsoleHost.csproj -c Release -r win-x64 --self-contained"
Write-Host ""