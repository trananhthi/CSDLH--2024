using System.Windows;
using System.Windows.Controls;
using ZXing.Common;
using ZXing;
using System.Drawing;
using System.Windows.Media.Imaging;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using System.IO;
using System.Windows.Threading;
using FutaBuss.DataAccess;
using FutaBuss.Model;

namespace FutaBuss.View
{
    /// <summary>
    /// Interaction logic for PaymentMethod.xaml
    /// </summary>
    public partial class PaymentMethod : Page
    {
        private DispatcherTimer timer;
        private int countdownSeconds = 100; // Thời gian đếm ngược, đơn vị là giây
        private FutaBuss.Model.Booking booking;
        private FutaBuss.Model.Booking returnBooking;
        private FutaBuss.Model.Customer customer;
        private FutaBuss.Model.Trip trip;
        private FutaBuss.Model.Trip returnTrip;
        private int totalPriceTrip = 0;
        private int returnTotalPriceTrip = 0;
        private string paymentMethod;

        private List<BookingSeat> bookingSeatList;
        private List<BookingSeat> returnBookingSeatList;

        private MongoDBConnection _mongoDBConnection;
        private RedisConnection _redisConnection;
        private PostgreSQLConnection _postgreSQLConnection;
        private CassandraDBConnection _cassandraDBConnection;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public PaymentMethod(Guid bookingId, Guid? returnBookingId = null)
        {
            InitializeComponent();
            InitializeDatabaseConnections();
            if(returnBookingId != null)
            {
                returnTripTab.Visibility = Visibility.Visible;
            }    
            if (_redisConnection.GetBookingWaitToPay(bookingId) != null)
            {
                countdownSeconds = _redisConnection.GetBookingTTL(bookingId);
            } 
            else
            {
                _redisConnection.SetBookingWaitToPay(bookingId);
                countdownSeconds = 20;
            }

            

            futaPayRadioButton.IsChecked = true;
            StartCountdown();
            InitializeAsync(bookingId, returnBookingId);
           

        }

        public async Task InitializeAsync(Guid bookingId, Guid? returnBookingId = null)
        {
            
            booking = await GetBookingAsync(bookingId);
              
            
            customer = await GetCustomerAsync(booking.UserId);
            trip = await GetTripAsync(booking.TripId);
            if (returnBookingId != null)
            {
                returnBooking = await GetBookingAsync((Guid)returnBookingId);
                returnTrip = await GetTripAsync(returnBooking.TripId);
                LoadReturnTripInfo(returnTrip);
            }
            LoadCustomerInfo(customer);
            LoadTripInfo(trip);
        }

        private void InitializeDatabaseConnections()
        {
            try
            {
                _mongoDBConnection = MongoDBConnection.Instance;
                _redisConnection = RedisConnection.Instance;
                _postgreSQLConnection = PostgreSQLConnection.Instance;
                _cassandraDBConnection = CassandraDBConnection.Instance;
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<FutaBuss.Model.Booking> GetBookingAsync(Guid bookingId)
        {
            try
            {
                return await _cassandraDBConnection.GetBookingByIdAsync(bookingId);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu cần thiết
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; // hoặc xử lý ngoại lệ theo nhu cầu của bạn
            }
        }

        private async Task<FutaBuss.Model.Customer> GetCustomerAsync(Guid customerId)
        {
            try
            {
                return await _cassandraDBConnection.GetCustomerByIdAsync(customerId);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu cần thiết
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; // hoặc xử lý ngoại lệ theo nhu cầu của bạn
            }
        }


        private async Task<FutaBuss.Model.Trip> GetTripAsync(string tripId)
        {
            try
            {
                return await _mongoDBConnection.GetTripByIdAsync(tripId);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu cần thiết
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; // hoặc xử lý ngoại lệ theo nhu cầu của bạn
            }
        }

        private async Task<string> GetProvinceName(string code)
        {
            try
            {
                return await _postgreSQLConnection.GetProvinceNameByCodeAsync(code);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu cần thiết
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; // hoặc xử lý ngoại lệ theo nhu cầu của bạn
            }
        }

        private async Task<int> CountSeat(Guid bookingId)
        {
            try
            {
                return await _cassandraDBConnection.CountSeat(bookingId);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu cần thiết
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; // hoặc xử lý ngoại lệ theo nhu cầu của bạn
            }
        }


        private async Task<List<BookingSeat>> GetAllBookingSeat(Guid bookingId)
        {
            try
            {
                return await _cassandraDBConnection.GetAllBookingSeats(bookingId);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu cần thiết
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; // hoặc xử lý ngoại lệ theo nhu cầu của bạn
            }
        }


        private async Task createNewPayment(Payment payment)
        {
            try
            {
                await _cassandraDBConnection.CreatePaymentAsync(payment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving booking: {ex.Message}");
                throw; 
            }
        }

        public async Task UpdateBooking(Guid bookingId, Guid paymentId)
        {
            await _cassandraDBConnection.CreateTicketsAsync(bookingId, paymentId);
        }

        //public async Task UpdateSeatIsSoldAsync(string tripId, string seatId)
        //{
        //    await _mongoDBConnection.UpdateSeatIsSoldAsync(tripId, seatId);
        //}

        private void LoadCustomerInfo (FutaBuss.Model.Customer customer)
        {
            fullName.Text = customer.FullName;
            email.Text = customer.Email;
            phoneNumber.Text = customer.PhoneNumber;
        }



        private async void LoadTripInfo (FutaBuss.Model.Trip trip)
        {
            departureTime.Text = $"{trip.DepartureTime:hh\\:mm} {trip.DepartureDate:dd/MM/yyyy}";
            // Lấy giờ khởi hành và ngày khởi hành từ trip
            var departureDateTime = trip.DepartureDate.Date + trip.DepartureTime;

            // Tính toán boarding time là departure time trừ đi 15 phút
            var boardingDateTime = departureDateTime.AddMinutes(-15);

            // Định dạng chuỗi để hiển thị
            boardingTime.Text = $"Trước {boardingDateTime:hh\\:mm dd/MM/yyyy}";
            await semaphore.WaitAsync();
            string departure_province;
            string destination_province ;
            try
            {
                departure_province = await GetProvinceName(trip.DepartureProvinceCode);
                destination_province = await GetProvinceName(trip.DestinationProvinceCode);
            }
            finally
            {
                semaphore.Release();
            }
            
            provincePlace.Text = departure_province + " - " + destination_province;
            int numberOfSeat = await CountSeat(booking.Id);
            noSeat.Text = numberOfSeat.ToString() + " Ghế";
            totalPrice.Text = $"{(numberOfSeat * trip.Price):#,0}đ";
            totalPriceTrip = numberOfSeat * trip.Price;

            bookingSeatList = await GetAllBookingSeat(booking.Id);
            List<string> bookingSeatAliasList = new List<string>();

            foreach (BookingSeat bookingSeat in bookingSeatList)
            {
                string alias = GetSeatAlias(bookingSeat.SeatId, trip);
                if (alias != null)
                {
                    bookingSeatAliasList.Add(alias);
                }
                
            }
            string result = string.Join(", ", bookingSeatAliasList);
            aliasSeat.Text = result;
            pickUpLocation.Text = GetPickUpPlace(booking.PickUpLocationId, trip);
            LoadTotalPriceInfo();

        }


        private async void LoadReturnTripInfo(FutaBuss.Model.Trip trip)
        {
            returnDepartureTime.Text = $"{trip.DepartureTime:hh\\:mm} {trip.DepartureDate:dd/MM/yyyy}";
            // Lấy giờ khởi hành và ngày khởi hành từ trip
            var departureDateTime = trip.DepartureDate.Date + trip.DepartureTime;

            // Tính toán boarding time là departure time trừ đi 15 phút
            var boardingDateTime = departureDateTime.AddMinutes(-15);

            // Định dạng chuỗi để hiển thị
            returnBoardingTime.Text = $"Trước {boardingDateTime:hh\\:mm dd/MM/yyyy}";
            await semaphore.WaitAsync();
            string departure_province;
            string destination_province;
            try
            {
                departure_province = await GetProvinceName(trip.DepartureProvinceCode);
                destination_province = await GetProvinceName(trip.DestinationProvinceCode);
            }
            finally
            {
                semaphore.Release();
            }
            returnProvincePlace.Text = departure_province + " - " + destination_province;
            int numberOfSeat = await CountSeat(booking.Id);
            returnNoSeat.Text = numberOfSeat.ToString() + " Ghế";
            returnTotalPrice.Text = $"{(numberOfSeat * trip.Price):#,0}đ";
            returnTotalPriceTrip = numberOfSeat * trip.Price;

            returnBookingSeatList = await GetAllBookingSeat(booking.Id);
            List<string> bookingSeatAliasList = new List<string>();

            foreach (BookingSeat bookingSeat in returnBookingSeatList)
            {
                string alias = GetSeatAlias(bookingSeat.SeatId, trip);
                if (alias != null)
                {
                    bookingSeatAliasList.Add(alias);
                }

            }
            string result = string.Join(", ", bookingSeatAliasList);
            returnAliasSeat.Text = result;
            returnPickUpLocation.Text = GetPickUpPlace(booking.PickUpLocationId, trip);

        }

        private void LoadTotalPriceInfo()
        {
            totalTicketPrice.Text = $"{(totalPriceTrip):#,0}đ" ;
            returnTotalTicketPrice.Text = $"{(returnTotalPriceTrip):#,0}đ";

            finalTotalPrice.Text = $"{(totalPriceTrip + returnTotalPriceTrip):#,0}đ";
            finalTotalPriceHeader.Text = $"{(totalPriceTrip + returnTotalPriceTrip):#,0}đ";
        }    


        private void StartCountdown()
        {
            // Tạo một DispatcherTimer với tick interval là 1 giây
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // Bắt đầu đếm ngược
            timer.Start();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            // Giảm số giây còn lại đi 1 và cập nhật lên giao diện
            countdownSeconds--;

            // Tính toán phút và giây từ số giây còn lại
            int minutes = countdownSeconds / 60;
            int seconds = countdownSeconds % 60;

            // Format lại chuỗi để hiển thị dưới dạng "mm:ss"
            string countdownText = $"{minutes:D2}:{seconds:D2}";

            // Hiển thị chuỗi đã định dạng lên TextBlock
            countDownTime.Text = countdownText;

            // Kiểm tra nếu đếm ngược kết thúc
            if (countdownSeconds == 0)
            {
                timer.Stop();
                cancelPayment();
                MessageBoxResult result = MessageBox.Show("Hết thời gian giữ chỗ!. Quay về trang chủ", "Hết thời gian", MessageBoxButton.OK, MessageBoxImage.Information);

                // Kiểm tra nếu người dùng nhấn OK
                if (result == MessageBoxResult.OK)
                {
                    this.NavigationService.Navigate(new FutaBuss.View.SearchTrips());
                }
            }
        }


        public void GenerateQRCode(string data)
        {

            string[] parts = data.Split('-');
            string lastPart = parts[parts.Length - 1];

            QrCodeEncodingOptions options = new()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 300,
                Height = 300,
                NoPadding = true,
            };

            BarcodeWriter writer = new()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            string filePath = Path.Combine("..\\..\\..", "Images", $"{lastPart}qrcode.png");

            Bitmap qrCodeBitmap = writer.Write(data);
            qrCodeBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        }

        // Method to update the QR code image
        private void UpdateQRCodeImage(string data)
        {
            GenerateQRCode(data);

            string[] parts = data.Split('-');
            string lastPart = parts[parts.Length - 1];

            string imagePath = Path.Combine("..\\..\\..", "Images", $"{lastPart}qrcode.png");

            // Kiểm tra xem tệp có tồn tại không trước khi thiết lập Source
            if (File.Exists(imagePath))
            {
                // Tạo một đối tượng BitmapImage để tải hình ảnh từ tệp
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmapImage.EndInit();

                // Thiết lập Source của control Image
                qrCodeImage.Source = bitmapImage;
            }
            else
            {
                throw new Exception($"File '{imagePath}' not found.");
            }
        }


        private void UpdateQRCodeLogo(string fileName)
        {

            if(fileName == "shopee.png" || fileName == "vietQR.png")
            {
                string imagePath = Path.Combine("..\\..\\..", "Images", fileName);
                imgQRLogo.Visibility = Visibility.Visible;
                svgQRLogo.Visibility= Visibility.Collapsed;

                // Kiểm tra xem tệp có tồn tại không trước khi thiết lập Source
                if (File.Exists(imagePath))
                {
                    // Tạo một đối tượng BitmapImage để tải hình ảnh từ tệp
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    // Thiết lập Source của control Image
                    imgQRLogo.Source = bitmapImage;
                }
                else
                {
                    throw new Exception($"File '{imagePath}' not found.");
                }
            }
            else {
                imgQRLogo.Visibility = Visibility.Collapsed;
                svgQRLogo.Visibility = Visibility.Visible;
                svgQRLogo.Source = new Uri($"pack://application:,,,/Images/{fileName}");
            }

        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton checkedRadioButton = sender as RadioButton;
            string data = fullName.Text + '-' + phoneNumber.Text + '-' + email.Text + '-' + Path.GetFileNameWithoutExtension(checkedRadioButton.Tag.ToString()); // Replace with your actual data
            paymentMethod = Path.GetFileNameWithoutExtension(checkedRadioButton.Tag.ToString());
            UpdateQRCodeImage(data);
            UpdateQRCodeLogo(checkedRadioButton.Tag.ToString());

        }

        private string GetSeatAlias(Guid seatId, Trip trip)
        {
            foreach (var floor in trip.SeatConfig.Floors)
            {
                foreach (var seat in floor.Seats)
                {
                    if (Guid.Parse(seat.SeatId) == seatId)
                    {
                        return seat.Alias; // Trả về alias nếu tìm thấy seatId
                    }
                }
            }

            // Trả về null hoặc một giá trị mặc định nếu không tìm thấy
            return null;
        }


        private string GetPickUpPlace(Guid PickUpLocationId, Trip trip)
        {
            foreach (var pickUp in trip.Transhipments.PickUp)
            {
                if (Guid.Parse(pickUp.Id) == PickUpLocationId)
                {
                    return pickUp.Location; // Trả về địa điểm pick-up nếu tìm thấy PickUpLocationId
                }
            }

            // Trả về null hoặc một giá trị mặc định nếu không tìm thấy
            return null;
        }

        private async void PaymentButton_Click(object sender, RoutedEventArgs e)
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string transactionCode = new string(Enumerable.Repeat(chars, 10) // 10 characters long
                .Select(s => s[random.Next(s.Length)]).ToArray());
            Guid newPaymentId = Guid.NewGuid();

            Payment payment = new Payment
            {
                Id = newPaymentId, // Tạo GUID mới
                PaidAt = DateTime.UtcNow, // Thời điểm thanh toán (thời gian hiện tại UTC)
                Platform = paymentMethod, // Nền tảng thanh toán
                Status = "Success", // Trạng thái thanh toán (có thể là Pending, Completed, Failed, etc.)
                TransactionCode = transactionCode  // Mã giao dịch
            };
            await createNewPayment(payment);
            await UpdateBooking(booking.Id, newPaymentId);
            if(returnBooking != null)
            {
                await UpdateBooking(returnBooking.Id, newPaymentId);
            }
            
            _redisConnection.DeleteKey($"booking:{booking.Id}:wait_to_pay");

            foreach (BookingSeat bookingSeat in bookingSeatList)
            {
                _redisConnection.DeleteKey($"booking:{booking.Id}:seat:{bookingSeat.SeatId.ToString().Replace("-","")}");

            }

            if (returnBooking != null)
            {
                foreach (BookingSeat bookingSeat in returnBookingSeatList)
                {
                    _redisConnection.DeleteKey($"booking:{booking.Id}:seat:{bookingSeat.SeatId.ToString().Replace("-", "")}");

                }
            }



            this.NavigationService.Navigate(new PaymentSuccess(customer,totalPriceTrip + returnTotalPriceTrip, paymentMethod, newPaymentId));

        }

        private void cancelPayment ()
        {
            _redisConnection.DeleteKey($"booking:{booking.Id}:wait_to_pay");

            foreach (BookingSeat bookingSeat in bookingSeatList)
            {
                _redisConnection.DeleteKey($"booking:{booking.Id}:seat:{bookingSeat.SeatId.ToString().Replace("-", "")}");

            }

            if (returnBooking != null)
            {
                foreach (BookingSeat bookingSeat in returnBookingSeatList)
                {
                    _redisConnection.DeleteKey($"booking:{booking.Id}:seat:{bookingSeat.SeatId.ToString().Replace("-", "")}");

                }
            }


        }

        private void CancelPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc muốn hủy thanh toán không?", "Xác nhận hủy thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                cancelPayment();
                this.NavigationService.Navigate(new FutaBuss.View.SearchTrips());
            }
            
        }
    }
}


