using k8s;
using __ProjectName__.Controller;
using __ProjectName__.Crd;
using __ProjectName__.Diagnostics;
using __ProjectName__.Operator.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

// Find a complete operator sample at 
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/tree/master/src/HealthChecks.UI.K8s.Operator/Operator

namespace __ProjectName__.Operator
{
    internal class __ProjectName__Operator
        : IKubernetesOperator
    {
        private Watcher<{{crdname}}Resource> _watcher;

        private readonly IKubernetes _kubernetesClient;
        private readonly I__ProjectName__Controller _controller;
        private readonly OperatorDiagnostics _diagnostics;

        public  __ProjectName__Operator(
            IKubernetes kubernetesClient,
            I__ProjectName__Controller controller,
            OperatorDiagnostics diagnostics)
        {
            _kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await StartWatcher(cancellationToken);
        }

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }

        private async Task StartWatcher(CancellationToken token)
        {
            var response = await _kubernetesClient.ListClusterCustomObjectWithHttpMessagesAsync(
                group: CRDConstants.Group,
                version: CRDConstants.Version,
                plural: CRDConstants.Plural,
                watch: true,
                timeoutSeconds: ((int)TimeSpan.FromMinutes(60).TotalSeconds),
                cancellationToken: token);

            _watcher = response.Watch<{{crdname}}Resource, object>(
                onEvent: async (type, item) => await OnEventHandlerAsync(type, item, token),
                onClosed: () =>
                {
                    _watcher.Dispose();
                    _ = StartWatcher(token);
                },
                onError: e => _diagnostics.WatcherThrow(e));
        }
        private async Task OnEventHandlerAsync(WatchEventType type, {{crdname}}Resource item, CancellationToken token = default)
        {
            if (type == WatchEventType.Added)
            {
                _diagnostics.OperatorEventAdded();

                await _controller.Do(item);
            }
            if (type == WatchEventType.Deleted)
            {
                _diagnostics.OperatorEventDeleted();

                await _controller.UnDo(item);
            }
        }
    }
}
