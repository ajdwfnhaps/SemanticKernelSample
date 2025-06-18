using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Baodian.AI.SemanticKernel.Milvus.Configuration;
using Baodian.AI.SemanticKernel.Milvus.Services;

namespace Baodian.AI.SemanticKernel.Milvus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMilvus(this IServiceCollection services, MilvusOptions options)
        {
            services.AddSingleton(options);


            services.AddSingleton<LoggingService>();
            services.AddSingleton<RetryService>();

            services.AddSingleton(p =>
            {
                return new CollectionService(options);
            });

            services.AddSingleton(p =>
            {
                return new SearchService(options);
            });


            services.AddSingleton(p =>
            {
                return new DataService(options);
            });


            services.AddSingleton(p =>
            {
                return new IndexService(options);
            });


            return services;
        }

        public static IServiceCollection AddMilvus(this IServiceCollection services, Action<MilvusOptions> configure)
        {
            var options = new MilvusOptions();
            configure(options);
            return services.AddMilvus(options);
        }

        public static IServiceCollection AddMilvusWithLogging(this IServiceCollection services, Action<MilvusOptions> configure, Action<ILoggingBuilder> configureLogging)
        {
            services.AddLogging(configureLogging);
            return services.AddMilvus(configure);
        }
    }
} 