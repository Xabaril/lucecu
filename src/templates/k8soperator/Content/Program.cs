using __ProjectName__.Controller;
using __ProjectName__.Diagnostics;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;

namespace __ProjectName__
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build();

            var diagnostics = host.Services
                .GetRequiredService<OperatorDiagnostics>();

            try
            {
                host.Run();
            }
            catch(Exception exception)
            {
                diagnostics.OperatorThrow(exception);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Operator>();
                services.AddSingleton<IKubernetes>(sp =>
                {
                    var config = KubernetesClientConfiguration.IsInCluster() ? KubernetesClientConfiguration.InClusterConfig() : KubernetesClientConfiguration.BuildConfigFromConfigFile();

                    return new Kubernetes(config);
                });
                services.AddSingleton<OperatorDiagnostics>();
                services.AddTransient<IHealthCheckOperatorController, HealthCheckOperatorController>();
            })
            .ConfigureLogging((cxt, builder) =>
            {
                var logger = new LoggerConfiguration()
                   .MinimumLevel.Information()
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                   .Enrich.WithProperty("Application", nameof(Operator))
                   .Enrich.FromLogContext()
                   .WriteTo.ColoredConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception:lj}")
                   .CreateLogger();

                builder.ClearProviders();
                builder.AddSerilog(logger, dispose: true);
            });
    }
}
