using MongoDB.Driver;

namespace FutaBuss.DataAccess
{
    public class MongoDBConnection
    {
        private readonly IMongoDatabase _database;

        public MongoDBConnection(string connectionString, string databaseName)
        {
            try
            {
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("MongoDB connection error: " + ex.Message, ex);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
