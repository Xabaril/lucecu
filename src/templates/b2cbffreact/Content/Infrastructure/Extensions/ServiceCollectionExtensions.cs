using __ProjectName__.Infrastructure.Yarp;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Abstractions.Config;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBffReverseProxy(this IServiceCollection services, IConfiguration configuration)
        {
            const string ReverseProxy = nameof(ReverseProxy);

            services.AddHttpContextAccessor()
                .AddReverseProxy()
                .LoadFromConfig(configuration.GetSection(ReverseProxy));

            services.AddSingleton<ITransformFactory, AddTokenFromScopeSectionTransormFactory>();

            return services;
        }
    }
}
