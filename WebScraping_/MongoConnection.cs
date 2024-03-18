using MongoDB.Bson;
using MongoDB.Driver;

namespace WebScraping_
{
    public class MongoConnection
    {
        private readonly IMongoDatabase _database;

        public MongoConnection()
        {
            string connectionString = "mongodb+srv://metehan:1234@webscrapingcluster.q9by7nn.mongodb.net/?retryWrites=true&w=majority&appName=WebScrapingCluster";
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            MongoClient client = new MongoClient(settings);
            _database = client.GetDatabase("WebScrapingDb");
        }

        public IMongoDatabase ConnectToMongoDB()
        {
            return _database;
        }

        public void DeleteAllDocuments(string collectionName)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Empty;
            collection.DeleteMany(filter);
        }
    }
}
