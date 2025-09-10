using Logship.Template.Utility.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Logship.Template.Utility
{
    internal abstract class BaseIntervalService : IHostedService, IDisposable
    {
        protected virtual TimeSpan Interval { get; } = TimeSpan.FromMinutes(1);
        private CancellationTokenSource _cts = new();
        private Task? _task;
        private readonly ILogshipExporter exporter;
        protected readonly ILogger logger;

        public BaseIntervalService(ILogshipExporter exporter, ILogger logger)
        {
            this.exporter = exporter;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._task = ExecuteAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this._cts.Cancel();
            try
            {
                if (this._task != null
                    && this._task.IsCanceled == false)
                {
                    await this._task.WaitAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log.UncaughtServiceException(logger, ex);
            }
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            while (token.IsCancellationRequested == false
                && this._cts.IsCancellationRequested == false)
            {
                try
                {
                    var results = await this.FetchDataAsync(token);
                    await this.exporter.SendAsync(results, token);
                } catch (OperationCanceledException) when (token.IsCancellationRequested) { }
                catch(Exception ex)
                {
                    Log.UncaughtServiceException(logger, ex);
                }

                await Task.Delay(Interval, this._cts.Token);
            }
        }

        protected abstract Task<IReadOnlyList<LogshipLogEntrySchema>> FetchDataAsync(CancellationToken cancellationToken);

        public void Dispose()
        {
            ((IDisposable)_cts).Dispose();
        }
    }
}
