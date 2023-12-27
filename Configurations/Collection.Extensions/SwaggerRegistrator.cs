using System.Reflection;
using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace Suspense.Server.Configurations.Collection.Extensions;

/// <summary>
/// Provides a static class for registering Swagger documentation in an ASP.NET Core application.
/// </summary>
public static class SwaggerRegistrator
{
    /// <summary>
    /// Registers Swagger documentation generation for the specified API version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to configure Swagger services.</param>
    /// <param name="major">The API Major version to generate Swagger documentation for.</param>
    /// <param name="minor">The API Minor version to generate Swagger documentation for.</param>
    public static void AddSwagger(this IServiceCollection services, int major, int minor)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc($"v{major}", new OpenApiInfo
            {
                Version = $"v{major}",
                Title = "Suspense",
                Description = "A card game variation of Crazy Eights and Switch.",
                License = new OpenApiLicense
                {
                    Name = "GNU General Public License version 3",
                    Url = new Uri("https://opensource.org/license/gpl-3-0/")
                }
            });

            // Setup swagger to display XML comments
            // To use this you must first enable the "Generate XML Documentation" option on the project
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        // Add swagger versioning
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(major, minor);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
            config.ApiVersionReader = ApiVersionReader.Combine(
                new HeaderApiVersionReader("x-api-version"),
                new QueryStringApiVersionReader("api-version")
            );
        });
    }
}