using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SoftwarePioniere.ReadModel.Services.MongoDb
{
    public class MongoDbEntityStore : EntityStoreBase<MongoDbOptions>
    {
        private readonly MongoDbConnectionProvider _provider;

        public MongoDbEntityStore(MongoDbOptions options,
            MongoDbConnectionProvider provider) : base(options)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public override async Task<T[]> LoadItemsAsync<T>(CancellationToken token = default(CancellationToken))
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType}", typeof(T));

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>());

            var items = await collection.FindAsync(filter, null, token);

            var ret = new List<T>();

            while (await items.MoveNextAsync(token))
            {
                ret.AddRange(items.Current);
            }

            return ret.ToArray();
        }

        public override async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken token = default(CancellationToken))
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType} {Expression}", typeof(T), predicate);

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(predicate);

            var items = await collection.FindAsync(filter, null, token);

            var ret = new List<T>();

            while (await items.MoveNextAsync(token))
            {
                ret.AddRange(items.Current);
            }

            return ret.Where(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>()).ToArray();
        }

        public override async Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default(CancellationToken))
        {
            if (parms == null)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            Logger.LogTrace("LoadPagedResultAsync: {EntityType} {Paramter}", typeof(T), parms);

            IQueryable<T> items;

            if (parms.Where != null)
            {
                items = (await LoadItemsAsync<T>(parms.Where, token)).AsQueryable();
            }
            else
            {
                items = (await LoadItemsAsync<T>(token)).AsQueryable();
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

        protected override async Task InternalDeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(x => x.EntityId == entityId);
            await collection.DeleteOneAsync(filter, token);
        }

        protected override async Task InternalInsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken))
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);


            var collection = _provider.GetCol<T>();
            await collection.InsertOneAsync(item, null, token).ConfigureAwait(false);
        }

        protected override async Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken token = new CancellationToken())
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            Logger.LogTrace("BulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);

            var collection = _provider.GetCol<T>();

            //var entities = items.Select(item => new MongoEntity<T> { _id = item.EntityId, Entity = item });

            await collection.InsertManyAsync(items, null, token);

        }

        protected override async Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken))
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalInsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            var collection = _provider.GetCol<T>();

            var filter = new ExpressionFilterDefinition<T>(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == item.EntityId);

            var exi = await collection.FindAsync(filter, null, token);

            if (await exi.MoveNextAsync(token))
            {
                await UpdateItemAsync(item, token).ConfigureAwait(false);
            }
            else
            {
                await InsertItemAsync(item, token).ConfigureAwait(false);
            }
        }

        protected override async Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            var collection = _provider.GetCol<T>();

            var filter = new ExpressionFilterDefinition<T>(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == entityId);

            var exi = await collection.FindAsync(filter, null, token);

            if (await exi.MoveNextAsync(token))
            {
                var cu = exi.Current.FirstOrDefault();
                if (cu != null)
                    return cu;
            }

            return null;
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken))
        {

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == item.EntityId);

            await collection.ReplaceOneAsync(filter, item, null, token);
        }
    }
}