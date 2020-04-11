using k8s;
using Microsoft.Extensions.DependencyInjection;
using __ProjectName__.Controller;
using __ProjectName__.Diagnostics;
using __ProjectName__.Operator;
using __ProjectName__.Operator.Abstractions;
using Serilog;
using Serilog.Events;
using System;
using System.Threading;

namespace OperatorPattern
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = CreateDefaultServiceProvider();

            var diagnostics = serviceProvider.GetRequiredService<OperatorDiagnostics>();
            var @operator = serviceProvider.GetRequiredService<IKubernetesOperator>();

            var cancelTokenSource = new CancellationTokenSource();

            diagnostics.OperatorStarting();

            _ = @operator.RunAsync(cancelTokenSource.Token);

            var reset = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (s, a) =>
            {
                diagnostics.OperatorShuttingdown();
                @operator.Dispose();
                cancelTokenSource.Cancel();

                reset.Set();
            };

            reset.Wait();
        }

        private static IServiceProvider CreateDefaultServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .Enrich.WithProperty("Application", nameof(__ProjectName__Operator))
                    .Enrich.FromLogContext()
                    .WriteTo.ColoredConsole()
                    .CreateLogger();

                builder.AddSerilog(logger, dispose: true);
            });

            services.AddSingleton<IKubernetes>(sp =>
            {
                var config = KubernetesClientConfiguration.IsInCluster() ? KubernetesClientConfiguration.InClusterConfig() : KubernetesClientConfiguration.BuildConfigFromConfigFile();

                return new Kubernetes(config);
            });

            services.AddSingleton<OperatorDiagnostics>();
            services.AddTransient<IKubernetesOperator, __ProjectName__Operator>();
            services.AddTransient<I__ProjectName__Controller, __ProjectName__Controller>();

            return services.BuildServiceProvider();
        }
    }
}
