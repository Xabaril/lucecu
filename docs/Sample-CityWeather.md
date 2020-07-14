# Getting started

This sample will demostrate how to use *Lucecú K8S Operator* for deploying a small application that shows the weather forecast of a city.

## Template

- Install the template:

```shell
> dotnet new -i Xabaril.Lucecu.K8SOperator
```

- Create the new project name CityWeatherOperator:

```shell
> dotnet new k8soperator -n CityWeatherOperator -cn CityWeather -csn cw
```

## Custom Resource Definition

- For each city we want to get its weather forecast, we will deploy a `cityweather` CRD. In this definition so, we have to specify the city and optionally its replica count (number of pods to be assigned for showing its forecasts), by default it will be 1. Therefore, we will add two new parameters on the resource specification:

```csharp
namespace CityWeatherOperator.Crd
{
    public class CityWeatherResourceSpec
    {
        public string City { get; set; }
        public int? Replicas { get; set; }
    }
}
```

- As well as on the deployment yaml `deployment/CityWeather_crd_definition` related. The `city` parameter will be required and the `replicas`optional:

```yaml
apiVersion: apiextensions.k8s.io/v1beta1
kind: CustomResourceDefinition
metadata:
  name: cityweathers.xabaril.io
spec:
  group: xabaril.io
  names:
    plural: cityweathers
    singular: cityweather
    kind: cityweather
    listKind: cityweathers
    shortNames:
      - cw
  versions:
    - name: v1
      served: true
      storage: true
  scope: Namespaced
  validation:
    openAPIV3Schema:
      properties:
        spec:
          properties:
            city:
              type: string
            replicas:
              type: int
          required:
            - city
```

## Operator

- Let's implement now the logic of what the operator is responsible for. So first, we are going to edit the interface `ICityWeatherOperator.csproj` to change the returned types to Task.

```csharp
    public interface ICityWeatherOperatorController
    {
        Task Do(CityWeatherResource resource);

        Task UnDo(CityWeatherResource resource);
    }
```

- Now, on the class `CityWeatherOperatorController`, let's create a constant with the name of the application:

```csharp
        private const string _appName = "cityweather";
```

- Using the `kubernetesClient`, implement the code for deploying a service. We will deploy a ClusterIP service (by default). The service will affect to any pod labeled with the app `cityweather` but only with the specific city label.

```csharp
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
```

- And the deployment manifest. The deployment will refer to our example image (`xabarilcoding/cityweatherapi:latest`) that contains the forecasting API. This image is going to read it environment variable `Weather__City` for the city name:

```csharp
        private async Task<V1Deployment> DeployDeployment(CityWeatherResource resource)
        {
            var labels = new Dictionary<string, string>()
                {
                    { "app", _appName },
                    { "city", resource.Spec.City }
                };

            var deploymentBody = new V1Deployment(
                metadata: new V1ObjectMeta(
                    name: resource.Metadata.Name,
                    labels: labels),
                spec: new V1DeploymentSpec(
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

            return await _kubernetesClient.CreateNamespacedDeploymentAsync(deploymentBody, resource.Metadata.NamespaceProperty);
        }
```
        
- Also, we have to implement the related code to execute for undeploying:


```csharp
        private async Task<V1Status> UndeployService(CityWeatherResource resource)
        {
            return await _kubernetesClient.DeleteNamespacedServiceAsync(resource.Metadata.Name, resource.Metadata.NamespaceProperty);
        }

        private async Task<V1Status> UndeployDeployment(CityWeatherResource resource)
        {
            return await _kubernetesClient.DeleteNamespacedDeployment2Async(resource.Metadata.Name, resource.Metadata.NamespaceProperty);
        }
```

- Let's add some new methods for enriching our diagnostic info on `OperatorDiagnostics`class:

```csharp
        public void ControllerDeploying(CityWeatherResource resource)
        {
            _logger.LogInformation($"{resource.Metadata.Name} controller deploying...");
        }

        public void ControllerDeployed(CityWeatherResource resource, V1Service service, V1beta2Deployment deployment)
        {
            _logger.LogInformation($"{resource.Metadata.Name} service deployed on IP {service.Spec.ClusterIP}");
            _logger.LogInformation($"{resource.Metadata.Name} deployment deployed with {deployment.Spec.Replicas} replicas");
            _logger.LogInformation($"{resource.Metadata.Name} controller deployed");
        }

        public void ControllerUndeploying(CityWeatherResource resource)
        {
            _logger.LogInformation("CityWeatherOperator controller Undoing...");
        }

        public void ControllerUndeployed(CityWeatherResource resource)
        {
            _logger.LogInformation($"{resource.Metadata.Name} service deleted");
            _logger.LogInformation($"{resource.Metadata.Name} deployment deleted");
            _logger.LogInformation($"{resource.Metadata.Name} controller undeployed");
        }
```

- And call these methods on the `Do` method (when a new creation of a `cityweather`CRD has been detected):

```csharp
        public async Task Do(CityWeatherResource resource)
        {
            _diagnostics.ControllerDeploying(resource);
            var service = await DeployService(resource);
            var deployment = await DeployDeployment(resource);
            _diagnostics.ControllerDeployed(resource, service, deployment);
        }
```

- And the same for the Undo method, deleting the service and the deployment:

```csharp
        public async Task UnDo(CityWeatherResource resource)
        {
            _diagnostics.ControllerUndeploying(resource);
            await UndeployService(resource);
            await UndeployDeployment(resource);
            _diagnostics.ControllerUndeployed(resource);
        }
```

## Deploy

- Our code is ready now. Let's generate the image of the operator and push it to a repository we are going to us for pulling from our kubernetes cluster:

```bash
> docker build . -t [your-repository]/cityweatheroperator:latest
> docker push [your-repository]/cityweatheroperator:latest
```

- And addapt the operator deployment manifest with this image (`operator\13-operator.yaml`):

```yaml
    spec:
      serviceAccountName: cityweatheroperator
      containers:
        - name: cityweatheroperator
          image: tyesamples.azurecr.io/cityweatheroperator:latest
 ```

- So, the operator is ready for being deployed. First, we will deploy the CRD:

```shell
> kubectl apply -f Deployment\Crd\CityWeather_crd_definition.yaml
```

- Second, the related manifests of the operator, with:

```shell
> kubectl apply -f Deployment\Operator\
serviceaccount/cityweatheroperator created
clusterrole.rbac.authorization.k8s.io/cityweatheroperator created
clusterrolebinding.rbac.authorization.k8s.io/cityweatheroperator created
configmap/cityweatheroperator-config created
deployment.apps/cityweatheroperator created
```

- We can verify if out operator has been properly deployed checking that its deployment its pod ready:

```shell
> kubectl get deployments
```

```shell
> kubectl get pods
```

## Test

- After every `cityweather` CRD deploy, we want our operator deploy a new specific service and deployment.

- For our example, we are going to deploy an `cityweather` for Madrid and another for Rome. So, we rename and change the yaml file to `create_new_CityWeather_Madrid.yaml` :

```yaml
apiVersion: "xabaril.io/v1"
kind: cityweather
metadata:
  name: cityweather-madrid
spec:
  city: Madrid
  replicas: 2
```

- And the same for Rome in `create_new_CityWeather_Rome`:

```yaml
apiVersion: "xabaril.io/v1"
kind: cityweather
metadata:
  name: cityweather-rome
spec:
  city: Rome
 ```

- Let's apply these two manifests to our cluster:

```shell
> kubectl apply -f Deployment\Crd\create_new_CityWeather_Madrid.yaml
> kubectl apply -f Deployment\Crd\create_new_CityWeather_Rome.yaml
```

- Each CRD deploy will trigger the creation of a new service and a new deployment. We can check the new two services:

```shell
> kubectl get services
```

- And the new two deployments (`cityweather-madrid` will have 2 replicas, `cityweather-rome` only 1):

```shell
> kubectl get deployments
```

- Let's open the operator pod logs in order to check its diagnostic trace:

```shell
> kubectl get pods
```

```shell
> kubectl log cityweatheroperator-6b9785d776-prcn7
[11:28:03 INF] Operator event Added.
[11:28:03 INF] cityweather-madrid controller deploying...
[11:28:09 INF] cityweather-madrid service deployed on IP 10.0.68.206
[11:28:09 INF] cityweather-madrid deployment deployed with 2 replicas
[11:28:09 INF] cityweather-madrid controller deployed
[11:31:03 INF] Operator event Added.
[11:31:03 INF] cityweather-rome controller deploying...
[11:31:07 INF] cityweather-rome service deployed on IP 10.0.144.182
[11:31:07 INF] cityweather-rome deployment deployed with 1 replicas
[11:31:07 INF] cityweather-rome controller deployed
```

- To consume the API's, because of the fact that their services are deployed with `ClusterIP` type, we will have to previously map local ports to the services ports. Let's set the local `9000` for Madrid: 

```shell
> kubectl port-forward svc/cityweather-madrid 9000:80
Forwarding from 127.0.0.1:9000 -> 80
Forwarding from [::1]:9000 -> 80
```

- And on a different console, local port `9001` for Rome:

```shell
> kubectl port-forward svc/cityweather-rome 9001:80
Forwarding from 127.0.0.1:9001 -> 80
Forwarding from [::1]:9001 -> 80
```

- Open two browsers and check that the Api returns the expected results:

```shell
http://localhost:9000/weatherforecast
```

```shell
http://localhost:9001/weatherforecast
```

- Finally, let's delete the deployed CRD's to check that also this part is working:

```shell
> kubectl delete -f CityWeatherOperator\Deployment\Crd\create_new_CityWeather_Madrid.yaml
> kubectl delete -f CityWeatherOperator\Deployment\Crd\create_new_CityWeather_Rome.yaml
```

- Let's watch again the operator pod log:

```shell
> kubectl log cityweatheroperator-6b9785d776-prcn7
[11:33:08 INF] Operator event Deleted.
[11:33:10 INF] CityWeatherOperator controller Undoing...
[11:33:10 INF] cityweather-madrid service deleted
[11:33:10 INF] cityweather-madrid deployment deleted
[11:33:10 INF] cityweather-madrid controller undeployed
[11:33:43 INF] Operator event Deleted.
[11:33:46 INF] CityWeatherOperator controller Undoing...
[11:33:46 INF] cityweather-rome service deleted
[11:33:46 INF] cityweather-rome deployment deleted
[11:33:46 INF] cityweather-rome controller undeployed
```

- And list the deployments and services to verify that they have been deleted:

```shell
> kubectl get deployments
```

```shell
> kubectl get services
```

## Clean

- Let's clean of our cluster of the objects deployed:

```shell
kubectl delete -f Deployment\Operator\
```

```shell
> kubectl delete -f CityWeatherOperator\Deployment\Crd\CityWeather_crd_definition.yaml
```
