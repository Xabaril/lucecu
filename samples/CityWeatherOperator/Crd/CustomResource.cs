using k8s;
using k8s.Models;

namespace CityWeatherOperator.Crd
{
    public abstract class CustomResource : KubernetesObject
    {
        public V1ObjectMeta Metadata { get; set; }
    }

    public abstract class CustomResource<TSpec, TStatus> : CustomResource
    {
        public TSpec Spec { get; set; }
        public TStatus Status { get; set; }
    }
}
