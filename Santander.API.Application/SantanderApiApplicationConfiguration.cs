using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Santander.API.Application.Configuration;
using Santander.API.Application.Queries.GetBestStories;

namespace Santander.API.Application
{
    public static class SantanderApiApplicationConfiguration
    {
        public static IServiceCollection AddHandlers(
            this IServiceCollection services)
        {
            return services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<GetBestStoriesQueryHandler>();

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
        }

        public static IServiceCollection AddValidators(
            this IServiceCollection services)
        {
            return services.AddValidatorsFromAssemblyContaining<GetBestStoriesQueryValidator>();
        }

        public static IServiceCollection AddApplicationConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var bestStoriesConfiguration = new BestStoriesConfiguration();
            configuration.GetSection(nameof(BestStoriesConfiguration)).Bind(bestStoriesConfiguration);
            services.AddSingleton(typeof(IBestStoriesConfiguration), bestStoriesConfiguration);

            return services;
        }
    }
}
