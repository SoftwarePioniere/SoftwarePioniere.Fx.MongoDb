using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.ReadModel.Tests;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.ReadModel.Services.MongoDb.Tests
{
    [Collection("MongoDbCollection")]
    public class MongoDbConnectionProviderTests  : TestBase
    {
        
        private MongoDbConnectionProvider CreateProvider()
        {
            return GetService<MongoDbConnectionProvider>();
        }

        public MongoDbConnectionProviderTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddMongoDbEntityStore(options => Configurator.Instance.ConfigurationRoot.Bind("MongoDb", options));
        }

        [Fact]
        public async Task CanConnectToClient()
        {
            var provider = CreateProvider();
            await provider.Client.Value.ListDatabasesAsync();
        }

        [Fact]
        public async Task CanClearDatabase()
        {
            var provider = CreateProvider();

            await provider.Database.Value.CreateCollectionAsync(Guid.NewGuid().ToString());
            (await provider.CheckDatabaseExists()).Should().BeTrue();

            await provider.ClearDatabaseAsync();
            (await provider.CheckDatabaseExists()).Should().BeFalse();

            await provider.Database.Value.CreateCollectionAsync(Guid.NewGuid().ToString());
            (await provider.CheckDatabaseExists()).Should().BeTrue();

        }
    }
}
