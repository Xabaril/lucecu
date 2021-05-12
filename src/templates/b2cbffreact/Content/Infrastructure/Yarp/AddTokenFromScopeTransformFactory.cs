using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using Yarp.ReverseProxy.Abstractions.Config;

namespace __ProjectName__.Infrastructure.Yarp
{
    public class AddTokenFromScopeSectionTransormFactory
        : ITransformFactory

    {
        private const string SET_TOKEN_TRANSFORM_NAME = "AddTokenFromScopeSection";
        private readonly ILogger<AddTokenFromScopeSectionTransormFactory> _logger;

        public AddTokenFromScopeSectionTransormFactory(ILogger<AddTokenFromScopeSectionTransormFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            _logger.LogDebug($"Build transformations with {nameof(AddTokenFromScopeSectionTransormFactory)} transformer.");

            var configuration = context.Services.GetService<IConfiguration>();
            var sectionName = transformValues[SET_TOKEN_TRANSFORM_NAME];
            var section = configuration.GetSection(sectionName);

            if (section != null)
            {
                _logger.LogDebug($"The transformer {nameof(AddTokenFromScopeSectionTransormFactory)} read the configured section {sectionName} to find the cluster {context.Cluster.Id} scope.");

                var scope = section.GetValue<string>(context.Cluster.Id);

                _logger.LogDebug($"The transformer {nameof(AddTokenFromScopeSectionTransormFactory)} found the scope {scope} for cluster {context.Cluster.Id}.");

                context.AddRequestTransform(async transformContext =>
                {
                    var accessor = transformContext.HttpContext.RequestServices.GetService<IHttpContextAccessor>();
                    var tokenAcquisition = transformContext.HttpContext.RequestServices.GetService<ITokenAcquisition>();

                    _logger.LogDebug($"The transformer {nameof(AddTokenFromScopeSectionTransormFactory)} is applying the transformation for scope {scope} on path {accessor.HttpContext.Request.Path}.");

                    if (accessor.HttpContext != null && accessor.HttpContext.User.Identity.IsAuthenticated)
                    {
                        try
                        {
                            var token = await tokenAcquisition.GetAccessTokenForUserAsync(new List<string>() { scope });

                            if (token is object)
                            {
                                transformContext.ProxyRequest.Headers.Add("authorization", $"Bearer {token}");
                            }
                            else
                            {
                                _logger.LogWarning($"The toke acquisition for scope {scope} can't be acquired property.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Token Acquisition failed on {nameof(AddTokenFromScopeSectionTransormFactory)} .", ex);
                        }
                    }
                });
            }

            return true;
        }

        public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.ContainsKey(SET_TOKEN_TRANSFORM_NAME))
            {
                if (transformValues.Count > 1)
                {
                    context.Errors.Add(new InvalidOperationException("The transform contains more parameters than expected: " + string.Join(';', transformValues.Keys)));
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
