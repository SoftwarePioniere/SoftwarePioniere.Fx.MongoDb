using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace SoftwarePioniere.ReadModel.Services.MongoDb
{

    public class MongoDbConnectionProvider : IEntityStoreConnectionProvider
    {
        private readonly ILogger _logger;

        public TypeKeyCache KeyCache { get; private set; }

        //private readonly Uri _collectionUri;

        public MongoDbOptions Settings { get; set; }

        static MongoDbConnectionProvider()
        {
            // set your options on this line
            var serializer = new DateTimeSerializer(DateTimeKind.Utc, BsonType.Document);
            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
        }

        public MongoDbConnectionProvider(ILoggerFactory loggerFactory, IOptions<MongoDbOptions> settings)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());

            KeyCache = new TypeKeyCache();

            Settings = settings.Value;
            _logger.LogInformation("{Connection}", settings);

            InitClient();
            InitDatabase();
        }


        public async Task<bool> CheckDatabaseExists()
        {
            _logger.LogTrace(nameof(CheckDatabaseExists));

            var client = CreateClient();

            var databases = await client.ListDatabasesAsync();

            while (await databases.MoveNextAsync())
            {
                var items = databases.Current.ToArray();
                var c = (items.Count(x => x["name"] == Settings.DatabaseId) > 0);

                if (c)
                    return true;

            }

            return false;
        }

        private IMongoClient CreateClient()
        {
            var url = new MongoServerAddress(Settings.Server, Settings.Port);
            var settings = new MongoClientSettings
            { Server = url };
            var client = new MongoClient(settings);
            return client;
        }



        private void InitDatabase()
        {
            Database = new Lazy<IMongoDatabase>(() =>
            {
                var client = CreateClient();
                return client.GetDatabase(Settings.DatabaseId);
            });
        }

        private void InitClient()
        {
            Client = new Lazy<IMongoClient>(() =>
                {
                    var client = CreateClient();
                    return client;
                }
            );
        }

        public Lazy<IMongoClient> Client { get; private set; }

        public Lazy<IMongoDatabase> Database { get; private set; }


        public async Task ClearDatabaseAsync()
        {
            _logger.LogInformation("Clear Database");
            await Client.Value.DropDatabaseAsync(Settings.DatabaseId);
            _logger.LogInformation("Reinit Client");
            InitDatabase();
        }

        public IMongoCollection<MongoEntity<T>> GetCol<T>() where T : Entity
        {
            return Database.Value.GetCollection<MongoEntity<T>>(KeyCache.GetEntityTypeKey<T>());
        }
    }
}
