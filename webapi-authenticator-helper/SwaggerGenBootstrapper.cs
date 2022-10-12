using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace webapi_authenticator_helper
{
    public class SwaggerGenBootstrapper
    {
        public static void BasicAuthenticationSwaggerSetupAction(SwaggerGenOptions options, string description = "Enter your username and password information here.")
        {
            options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Description = description,
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "Basic",
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id="basic"}
                    },
                    new List<string>()
                }
            });
        }

        public static void BearerAuthenticationSwaggerSetupAction(SwaggerGenOptions options, string description = "Enter your jwt token information here.(example; HJkasd6HAS6...)")
        {
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Description = description,
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id="bearer"}
                    },
                    new List<string>()
                }
            });
        }
    }
}
