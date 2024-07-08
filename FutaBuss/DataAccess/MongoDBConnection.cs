using FutaBuss.Model;
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

        public async Task<List<Trip>> SearchTripsAsync(string departure, string destination, string departureDate, int ticketCount)
        {
            var collection = _database.GetCollection<Trip>("trips");
            var filter = Builders<Trip>.Filter.Eq("departure_province_code", departure)
                          & Builders<Trip>.Filter.Eq("destination_province_code", destination)
                          & Builders<Trip>.Filter.Eq("departure_date", departureDate);

            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<Trip>> SearchRoundTripsAsync(string departure, string destination, string departureDate, int ticketCount)
        {
            var collection = _database.GetCollection<Trip>("trips");
            var filter = Builders<Trip>.Filter.Eq("departure_province_code", departure)
                          & Builders<Trip>.Filter.Eq("destination_province_code", destination)
                          & Builders<Trip>.Filter.Eq("departure_date", departureDate);

            return await collection.Find(filter).ToListAsync();
        }
    }
}
