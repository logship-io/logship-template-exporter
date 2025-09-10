using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Logship.Template.Utility.Internal
{
    public sealed class LogshipAgentExporter : ILogshipExporter, IDisposable
    {
        private readonly ILogger<LogshipAgentExporter> logger;
        private readonly UdpClient udpClient;
        private readonly IPEndPoint endpoint;
        private readonly object mutex = new object();
        private bool disposed;

        public LogshipAgentExporter(string endpoint, ILogger<LogshipAgentExporter> logger)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            }

            var uri = new Uri(endpoint);
            var host = uri.Host;
            var port = uri.Port;

            if (port == -1)
            {
                port = uri.Scheme == "https" ? 443 : 80;
            }

            IPAddress? ipAddress;
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.Loopback;
            }
            else if (!IPAddress.TryParse(host, out ipAddress))
            {
                var hostEntry = Dns.GetHostEntry(host);
                ipAddress = hostEntry.AddressList[0];
            }

            this.endpoint = new IPEndPoint(ipAddress!, port);
            this.udpClient = new UdpClient();
            this.logger = logger;
        }

        public async Task SendAsync(IReadOnlyList<LogshipLogEntrySchema> entries, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            foreach (var entry in entries)
            {
                using var stream = new MemoryStream(2048);
                try
                {
                    await JsonSerializer.SerializeAsync(stream, entry, ExporterSerializerContext.Default.LogshipLogEntrySchema, token);
                    await udpClient.SendAsync(stream.ToArray(), endpoint, token);
                }
                catch (Exception ex)
                {
                    Log.AgentExportFailed(logger, ex);
                }
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                lock (mutex)
                {
                    if (!disposed)
                    {
                        udpClient.Dispose();
                        disposed = true;
                    }
                }
            }
        }
    }
}