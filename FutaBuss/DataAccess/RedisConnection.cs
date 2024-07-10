using FutaBuss.Model;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace FutaBuss.DataAccess
{
    public class RedisConnection
    {
        private static readonly Lazy<RedisConnection> _instance = new Lazy<RedisConnection>(() => new RedisConnection());
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        private const string ConnectionString = "redis-18667.c8.us-east-1-2.ec2.cloud.redislabs.com:18667";
        private const string Username = "default";
        private const string Password = "dVZCrABvG85l0L9JQI9izqn2SDvvTx82";

        // Private constructor to prevent instantiation from outside
        private RedisConnection()
        {
            try
            {
                var options = ConfigurationOptions.Parse(ConnectionString);
                options.User = Username;
                options.Password = Password;
                _redis = ConnectionMultiplexer.Connect(options);
                _db = _redis.GetDatabase();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Redis connection error: " + ex.Message, ex);
            }
        }

        public static RedisConnection Instance => _instance.Value;

        public void SetString(string key, string value)
        {
            try
            {
                _db.StringSet(key, value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Redis set string error: " + ex.Message, ex);
            }
        }

        public string? GetString(string key)
        {
            try
            {
                return _db.StringGet(key);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Redis get string error: " + ex.Message, ex);
            }
        }

        public void DeleteKey(string key)
        {
            try
            {
                _db.KeyDelete(key);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Redis delete key error: " + ex.Message, ex);
            }
        }


        public void CacheBooking(Guid id, Guid userId, List<string> seatIds)
        {
            var bookingData = new Dictionary<string, string>();

            foreach (var seatId in seatIds)
            {
                var key = $"booking:{id}:seat:{seatId}";
                var value = JsonConvert.SerializeObject(new
                {
                    UserId = userId,
                    SeatId = seatId
                });
                bookingData[key] = value;
            }

            var expiry = TimeSpan.FromMinutes(15);
            var expiryTime = DateTime.UtcNow.Add(expiry);

            var batch = _db.CreateBatch();
            foreach (var item in bookingData)
            {
                batch.StringSetAsync(item.Key, item.Value);
                batch.KeyExpireAsync(item.Key, expiryTime);
            }

            batch.StringSetAsync($"booking:{id}:wait_to_pay", "OK");
            batch.KeyExpireAsync($"booking:{id}:wait_to_pay", expiryTime);

            batch.Execute();
        }

        public List<Province> GetAllProvinces()
        {
            var keys = _redis.GetServer(_redis.GetEndPoints()[0]).Keys(pattern: "province:*:name");
            var provinces = new List<Province>();

            foreach (var key in keys)
            {
                var name = _db.StringGet(key);
                if (!name.IsNullOrEmpty)
                {
                    var code = key.ToString().Split(':')[1];
                    provinces.Add(new Province(code, name));
                }
            }

            provinces = provinces.OrderBy(p => p.Code).ToList();

            return provinces;
        }

        public bool KeyExistsPattern(string pattern)
        {
            var keys = _redis.GetServer(_redis.GetEndPoints()[0]).Keys(pattern: pattern);
            foreach (var key in keys)
            {
                if (_db.KeyExists(key))
                {
                    return true;
                }
            }
            return false;
        }


        public void SetBookingWaitToPay(Guid bookingId)
        {
            var key = $"booking:{bookingId}:wait_to_pay";
            var value = "WAITING"; // Or any appropriate value
            var expiry = TimeSpan.FromMinutes(15); // Adjust expiry time as needed
            var expiryTime = DateTime.UtcNow.Add(expiry);

            _db.StringSet(key, value, expiry);
            _db.KeyExpire(key, expiryTime);
        }

        public string GetBookingWaitToPay(Guid bookingId)
        {
            var key = $"booking:{bookingId}:wait_to_pay";
            return _db.StringGet(key);
        }

        public int GetBookingTTL(Guid bookingId)
        {
            var key = $"booking:{bookingId}:wait_to_pay";
            var ttl = _db.KeyTimeToLive(key);

            // Chuyển đổi TTL thành số giây nếu không null
            return (int)ttl?.TotalSeconds;
        }

    }
}
