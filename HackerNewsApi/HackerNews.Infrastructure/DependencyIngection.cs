using HackerNews.Application.Interfaces;
using HackerNews.Infrastructure.HackerNews;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackerNews.Infrastructure
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

            services.AddHttpClient<HackerNewsGateway>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<HackerNewsClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSecond);
            }).AddStandardResilienceHandler();

            services.AddSingleton<IHackerNewsGateway>(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<HackerNewsGateway>();
                var cache = serviceProvider.GetRequiredService<IMemoryCache>();
                var cachingOptions = serviceProvider.GetRequiredService<IOptions<HackerNewsCachingOptions>>();
                var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
                return new CachingHackerNewsGateway(client, memoryCache, cachingOptions.Value);
            });

            return services;
        }
    }
}
