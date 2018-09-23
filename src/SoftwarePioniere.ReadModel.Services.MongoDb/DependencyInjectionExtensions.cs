using Microsoft.Extensions.DependencyInjection;
using System;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.ReadModel;
using SoftwarePioniere.ReadModel.Services.MongoDb;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMongoDbEntityStore(this IServiceCollection services) =>
            services.AddMongoDbEntityStore(_ => { });

        public static IServiceCollection AddMongoDbEntityStore(this IServiceCollection services, Action<MongoDbOptions> configureOptions)
        {

            services
                .AddOptions()
                .Configure(configureOptions);

            //var settings = services.BuildServiceProvider().GetService<IOptions<AzureCosmosDbOptions>>().Value;

            services
                .AddSingleton<MongoDbConnectionProvider>()
                .AddSingleton<IEntityStoreConnectionProvider>(provider => provider.GetRequiredService<MongoDbConnectionProvider>())
                .AddSingleton<IEntityStore>(provider =>
                    {

                        var options = provider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
                        options.CacheClient = provider.GetRequiredService<ICacheClient>();
                        options.LoggerFactory = provider.GetRequiredService<ILoggerFactory>();

                        return new MongoDbEntityStore(options, provider.GetRequiredService<MongoDbConnectionProvider>());

                    }
);

            return services;
        }
    }
}
