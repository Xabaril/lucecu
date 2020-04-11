using k8s;
using __ProjectName__.Crd;
using __ProjectName__.Diagnostics;
using System;
using System.Threading.Tasks;

namespace __ProjectName__.Controller
{
    public class __ProjectName__Controller
        : I__ProjectName__Controller
    {
        private readonly IKubernetes _kubernetesClient;
        private readonly OperatorDiagnostics _diagnostics;

        public __ProjectName__Controller(
            IKubernetes kubernetesClient,
            OperatorDiagnostics diagnostics)
        {
            _kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public ValueTask Do({{crdname}}Resource resource)
        {
            _diagnostics.ControllerDo();

            return new ValueTask();
        }

        public ValueTask UnDo({{crdname}}Resource resource)
        {
            _diagnostics.ControllerUnDo();

            return new ValueTask();
        }
    }
}
