using Microsoft.Extensions.Logging;
using System;

namespace __ProjectName__.Diagnostics
{
    public class OperatorDiagnostics
    {
        private readonly ILogger _logger;

        public OperatorDiagnostics(ILoggerFactory loggerFactory)
        {
            _ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger("__ProjectName__");
        }


        public void OperatorStarting()
        {
            _logger.LogInformation("The operator is starting!");
        }

        public void OperatorShuttingdown()
        {
            _logger.LogInformation("The operator is shuttingdown!");
        }

        public void ControllerDo()
        {
            _logger.LogInformation("__ProjectName__ controller Do!");
        }

        public void ControllerUnDo()
        {
            _logger.LogInformation("__ProjectName__ controller UnDo!");
        }

        public void OperatorEventAdded()
        {
            _logger.LogInformation("Operator event Added");
        }

        public void OperatorEventDeleted()
        {
            _logger.LogInformation("Operator event Deleted");
        }

        public void WatcherThrow(Exception exception)
        {
            _logger.LogError(exception, "{{crd-name}} CRD is deleted!");
        }
    }
}
