# Logship Template Exporter

A .NET template for creating custom Logship exporters. This template provides a foundation for building data exporters that send information to Logship.

## Quick Start

To create a new exporter from this template:

### 1. Clone

Clone this repository

### 2. Run Template Setup

Use the provided PowerShell script to replace all template references with your custom name:

```powershell
./template-setup.ps1 -NewName "Template"
```

This script will:
- Replace the source name (default: `Template`) with `Template` (maintaining PascalCase)
- Replace lowercase version (default: `template`) with `template` (converting to lowercase)
- Update all file contents, directory names, and file names
- Process `.cs`, `.csproj`, `.json`, `.yml`, and other relevant files
- Use word boundaries to avoid partial replacements


### 3. Build and Run

```bash
dotnet tool install --global Microsoft.VisualStudio.SlnGen.Tool
slngen
dotnet build

dotnet run --project src/ConsoleHost/Logship.Template.Exporter/Logship.Template.Exporter.ConsoleHost.csproj
```

## Project Structure

```
├── src/
│   ├── ConsoleHost/
│   │   └── Logship.Template.Exporter/       # Console application entry point
│   │       ├── Program.cs                           # Main application
│   │       ├── appsettings.json                     # Configuration
│   │       └── Internal/                            # Internal logging
│   └── Util/
│       └── Logship.Template.Utility/        # Core export functionality
│           ├── BaseIntervalService.cs               # Base service for interval-based exports
│           ├── Extensions.cs                        # Dependency injection extensions
│           ├── LogshipLogEntrySchema.cs             # Data schema
│           └── Internal/                            # Internal implementation
│               ├── ILogshipExporter.cs              # Exporter interface
│               ├── LogshipExporter.cs               # Direct HTTP exporter
│               └── LogshipAgentExporter.cs          # Agent-based exporter
├── build/                                           # Build configuration
├── .github/                                         # CI/CD workflows
├── template-setup.ps1                               # Template setup script
└── README.md                                        # This file
```

## How It Works

1. **BaseIntervalService**: Inherit from this class to create services that run on intervals
2. **LogshipExporter**: Handles direct HTTP communication with Logship API
3. **LogshipAgentExporter**: Routes data through a local Logship agent
4. **Configuration**: Uses standard .NET configuration with appsettings.json

## Development

### Extending the Template

1. Implement your data collection logic in a class that inherits from `BaseIntervalService`
2. Use the `ILogshipExporter` to send data to Logship
3. Configure the service in `Extensions.cs`
4. Register your service in `Program.cs`

### Example Service Implementation

```csharp
public class MyCustomService : BaseIntervalService
{
    private readonly ILogshipExporter exporter;

    public MyCustomService(ILogshipExporter exporter, ILogger<MyCustomService> logger) 
        : base(TimeSpan.FromMinutes(5), logger)
    {
        this.exporter = exporter;
    }

    protected override async Task ExecuteIntervalAsync(CancellationToken cancellationToken)
    {
        // Collect your data
        var data = await CollectDataAsync();
        
        // Convert to Logship format
        var entries = data.Select(d => new LogshipLogEntrySchema
        {
            Timestamp = DateTime.UtcNow,
            Message = d.ToString(),
            // Add other properties as needed
        }).ToList();

        // Send to Logship
        await exporter.SendAsync(entries, cancellationToken);
    }
}
```

### Testing

The template includes configurations for both direct Logship API integration and local agent routing for testing.

## Deployment

### Docker

A `Containerfile` is included for containerized deployments:

```bash
podman build . -f Containerfile -t logship-template-exporter:latest
podman run --rm logship-template-exporter:latest

podman run --rm -v /path/to/config:/app/appsettings.json logship-template-exporter:latest
```

### Self-Contained Executable

For standalone deployment without .NET runtime requirements:

```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

## Configuration Options

| Setting | Description | Example |
|---------|-------------|---------|
| `outputMode` | `"direct"` for API, `"agent"` for local agent | `"direct"` |
| `logshipEndpoint` | Logship API endpoint | `"https://backend.logship.example"` |
| `logshipAccount` | Your Logship account ID | `"00000000-0000-0000-0000-000000000000"` |
| `logshipBearerToken` | Authentication token | `"zzz..."` |

## Contributing

1. Keep the template generic and reusable
2. Update this README when adding new features
3. Ensure the template setup script handles all new template references
4. Test with multiple exporter names to verify the template system works

## License

[Add your license information here]