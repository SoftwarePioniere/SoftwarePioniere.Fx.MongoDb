using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.ReadModel.Tests;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.ReadModel.Services.MongoDb.Tests
{
    [Collection("MongoDbCollection")]
    public class MongoDbEntityStoreTests  : EntityStoreTestsBase
    {
        public MongoDbEntityStoreTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddMongoDbEntityStore(options => Configurator.Instance.ConfigurationRoot.Bind("MongoDb", options));
        }

        [Fact]
        public override Task CanInsertAndDeleteItem()
        {
            return base.CanInsertAndDeleteItem();
        }

        [Fact]
        public override Task CanInsertAndUpdateItem()
        {
            return base.CanInsertAndUpdateItem();
        }

        [Fact]
        public override Task CanInsertItem()
        {
            return base.CanInsertItem();
        }

        [Fact]
        public override void DeleteThrowsErrorWithKeyNullOrEmpty()
        {
            base.DeleteThrowsErrorWithKeyNullOrEmpty();
        }

        [Fact]
        public override void LoadItemThrowsErrorWithKeyNullOrEmpty()
        {
            base.LoadItemThrowsErrorWithKeyNullOrEmpty();
        }

        [Fact]
        public override Task LoadItemsWithPagingWorks()
        {
            return base.LoadItemsWithPagingWorks();
        }

        [Fact]
        public override Task LoadItemsWithWhereWorks()
        {
            return base.LoadItemsWithWhereWorks();
        }

        [Fact]
        public override Task SaveAndLoadItemPropertiesEquals()
        {
            return base.SaveAndLoadItemPropertiesEquals();
        }

        [Fact]
        public override Task SaveAndLoadItemsContainsAll()
        {
            return base.SaveAndLoadItemsContainsAll();
        }

        [Fact]
        public override Task SaveAndUpdateItemPropertiesEquals()
        {
            return base.SaveAndUpdateItemPropertiesEquals();
        }

        [Fact]
        public override void SaveThrowsErrorWithItemNull()
        {
            base.SaveThrowsErrorWithItemNull();
        }

        [Fact]
        public override Task CanInsertManyItems()
        {
            return base.CanInsertManyItems();
        }
    }
}
