using Microsoft.Extensions.Logging;
using System;

namespace Logship.Template.Utility.Internal
{
    internal sealed partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Uncaught Exception in IntervalService")]
        public static partial void UncaughtServiceException(ILogger logger, Exception ex);

        [LoggerMessage(LogLevel.Error, "Failed to send UDP message")]
        public static partial void AgentExportFailed(ILogger logger, Exception ex);
    }
}
