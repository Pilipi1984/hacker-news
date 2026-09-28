using HackerNews.Application.Common;
using HackerNews.Application.Interfaces;
using HackerNews.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HackerNews.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BestStoriesOptions>()
                .Bind(configuration.GetSection(BestStoriesOptions.SectionName));

            services.AddScoped<IBestStoriesService, BestStoriesService>();

            return services;
        }
    }
}
