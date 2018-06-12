using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SoftwarePioniere.ReadModel.Services.MongoDb
{
    public class MongoDbEntityStore : EntityStoreBase
    {
        private readonly MongoDbConnectionProvider _provider;

        public MongoDbEntityStore(ILoggerFactory loggerFactory, ICacheClient cacheClient,
            MongoDbConnectionProvider provider) : base(loggerFactory, cacheClient)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public override async Task<T[]> LoadItemsAsync<T>()
        {
            Logger.LogDebug("LoadItemsAsync: {EntityType}", typeof(T));

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<MongoEntity<T>>(x => x.Entity.EntityType == _provider.KeyCache.GetEntityTypeKey<T>());

            var items = await collection.FindAsync(filter);

            var ret = new List<T>();

            while (await items.MoveNextAsync())
            {
                ret.AddRange(items.Current.Select(x => x.Entity));
            }

            return ret.ToArray();
        }

        public override async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> predicate)
        {
            Logger.LogDebug("LoadItemsAsync: {EntityType} {Expression}", typeof(T), predicate);

            //TODO: echten filter einbauen
            var items = await LoadItemsAsync<T>();
            return items.AsQueryable().Where(predicate).ToArray();
        }

        public override async Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms)
        {
            if (parms == null)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadPagedResultAsync: {EntityType} {Paramter}", typeof(T), parms);
            }

            var items = (await LoadItemsAsync<T>()).AsQueryable();

            if (parms.Where != null)
            {
                items = items.Where(parms.Where);
            }

            if (parms.OrderByDescending != null)
            {
                items = items.OrderByDescending(parms.OrderByDescending);
            }

            if (parms.OrderBy != null)
            {
                items = items.OrderBy(parms.OrderBy);
            }

            return items.GetPagedResults(parms.PageSize, parms.Page);
        }

        protected override async Task InternalDeleteItemAsync<T>(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);
            }

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<MongoEntity<T>>(x => x._id == entityId);
            await collection.DeleteOneAsync(filter);
        }

        protected override async Task InternalInsertItemAsync<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            var collection = _provider.GetCol<T>();
            await collection.InsertOneAsync(new MongoEntity<T> { _id = item.EntityId, Entity = item }).ConfigureAwait(false);
        }

        protected override async Task InternalInsertOrUpdateItemAsync<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalInsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            var collection = _provider.GetCol<T>();

            var filter = new ExpressionFilterDefinition<MongoEntity<T>>(x => x.Entity.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x._id == item.EntityId);

            var exi = await collection.FindAsync(filter);

            if (await exi.MoveNextAsync())
            {
                await UpdateItemAsync(item).ConfigureAwait(false);
            }
            else
            {
                await InsertItemAsync(item).ConfigureAwait(false);
            }
        }

        protected override async Task<T> InternalLoadItemAsync<T>(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);
            }

            var collection = _provider.GetCol<T>();

            var filter = new ExpressionFilterDefinition<MongoEntity<T>>(x => x.Entity.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x._id == entityId);

            var exi = await collection.FindAsync(filter);

            if (await exi.MoveNextAsync())
            {
                var cu = exi.Current.FirstOrDefault();
                if (cu != null)
                    return cu.Entity;
            }

            return null;
        }

        protected override async Task InternalUpdateItemAsync<T>(T item)
        {

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<MongoEntity<T>>(x => x.Entity.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x._id == item.EntityId);

            await collection.ReplaceOneAsync(filter, new MongoEntity<T> { _id = item.EntityId, Entity = item });
        }
    }
}