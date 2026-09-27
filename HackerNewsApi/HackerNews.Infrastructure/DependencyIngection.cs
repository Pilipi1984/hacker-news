using HackerNews.Infrastructure.HackerNews;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackerNewsApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddOptions<HackerNewsClientOptions>()
                .Bind(configuration.GetSection(HackerNewsClientOptions.SectionName));

            services.AddOptions<HackerNewsCachingOptions>()
                .Bind(configuration.GetSection(HackerNewsCachingOptions.SectionName));

            services.AddMemoryCache();

            services.AddHttpClient<http

            return services;
        }
    }
}
