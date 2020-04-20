using CityWeatherOperator.Crd;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System;

namespace CityWeatherOperator.Diagnostics
{
    public class OperatorDiagnostics
    {
        private readonly ILogger _logger;

        public OperatorDiagnostics(ILoggerFactory loggerFactory)
        {
            _ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger("CityWeatherOperator");
        }

        public void OperatorStarting()
        {
            _logger.LogInformation("The operator is starting.");
        }

        public void OperatorShuttingdown()
        {
            _logger.LogInformation("The operator is shuttingdown.");
        }

        public void OperatorThrow(Exception exception)
        {
            _logger.LogError(exception, "The operator throw an unhandled exception on start.");
        }

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

        public void OperatorEventAdded()
        {
            _logger.LogInformation("Operator event Added.");
        }

        public void OperatorEventDeleted()
        {
            _logger.LogInformation("Operator event Deleted.");
        }

        public void WatcherThrow(Exception exception)
        {
            _logger.LogError(exception, "Watcher is throwing an error.");
        }
    }
}
