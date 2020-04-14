using __ProjectName__.Controller;
using __ProjectName__.Crd;
using __ProjectName__.Diagnostics;
using k8s;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace __ProjectName__
{
    public class Operator : IHostedService
    {
        private Watcher<{{crdname}}Resource> _watcher;

        private readonly Channel<WatchData> _channel;

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly IKubernetes _kubernetesClient;
        private readonly I__ProjectName__Controller _controller;
        private readonly OperatorDiagnostics _diagnostics;

        public Operator(
            IKubernetes kubernetesClient,
            I__ProjectName__Controller controller,
            OperatorDiagnostics diagnostics)
        {
            _kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

            _channel = Channel.CreateUnbounded<WatchData>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = true
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _diagnostics.OperatorStarting();

            // starting the channel reader task 
            // and the Kubernetes watcher

            _ = Task.Run(WatchListener);
            await StartWatcher(_stoppingCts.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _diagnostics.OperatorShuttingdown();

            _stoppingCts.Cancel();
            _channel.Writer.Complete();
            
            if (_watcher != null && _watcher.Watching)
            {
                //if response is not finished when hosted service is 
                //on close this object will be null.
                _watcher.Dispose();
            }
            
            return Task.CompletedTask;
        }
      
        private Task StartWatcher(CancellationToken cancellationToken)
        {
            var response = _kubernetesClient.ListClusterCustomObjectWithHttpMessagesAsync(
                group: CRDConstants.Group,
                version: CRDConstants.Version,
                plural: CRDConstants.Plural,
                watch: true,
                timeoutSeconds: ((int)TimeSpan.FromMinutes(60).TotalSeconds),
                cancellationToken: cancellationToken);

            _watcher = response.Watch<{{crdname}}Resource, object>(
                onEvent: async (type, item) =>
                {
                    await _channel.Writer.WriteAsync(new WatchData()
                    {
                        EventType = type,
                        Resource = item
                    }, cancellationToken);
                },
                onClosed: () =>
                {
                    // watcher is closed and reactivated automatically 
                    // every "timeoutSeconds"  

                    if (!_stoppingCts.IsCancellationRequested)
                    {
                        _watcher.Dispose();
                        _ = StartWatcher(_stoppingCts.Token);
                    }
                },
                onError: e => _diagnostics.WatcherThrow(e));
            
            return Task.CompletedTask;
        }

        private async Task WatchListener()
        {
            while (await _channel.Reader.WaitToReadAsync() && !_stoppingCts.IsCancellationRequested)
            {
                while (_channel.Reader.TryRead(out WatchData watchData))
                {
                    if (watchData.EventType == WatchEventType.Added)
                    {
                        _diagnostics.OperatorEventAdded();

                        /*
                         * This is where you write your custom code. Typically you 
                         * need to invoke some method on controller to perform the 
                         * desired actions when a new CRD object is deployed on K8S.
                         */
                        await _controller.Do(watchData.Resource);
                    }
                    else if (watchData.EventType == WatchEventType.Deleted)
                    {
                        _diagnostics.OperatorEventDeleted();

                        /*
                          * This is where you write your custom code. Typically you 
                          * need to invoke some method on controller to perform the 
                          * desired actions when a  CRD object is deleted on K8S.
                          */
                        await _controller.UnDo(watchData.Resource);
                    }
                }
            }
        }

        private class WatchData
        {
            public WatchEventType EventType { get; set; }

            public {{crdname}}Resource Resource { get; set; }
        }
    }
}
