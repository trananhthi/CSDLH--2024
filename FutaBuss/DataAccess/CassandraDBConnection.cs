using Cassandra;
using FutaBuss.Model;
using System.IO;

namespace FutaBuss.DataAccess
{
    public class CassandraDBConnection
    {
        private static readonly Lazy<CassandraDBConnection> _instance = new Lazy<CassandraDBConnection>(() => new CassandraDBConnection());
        private readonly ISession _session;

        // Cassandra Astra connection details
        private const string clientId = "NurzkWTRQhNjHRWeddORtjxd";
        private const string clientSecret = "Zl60ab8yFucPW_Z5.3sO3Cjx4DMlZGJL32Cx5SnyWl_4+GfAkMEFZ3UF44v1ZeRb_hkqzTn,CdSOh4USF2,-tErnBSuiFvTk9JdtOG8YedGArFUn1Ia_uzqSqgkKnA_n";
        //private const string SecureConnectBundlePath = "C:\\Users\\Admin\\Desktop\\NoSQL\\secure-connect-futabus.zip";
        private static readonly string SecureConnectBundlePath = Path.Combine("..\\..\\..", "Bundle", "secure-connect-futabus.zip");

        private const string Keyspace = "futabus";

        // Private constructor to prevent instantiation from outside
        private CassandraDBConnection()
        {
            try
            {
                // Create cluster and session
                var cluster = Cluster.Builder()
                    .WithCloudSecureConnectionBundle(SecureConnectBundlePath)
                    .WithCredentials(clientId, clientSecret)
                    .Build();

                _session = cluster.Connect(Keyspace);

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cassandra connection error: " + ex.Message, ex);
            }
        }

        public static CassandraDBConnection Instance => _instance.Value;

        public ISession GetSession()
        {
            return _session;
        }

        public async Task AddBookingAsync(Booking booking, List<string> seatIdStrings)
        {
            // Chuyển đổi danh sách chuỗi GUID thành danh sách Guid
            var seatIds = seatIdStrings.ConvertAll(Guid.Parse);

            // Lưu vào bảng Booking
            var insertBookingQuery = "INSERT INTO Booking (id, user_id, trip_id, created_at) VALUES (?, ?, ?, ?)";
            var preparedStatement = await _session.PrepareAsync(insertBookingQuery);
            var boundStatement = preparedStatement.Bind(booking.Id, booking.UserId, booking.TripId, booking.CreatedAt);
            await _session.ExecuteAsync(boundStatement);

            // Lưu vào bảng BookingSeat
            var insertBookingSeatQuery = "INSERT INTO BookingSeat (id, seat_id, booking_id) VALUES (?, ?, ?)";
            var preparedStatementSeat = await _session.PrepareAsync(insertBookingSeatQuery);

            foreach (var seatId in seatIds)
            {
                var bookingSeat = new BookingSeat
                {
                    Id = Guid.NewGuid(),
                    SeatId = seatId,
                    BookingId = booking.Id
                };

                var boundStatementSeat = preparedStatementSeat.Bind(bookingSeat.Id, bookingSeat.SeatId, bookingSeat.BookingId);
                await _session.ExecuteAsync(boundStatementSeat);
            }
        }


        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            var selectQuery = "SELECT * FROM Booking WHERE id = ?";
            var preparedStatement = await _session.PrepareAsync(selectQuery);
            var boundStatement = preparedStatement.Bind(bookingId);

            var resultSet = await _session.ExecuteAsync(boundStatement);

            var row = resultSet.FirstOrDefault(); // Chỉ lấy bản ghi đầu tiên nếu có

            if (row != null)
            {
                var booking = new Booking
                {
                    Id = row.GetValue<Guid>("id"),
                    UserId = row.GetValue<Guid>("user_id"),
                    TripId = row.GetValue<string>("trip_id"),
                    CreatedAt = row.GetValue<DateTime>("created_at")
                    // Các trường thông tin khác của Booking nếu có
                };

                return booking;
            }

            return null; // Trả về null nếu không tìm thấy booking có id tương ứng
        }


        public async Task<User> GetCustomerByIdAsync(Guid customerId)
        {
            var selectQuery = "SELECT * FROM customer WHERE id = ?";
            var preparedStatement = await _session.PrepareAsync(selectQuery);
            var boundStatement = preparedStatement.Bind(customerId);

            var resultSet = await _session.ExecuteAsync(boundStatement);

            var row = resultSet.FirstOrDefault(); // Chỉ lấy bản ghi đầu tiên nếu có

            if (row != null)
            {
                var user = new User
                {
                    Id = row.GetValue<Guid>("id"),
                    FullName = row.GetValue<string>("fullname"),
                    Email = row.GetValue<string>("email"),
                    PhoneNumber = row.GetValue<string>("phonenumber")
                    // Các trường thông tin khác của Booking nếu có
                };

                return user;
            }

            return null; // Trả về null nếu không tìm thấy booking có id tương ứng
        }
    }
}
