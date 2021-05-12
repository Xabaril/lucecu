![Lucecu CD Build](https://github.com/Xabaril/lucecu/workflows/Lucecu%20CD%20Build/badge.svg)

![Lucecu CI Build](https://github.com/xabaril/Lucecu/workflows/Lucecu%20CI%20Build/badge.svg?branch=master)

# About Lucecú

*Lucecú* is the Xabaril repository for custom DotNet templates. Our goal is to simplify the initial scaffolding of some scenarios where DotNet does not have a template yet.

At the moment, the templates included in this repository are:

- [K8S Operator pattern](#k8s-operator-pattern-template)
- [AzureB2C BFF](#b2c-bff-template-for-react)

## How to build

*Lucecú* is built against .NET COre
- Run dotnet new -i ./src/templates/XXX in the root of the repo for building XXX template.

## K8S Operator pattern template

This template creates the default scaffolding project to create a Kubernetes operator, including:

- Project scaffolding
- A Simple CRD
- YAML templates for deploy CRD
- A CRD watcher
- A Kubernetes controller with Kubernetes client dependency

 To get more information, please checkout our [Getting Starting with K8S Operator template](./docs/GettingStarted-K8SOperator.md)


## B2C BFF template fore React

This template creates the default scaffolding project ofr a React SPA application with Backend for FrontEnd using YARP reverse proxy.

To get more information, please checkout our ....

## Acknowledgements
*Lucecú* is built using the following great open source projects and free services:

- [ASP.NET Core](https://github.com/aspnet)
- [Serilog](https://github.com/serilog/serilog)

..and last but not least a big thanks to all our [contributors](https://github.com/Xabaril/Lucecu/graphs/contributors)!

## Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
