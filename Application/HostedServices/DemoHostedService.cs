using Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.ViewModels;

namespace Application.HostedServices
{
    // Copyright (c) .NET Foundation. Licensed under the Apache License, Version 2.0.
    /// <summary>
    /// Base class for implementing a long running <see cref="IHostedService"/>.
    /// </summary>
    public class DemoHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<DemoHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private static readonly SemaphoreSlim _slock = new(1, 1);
        private Timer? _timer = null;

        public DemoHostedService(IServiceProvider serviceProvider, ILogger<DemoHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            //_timer = new Timer(UploadDocument, null, TimeSpan.Zero,
            //TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private async void UploadDocument(object? state)
        {
            if (_slock.Wait(100))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        //var _documentProcessService = scope.ServiceProvider.GetService<IDocumentProcessService>();
                        //if (_documentProcessService != null)
                        //{
                        //    var data = _documentProcessService.BackgroundDocumentProcessing();
                        //    Parallel.ForEach(data, x =>
                        //     {
                        //         up(x).ConfigureAwait(false);
                        //     });
                        //    await Task.Delay(5000);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occur while running FailoverDocument background processes : ", ex);
                }
                finally
                {
                    _slock.Release();
                }
            }
        }

        //private async Task up(DocumentModelVm model)
        //{
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        //var _documentProcessService = scope.ServiceProvider.GetService<IDocumentProcessService>();
        //        //await _documentProcessService.DocumentProcessing(model, true);
        //    }
        //}
        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
