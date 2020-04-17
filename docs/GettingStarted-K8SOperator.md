# Getting started

*Lucecú K8S Operator* is a DotNet template to create a default scaffolding for Kubernetes operators on DotNet/C#.

> You can check more information about Kubernetes operators on the [Oficial Documentation](https://kubernetes.io/docs/concepts/extend-kubernetes/operator/).

## Install template

To install this template, use the following command:

```shell
> dotnet new -i Xabaril.Lucecu.K8SOperator
```
You can verify that it has been successfully installed checking the command output or using the command:

```shell
> dotnet new --list
```

## Create a new project

Once the template is installed you can use it to create a new projects for Kubernetes Operator as follows:

```shell
> dotnet new k8soperator --name HealthChecksOperator --crd-name HealthCheck --crd-short-name hc
```

Where

- *name* is the name of the project to be created.
- *crd-name* is the name of the new Kubernetes CRD, you can write on CamelCase, it is lowered on yaml templates.
- *crd-short-name* is the short name of the new Kubernetes CRD (as svc is the short name for services), it is lowered on yaml templates.

> Optionally you can add:

- *crd-plural-name* is the plural name of the new Kubernetes CRD, you can write on CamelCase, it is lowered on yaml templates. If empty, it will be obtained pluralizing `crd-name`.
- *crd-group-name* parameter. By default, `xabaril.io` will be used.
- *operator-name* with the name of your operator. This name is used to generate the YAML files to deploy the operator on Kubernetes. Defaults to `name` parameter value
- *image-name* parameter with the docker image name of your operator. Defaults to same `operator-name` parameter value.

## Installing CRD

The new project includes a *Deployment* folder with the new CRD deployment manifest, like this:

```yaml
apiVersion: apiextensions.k8s.io/v1beta1
kind: CustomResourceDefinition
metadata:
  name: healthchecks.xabaril.io
spec:
  group: xabaril.io
  names:
    plural: healthchecks
    singular: healthcheck
    kind: healthcheck
    listKind: healthchecks
    shortNames:
      - hc
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
# include here more properties for your custom CRD and 
# modify the {{crdName}}ResourceSpec.cs to represent this properties
            someproperty:
              type: string
          required:
            - someproperty
```

By default, the new CRD includes a property with name **someproperty** of type string, but you can add here new properties, and modify the generated class [CRDName]ResourceSpec.cs to add also your new properties.

To create this new CRD on Kubernetes, execute the following command:

```shell
> kubectl apply -f ./HealthCheck_crd_definition.yaml
```
If the command is working properly, it will show an output similar to:

```shell
customresourcedefinition.apiextensions.k8s.io/healthchecks.xabaril.io created
```
To verify if Kubernetes understands the new CRD, you can execute some of the next commands:

```shell
> kubectl get healthchecks
```
```shell
> kubectl get healthcheck
```
```shell
> kubectl get hc
```

## Deploying the operator

Template includes a set of basic YAML files to deploy the operator itself in the cluster. Following kubernetes objects are generated:

* A deployment to install the operator
* A configmap to allow configuration of the operator
* RBAC configuration (role, rolebinding and serviceaccount)

All these files are generated inm the `Deployment` folder of the generated project.

## Debugging CRD

You can debug the Kubernetes operator like a regular project, the Kubernetes client uses the cluster config or user config depending where the operator is executed.

```csharp
services.AddSingleton<IKubernetes>(sp =>
{
    var config = KubernetesClientConfiguration.IsInCluster() 
    ? KubernetesClientConfiguration.InClusterConfig() 
    : KubernetesClientConfiguration.BuildConfigFromConfigFile();

    return new Kubernetes(config);
});
```

While the project is on debugging, add a new breakpoint on the method *OnEventHandlerAsync*, inside the operator class HealthChecksOperatorOperator.cs.

Now deploy a new CRD object using the file '/deployment/create_new_healthcheck.yaml';

```shell
❯ kubectl apply -f [your-registry]/[your-operator-image-name]
Sending build context to Docker daemon  139.8kB
```

This will emmit a new WatchEvent that will activate the breakpoint.

![Debugging Operator](./images/debug_operator.png)

## Deploy Operator

The operator project contains a Dockerfile to build the image of the operator. You can use this file to build your operator image:

```shell
❯ docker build . -t [your-registry]/[your-operator-image-name]
Sending build context to Docker daemon  139.8kB
```

When the image is build, you can push the operator image to your private or public repository

```shell
❯ docker push [your-registry]/[your-operator-image-name]
The push refers to repository [XXX]
```

## Delete CRD

To delete created CRD get the name and delete it:

```shell
❯ kubectl get crds
NAME                                    CREATED AT
healthchecks.xabaril.io                 2020-04-10T15:34:54Z
```

```shell
❯ kubectl delete crd healthchecks.xabaril.io
customresourcedefinition.apiextensions.k8s.io "healthchecks.xabaril.io" deleted
```