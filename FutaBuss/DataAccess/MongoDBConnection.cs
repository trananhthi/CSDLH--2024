using MongoDB.Bson;
using MongoDB.Driver;


namespace FutaBuss.DataAccess
{
    public class MongoDBConnection
    {
        private static readonly Lazy<MongoDBConnection> _instance = new Lazy<MongoDBConnection>(() => new MongoDBConnection());
        private readonly IMongoDatabase _database;

        // Connection string and database name are hardcoded here
        private const string ConnectionString = "mongodb+srv://thuannt:J396QWpWuiGDZhOs@thuannt.yzjzr9s.mongodb.net/?appName=ThuanNT";
        private const string DatabaseName = "futabus";

        // Private constructor to prevent instantiation from outside
        private MongoDBConnection()
        {
            try
            {
                var client = new MongoClient(ConnectionString);
                _database = client.GetDatabase(DatabaseName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("MongoDB connection error: " + ex.Message, ex);
            }
        }

        public static MongoDBConnection Instance => _instance.Value;

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
