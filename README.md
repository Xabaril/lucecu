![Lucecu Build](https://github.com/xabaril/Lucecu/workflows/Lucecu%20CD%20Build/badge.svg?branch=master)

![Lucecu Build](https://github.com/xabaril/Lucecu/workflows/Lucecu%20CI%20Build/badge.svg?branch=master)

# About Lucecu

Lucecú is the Xabaril respository for custom DotNet templates. Our goal is simplify the initial scaffolding of some scenarios where DotNet does not have a template yet.

At this momment, the templates included in this repository are:

## K8S Operator pattern template

This template create de default scaffolding project to create a Kubernetes operator,  including:

 - Project scaffolding
 - A Simple CRD
 - YAML templates for deploy CRD
 - A CRD watcher
 - A Kubernetes controller with Kubernetes client dependency


 To get more information, please checkout our [Getting Starting with K8S Operator teamplte](./docs/GettingStarted-K8SOperator.md)


 ## How to build
Lucecu is built against the latest NET Core 3.

* [Install](https://www.microsoft.com/net/download/core#/current) the [required](https://github.com/Xabaril/Esquio/blob/master/global.json) .NET Core SDK
* Run dotnet new -i ./src/templates/k8soperator in the root of the repo.

## Acknowledgements
Esquio is built using the following great open source projects and free services:

* [ASP.NET Core](https://github.com/aspnet)
* [Serilog](https://github.com/serilog/serilog)

..and last but not least a big thanks to all our [contributors](https://github.com/Xabaril/Lucecu/graphs/contributors)!

## Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
