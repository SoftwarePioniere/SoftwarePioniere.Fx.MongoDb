using Microsoft.Extensions.DependencyInjection;
using System;

namespace SoftwarePioniere.ReadModel.Services.MongoDb
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
                .AddSingleton<IEntityStore, MongoDbEntityStore>();

            return services;
        }
    }
}
