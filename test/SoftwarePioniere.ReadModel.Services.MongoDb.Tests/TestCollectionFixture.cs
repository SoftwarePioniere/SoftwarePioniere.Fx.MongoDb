using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoftwarePioniere.ReadModel.Services.MongoDb.Tests
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestCollectionFixture : IDisposable
    {
        public void Dispose()
        {
            var services = new ServiceCollection();
            services
                .AddOptions()
                .AddSingleton<ILoggerFactory>(new NullLoggerFactory())
                .AddMongoDbEntityStore(options => new TestConfiguration().ConfigurationRoot.Bind("MongoDb", options));

            var provider = services.BuildServiceProvider().GetService<MongoDbConnectionProvider>();
            provider.ClearDatabaseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}