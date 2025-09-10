using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Logship.Template.Utility.Internal
{
    public sealed class LogshipExporter : ILogshipExporter, IDisposable
    {
        private readonly IHttpClientFactory factory;
        private readonly string endpoint;
        private readonly Guid account;
        private readonly string bearerToken;

        public LogshipExporter(IHttpClientFactory factory, string endpoint, Guid account, string bearerToken)
        {
            this.factory = factory;

            this.endpoint = endpoint;
            this.account = account;
            this.bearerToken = bearerToken;
        }

        public void Dispose()
        {
        }

        public async Task SendAsync(IReadOnlyList<LogshipLogEntrySchema> entries, CancellationToken token)
        {
            using var stream = new MemoryStream(1000);
            using var client = factory.CreateClient(nameof(LogshipExporter));
            await UploadMetrics(client, endpoint, this.account, this.bearerToken, entries, token);
        }

        private static async Task UploadMetrics(HttpClient client, string endpoint, Guid subscription, string bearerToken, IReadOnlyList<LogshipLogEntrySchema> entries, CancellationToken token)
        {
            var content = JsonSerializer.Serialize<IReadOnlyList<LogshipLogEntrySchema>>(entries, ExporterSerializerContext.Default.IReadOnlyListLogshipLogEntrySchema);

            var request = new HttpRequestMessage(HttpMethod.Put, $"{endpoint}/inflow/{subscription}");
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await client.SendAsync(request, token);
            response.EnsureSuccessStatusCode();
        }
    }

    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(decimal))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(float))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(LogshipLogEntrySchema))]
    [JsonSerializable(typeof(IReadOnlyList<LogshipLogEntrySchema>))]
    public sealed partial class ExporterSerializerContext : JsonSerializerContext { }

    
}
