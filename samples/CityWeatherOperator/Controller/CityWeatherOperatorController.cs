using k8s;
using CityWeatherOperator.Crd;
using CityWeatherOperator.Diagnostics;
using System;
using System.Threading.Tasks;
using k8s.Models;
using System.Collections.Generic;

namespace CityWeatherOperator.Controller
{
    public class CityWeatherOperatorController
        : ICityWeatherOperatorController
    {
        private readonly IKubernetes _kubernetesClient;
        private readonly OperatorDiagnostics _diagnostics;
        private const string _appName = "cityweather";

        public CityWeatherOperatorController(
            IKubernetes kubernetesClient,
            OperatorDiagnostics diagnostics)
        {
            _kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public async Task Do(CityWeatherResource resource)
        {
            _diagnostics.ControllerDeploying(resource);
            var service = await DeployService(resource);
            var deployment = await DeployDeployment(resource);
            _diagnostics.ControllerDeployed(resource, service, deployment);
        }

        private async Task<V1Service> DeployService(CityWeatherResource resource)
        {
            var labels = new Dictionary<string, string>()
                {
                    { "app",  _appName},
                    { "city", resource.Spec.City }
                };

            var serviceBody = new V1Service(
                metadata: new V1ObjectMeta(
                    name: resource.Metadata.Name,
                    labels: labels),
                spec: new V1ServiceSpec(
                    ports: new List<V1ServicePort>()
                        { new V1ServicePort(port: 80, targetPort: "http", protocol: "TCP", name: "http") },
                    selector: labels
                )
             );

            return await _kubernetesClient.CreateNamespacedServiceAsync(serviceBody, resource.Metadata.NamespaceProperty);
        }

        private async Task<V1beta2Deployment> DeployDeployment(CityWeatherResource resource)
        {
            var labels = new Dictionary<string, string>()
                {
                    { "app", _appName },
                    { "city", resource.Spec.City }
                };

            var deploymentBody = new V1beta2Deployment(
                metadata: new V1ObjectMeta(
                    name: resource.Metadata.Name,
                    labels: labels),
                spec: new V1beta2DeploymentSpec(
                    selector: new V1LabelSelector(matchLabels: labels),
                    replicas: resource.Spec.Replicas,
                    template: new V1PodTemplateSpec(
                        metadata: new V1ObjectMeta(labels: labels),
                        spec: new V1PodSpec(
                            containers: new List<V1Container>() {
                                new V1Container(
                                    name: resource.Metadata.Name,
                                    env: new List<V1EnvVar>(){ new V1EnvVar("Weather__City", resource.Spec.City) },
                                    imagePullPolicy: "IfNotPresent",
                                    image: "xabarilcoding/cityweatherapi:latest",
                                    ports: new List<V1ContainerPort>(){
                                        new V1ContainerPort(containerPort: 80, protocol: "TCP", name: "http")
                                    })
                            }))));

            return await _kubernetesClient.CreateNamespacedDeployment2Async(deploymentBody, resource.Metadata.NamespaceProperty);
        }

        public async Task UnDo(CityWeatherResource resource)
        {
            _diagnostics.ControllerUndeploying(resource);
            await UndeployService(resource);
            await UndeployDeployment(resource);
            _diagnostics.ControllerUndeployed(resource);
                
        }

        private async Task<V1Status> UndeployService(CityWeatherResource resource)
        {
            return await _kubernetesClient.DeleteNamespacedServiceAsync(resource.Metadata.Name, resource.Metadata.NamespaceProperty);
        }

        private async Task<V1Status> UndeployDeployment(CityWeatherResource resource)
        {
            return await _kubernetesClient.DeleteNamespacedDeployment2Async(resource.Metadata.Name, resource.Metadata.NamespaceProperty);
        }

    }
}