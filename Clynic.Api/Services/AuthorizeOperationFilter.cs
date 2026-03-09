using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Clynic.Api.Services
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var declaringType = method.DeclaringType;

            var hasAllowAnonymous = method.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                || (declaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

            if (hasAllowAnonymous)
                return;
        

            var hasAuthorize = method.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                || (declaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false);

            if (!hasAuthorize)
                return;
            

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }
}
