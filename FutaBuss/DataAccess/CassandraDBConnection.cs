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
            var insertBookingQuery = "INSERT INTO Booking (id, user_id, trip_id, pickup_location_id, dropoff_location_id, created_at) VALUES (?, ?, ?, ?, ?, ?)";
            var preparedStatement = await _session.PrepareAsync(insertBookingQuery);
            var boundStatement = preparedStatement.Bind(booking.Id, booking.UserId, booking.TripId, booking.PickUpLocationId,
                                                        booking.DropOffLocationId, booking.CreatedAt);
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

        public async Task CreateCustomerAsync(Customer customer)
        {
            var insertCustomerQuery = "INSERT INTO customer (id, fullname, phonenumber, email) VALUES (? , ?, ?, ?)";
            var preparedStatement = await _session.PrepareAsync(insertCustomerQuery);
            var boundStatement = preparedStatement.Bind(customer.Id, customer.FullName, customer.PhoneNumber, customer.Email);
            await _session.ExecuteAsync(boundStatement);
        }

        public async Task CreateTicketsAsync(Guid bookingId, Guid paymentId)
        {
            // Lấy thông tin Booking từ bảng Booking
            var booking = await GetBookingByIdAsync(bookingId);

            // Lấy thông tin BookingSeat từ bảng BookingSeat
            var bookingSeats = await GetBookingSeatsByBookingIdAsync(bookingId);
            var tickets = new List<Ticket>();

            foreach (var bookingSeat in bookingSeats)
            {
                // Tạo Ticket
                var ticket = new Ticket
                {
                    CustomerId = booking.UserId,
                    TripId = booking.TripId,
                    SeatId = bookingSeat.SeatId,
                    PickUpLocationId = booking.PickUpLocationId,
                    DropOffLocationId = booking.DropOffLocationId,
                    PaymentId = paymentId
                };

                tickets.Add(ticket);
            }

            // Lưu các Ticket vào bảng Ticket
            var tasks = tickets.Select(ticket =>
                _session.ExecuteAsync(new SimpleStatement(
                    "INSERT INTO Ticket (id, customer_id, trip_id, seat_id, pickup_location_id, dropoff_location_id, payment_id) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    ticket.Id, ticket.CustomerId, ticket.TripId, ticket.SeatId, ticket.PickUpLocationId, ticket.DropOffLocationId, ticket.PaymentId))
            );

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var mongoDB = MongoDBConnection.Instance;
            var updateTasks = bookingSeats.Select(bookingSeat => mongoDB.UpdateSeatIsSoldAsync(booking.TripId.ToString(), bookingSeat.SeatId.ToString()));
            await Task.WhenAll(updateTasks).ConfigureAwait(false);
        }

        //public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        //{
        //    var statement = new SimpleStatement("SELECT * FROM Booking WHERE id = ?", bookingId);
        //    var bookingRowSet = await _session.ExecuteAsync(statement).ConfigureAwait(false);
        //    var bookingRow = bookingRowSet.FirstOrDefault();

        //    if (bookingRow == null)
        //    {
        //        throw new Exception("Booking not found");
        //    }

        //    return new Booking
        //    {
        //        Id = bookingRow.GetValue<Guid>("id"),
        //        UserId = bookingRow.GetValue<Guid>("user_id"),
        //        TripId = bookingRow.GetValue<string>("trip_id"),
        //        CreatedAt = bookingRow.GetValue<DateTime>("created_at")
        //    };
        //}

        public async Task<List<BookingSeat>> GetBookingSeatsByBookingIdAsync(Guid bookingId)
        {
            var statement = new SimpleStatement("SELECT * FROM BookingSeat WHERE booking_id = ?", bookingId);
            var bookingSeatRows = await _session.ExecuteAsync(statement).ConfigureAwait(false);

            var bookingSeats = new List<BookingSeat>();

            foreach (var seatRow in bookingSeatRows)
            {
                var bookingSeat = new BookingSeat
                {
                    Id = seatRow.GetValue<Guid>("id"),
                    SeatId = seatRow.GetValue<Guid>("seat_id"),
                    BookingId = seatRow.GetValue<Guid>("booking_id")
                };

                bookingSeats.Add(bookingSeat);
            }

            return bookingSeats;
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
                    CreatedAt = row.GetValue<DateTime>("created_at"),
                    PickUpLocationId = row.GetValue<Guid>("pickup_location_id"),
                    DropOffLocationId = row.GetValue<Guid>("dropoff_location_id"),
                    // Các trường thông tin khác của Booking nếu có
                };

                return booking;
            }

            return null; // Trả về null nếu không tìm thấy booking có id tương ứng
        }


        public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
        {
            var selectQuery = "SELECT * FROM customer WHERE id = ?";
            var preparedStatement = await _session.PrepareAsync(selectQuery);
            var boundStatement = preparedStatement.Bind(customerId);

            var resultSet = await _session.ExecuteAsync(boundStatement);

            var row = resultSet.FirstOrDefault(); // Chỉ lấy bản ghi đầu tiên nếu có

            if (row != null)
            {
                var user = new Customer
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

        public async Task<int> CountSeat(Guid bookingId)
        {
            try
            {
                var statement = new SimpleStatement("SELECT COUNT(*) FROM BookingSeat WHERE booking_id = ? ALLOW FILTERING", bookingId);
                var resultSet = await _session.ExecuteAsync(statement).ConfigureAwait(false);

                var row = resultSet.FirstOrDefault();
                if (row != null)
                {
                    var count = row.GetValue<long>("count"); // Sử dụng GetValue<long>() để lấy giá trị long từ Cassandra
                    return (int)count; // Ép kiểu long về int
                }

                return 0; // Trả về 0 nếu không tìm thấy số lượng ghế đặt cho bookingId tương ứng
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error counting seats: " + ex.Message, ex);
            }
        }


        public async Task<List<BookingSeat>> GetAllBookingSeats(Guid bookingId)
        {
            try
            {
                var statement = new SimpleStatement("SELECT * FROM BookingSeat WHERE booking_id = ? ALLOW FILTERING", bookingId);
                var resultSet = await _session.ExecuteAsync(statement).ConfigureAwait(false);

                var bookingSeats = new List<BookingSeat>();
                foreach (var row in resultSet)
                {
                    var bookingSeat = new BookingSeat
                    {
                        // Đọc dữ liệu từ row và map vào đối tượng BookingSeat
                        Id = row.GetValue<Guid>("id"),
                        BookingId = row.GetValue<Guid>("booking_id"),
                        SeatId = row.GetValue<Guid>("seat_id"),
                        // Thêm các trường dữ liệu khác tương ứng với cấu trúc của BookingSeat
                    };
                    bookingSeats.Add(bookingSeat);
                }

                return bookingSeats;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving booking seats: " + ex.Message, ex);
            }
        }


        //public async Task CreatePaymentAsync(Payment payment)
        //{
        //    try
        //    {
        //        // Insert query for Payment
        //        var insertPaymentQuery = "INSERT INTO Payment (id, paid_at, platform, status, transaction_code) VALUES (?, ?, ?, ?, ?)";
        //        var preparedStatement = await _session.PrepareAsync(insertPaymentQuery);

        //        var boundStatement = preparedStatement.Bind(
        //            payment.Id,
        //            payment.PaidAt,
        //            payment.Platform,
        //            payment.Status,
        //            payment.TransactionCode
        //        );

        //        await _session.ExecuteAsync(boundStatement).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("Error creating payment: " + ex.Message, ex);
        //    }
        //}



    }
}
