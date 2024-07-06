using MongoDB.Bson;
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

        public async Task<List<BsonDocument>> SearchTripsAsync(string departure, string destination, string departureDate, int ticketCount)
        {
            var collection = _database.GetCollection<BsonDocument>("trips");
            var filter = Builders<BsonDocument>.Filter.Eq("departure_province_code", departure)
                          & Builders<BsonDocument>.Filter.Eq("destination_province_code", destination)
                          & Builders<BsonDocument>.Filter.Eq("departure_date", departureDate);

            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<BsonDocument>> SearchRoundTripsAsync(string departure, string destination, string departureDate, int ticketCount)
        {
            var collection = _database.GetCollection<BsonDocument>("trips");
            var filter = Builders<BsonDocument>.Filter.Eq("departure_province_code", departure)
                          & Builders<BsonDocument>.Filter.Eq("destination_province_code", destination)
                          & Builders<BsonDocument>.Filter.Eq("departure_date", departureDate);

            return await collection.Find(filter).ToListAsync();
        }
    }
}
