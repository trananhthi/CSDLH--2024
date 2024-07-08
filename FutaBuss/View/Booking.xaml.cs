using FutaBuss.DataAccess;
using FutaBuss.Model;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FutaBuss.View
{
    /// <summary>
    /// Interaction logic for Booking.xaml
    /// </summary>
    public partial class Booking : Page
    {
        private MongoDBConnection _mongoDBConnection;
        private RedisConnection _redisConnection;
        private PostgreSQLConnection _postgreSQLConnection;
        private CassandraDBConnection _cassandraDBConnection;
        private MongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<Trip> tripsCollection;
        private Trip trip;
        private Trip returnTrip;
        List<string> departureSeatIds = new List<string>();
        List<string> returnSeatIds = new List<string>();
        int departureSeatCount = 0;
        int returnSeatCount = 0;
        int departureSeatPrice = 0;
        int returnSeatPrice = 0;
        int paymentFee = 0;
        string seatText = string.Empty;
        string returnSeatText = string.Empty;
        bool isRoundTrip = false;
        public Booking()
        {
            InitializeComponent();
            InitializeDatabaseConnections();


            InitializeAsync();

        }



        private void InitializeDatabaseConnections()
        {
            try
            {
                // Kết nối MongoDB
                //_mongoDBConnection = new MongoDBConnection("mongodb+srv://thuannt:J396QWpWuiGDZhOs@thuannt.yzjzr9s.mongodb.net/?appName=ThuanNT", "futabus");

                //_redisConnection = new RedisConnection("redis-18667.c8.us-east-1-2.ec2.cloud.redislabs.com:18667", "default", "dVZCrABvG85l0L9JQI9izqn2SDvvTx82");

                //_postgreSQLConnection = new PostgreSQLConnection("Host=dpg-cq12053v2p9s73cjijm0-a.singapore-postgres.render.com;Username=root;Password=vTwWs92lObTZrhI9IFcJGXJxZCdzeBas;Database=mds_postpresql");

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

        private async void InitializeAsync()
        {
            try
            {
                tripsCollection = _mongoDBConnection.GetCollection<Trip>("trips");
                await LoadTripDataAsync("b86d8901d5c045f9b19a0a4939d46a25", "b86d8901d5c045f9b19a0a4939d46a25");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ImageBrush brush = button.Background as ImageBrush;
                if (brush != null)
                {
                    ImageBrush newBrush = new ImageBrush();
                    if (brush.ImageSource.ToString().Contains("seat_active.png"))
                    {
                        if (departureSeatCount > 4)
                        {
                            MessageBox.Show("Đã chọn đủ số ghế");
                            return;
                        }
                        newBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_selecting.png"));
                        departureSeatPrice += trip.Price;
                        if (!string.IsNullOrEmpty(seatText))
                        {
                            seatText += ",";
                        }
                        seatText += button.Content.ToString();
                        departureSeatIds.Add((string)button.Tag);
                        departureSeatCount++;

                    }
                    else if (brush.ImageSource.ToString().Contains("seat_selecting.png"))
                    {
                        newBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                        departureSeatPrice -= trip.Price;
                        string seatToRemove = button.Content.ToString();
                        List<string> seats = seatText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (seats.Contains(seatToRemove))
                        {
                            seats.Remove(seatToRemove);
                            seatText = string.Join(",", seats);
                        }
                        departureSeatIds.Remove((string)button.Tag);
                        departureSeatCount--;
                    }
                    else
                    {
                        return;
                    }
                    SeatCountTextBlock.Text = $"{departureSeatCount} Ghế";
                    TotalDeparturePriceTextBlock.Text = $"{departureSeatPrice}đ";
                    SeatTextBlock.Text = seatText;
                    DepartureSeatPriceTextBlock.Text = $"{departureSeatPrice}đ";
                    PaymentFeeTextBlock.Text = $"{paymentFee}đ";
                    TotalPriceTextBlock.Text = $"{departureSeatPrice + returnSeatPrice + paymentFee}đ";
                    button.Background = newBrush;
                }
            }
        }

        private void ReturnSeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ImageBrush brush = button.Background as ImageBrush;
                if (brush != null)
                {
                    ImageBrush newBrush = new ImageBrush();
                    if (brush.ImageSource.ToString().Contains("seat_active.png"))
                    {
                        if (returnSeatCount > 4)
                        {
                            MessageBox.Show("Đã chọn đủ số ghế");
                            return;
                        }
                        newBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_selecting.png"));
                        returnSeatPrice += returnTrip.Price;
                        if (!string.IsNullOrEmpty(returnSeatText))
                        {
                            returnSeatText += ",";
                        }
                        returnSeatText += button.Content.ToString();
                        returnSeatIds.Add((string)button.Tag);
                        returnSeatCount++;

                    }
                    else if (brush.ImageSource.ToString().Contains("seat_selecting.png"))
                    {
                        newBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                        returnSeatPrice -= returnTrip.Price;
                        string seatToRemove = button.Content.ToString();
                        List<string> seats = returnSeatText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (seats.Contains(seatToRemove))
                        {
                            seats.Remove(seatToRemove);
                            returnSeatText = string.Join(",", seats);
                        }
                        returnSeatIds.Remove((string)button.Tag);
                        returnSeatCount--;
                    }
                    else
                    {
                        return;
                    }
                    ReturnSeatCountTextBlock.Text = $"{returnSeatCount} Ghế";
                    ReturnSeatPriceTextBlock.Text = $"{returnSeatPrice}đ";
                    ReturnSeatTextBlock.Text = returnSeatText;
                    TotalReturnPriceTextBlock.Text = $"{returnSeatPrice}đ";
                    PaymentFeeTextBlock.Text = $"{paymentFee}đ";
                    TotalPriceTextBlock.Text = $"{departureSeatPrice + returnSeatPrice + paymentFee}đ";
                    button.Background = newBrush;
                }
            }
        }

        private async Task LoadTripDataAsync(string tripId, string? returnTripId = null)
        {
            trip = tripsCollection.Find<Trip>(trip => trip.TripId == tripId).FirstOrDefault();

            if (trip != null)
            {
                var departureProvince = _redisConnection.GetString($"province:{trip.DepartureProvinceCode}:name");
                if (departureProvince == null)
                {
                    departureProvince = (await _postgreSQLConnection.GetProvinceByCodeAsync(trip.DepartureProvinceCode))?.Name;
                    if (departureProvince != null)
                    {
                        _redisConnection.SetString($"province:{trip.DepartureProvinceCode}:name", departureProvince);
                    }

                }
                var destinationProvince = _redisConnection.GetString($"province:{trip.DestinationProvinceCode}:name");
                if (destinationProvince == null)
                {
                    destinationProvince = (await _postgreSQLConnection.GetProvinceByCodeAsync(trip.DestinationProvinceCode))?.Name;
                    if (destinationProvince != null)
                    {
                        _redisConnection.SetString($"province:{trip.DestinationProvinceCode}:name", destinationProvince);
                    }
                }
                if (returnTripId != null)
                {
                    isRoundTrip = true;
                    ReturnTab.Visibility = Visibility.Visible;
                    returnTrip = tripsCollection.Find<Trip>(trip => trip.TripId == returnTripId).FirstOrDefault();
                    ReturnSeatPriceLabelTextBlock.Visibility = Visibility.Visible;
                    ReturnSeatPriceTextBlock.Visibility = Visibility.Visible;
                    ReturnRouteTextBlock.Text = $"{departureProvince} ➜ {destinationProvince}";
                    ReturnDepartureTimeTextBlock.Text = returnTrip.DepartureDate.ToString("HH:mm dd/MM/yyyy");
                    ReturnSeatCountTextBlock.Text = $"{returnSeatCount} Ghế";
                    ReturnSeatPriceTextBlock.Text = $"{returnSeatPrice}đ";
                    TotalReturnPriceTextBlock.Text = $"{returnSeatPrice}đ";
                    ReturnPickUpComboBox.ItemsSource = returnTrip.Transhipments.PickUp.ToList();
                    ReturnDropOffComboBox.ItemsSource = returnTrip.Transhipments.DropOff.ToList();
                }
                // Binding thông tin chuyến đi
                RouteTextBlock.Text = $"{departureProvince} ➜ {destinationProvince}";
                DepartureTimeTextBlock.Text = trip.DepartureDate.ToString("HH:mm dd/MM/yyyy");
                SeatCountTextBlock.Text = $"{departureSeatCount} Ghế";
                TotalDeparturePriceTextBlock.Text = $"{departureSeatPrice}đ";
                DepartureSeatPriceTextBlock.Text = $"{departureSeatPrice}đ";
                PaymentFeeTextBlock.Text = $"{paymentFee}đ";
                TotalPriceTextBlock.Text = $"{returnSeatPrice + departureSeatPrice + paymentFee}đ";
                PickUpComboBox.ItemsSource = trip.Transhipments.PickUp.ToList();
                // Bind drop_off locations to dropOffComboBox
                DropOffComboBox.ItemsSource = trip.Transhipments.DropOff.ToList();
                // Binding cấu hình ghế ngồi
                LoadSeatConfig(trip.SeatConfig, returnTrip?.SeatConfig);
            }
        }

        private void LoadSeatConfig(SeatConfig seatConfig, SeatConfig? returnSeatConfig = null)
        {
            // Tầng dưới
            var floors = seatConfig.Floors;

            foreach (var floor in floors)
            {
                if (floor.Ordinal == 1)
                {
                    DepartureSeatGridBot.Rows = floor.NumRows;
                    DepartureSeatGridBot.Columns = floor.NumCols;
                }
                else if (floor.Ordinal == 2)
                {
                    DepartureSeatGridTop.Rows = floor.NumRows;
                    DepartureSeatGridTop.Columns = floor.NumCols;
                }
                foreach (var seat in seatConfig.Floors.FirstOrDefault(f => f.Ordinal == floor.Ordinal).Seats.Take(floor.NumRows * floor.NumCols))
                {
                    Button seatButton = new Button();
                    seatButton.Tag = seat.SeatId;
                    seatButton.Content = seat.Alias;
                    seatButton.FontSize = 11.5;
                    seatButton.Style = (Style)FindResource("NoHoverButtonStyle");
                    seatButton.Margin = new Thickness(5);
                    seatButton.Click += SeatButton_Click;
                    ImageBrush brush = new ImageBrush();
                    if (_redisConnection.KeyExistsPattern($"booking:*:seat:{seat.SeatId}") || seat.IsSold == true)
                    {
                        brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_disabled.png"));
                    }
                    else
                    {
                        brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                    }
                    seatButton.Background = brush;
                    if (floor.Ordinal == 1)
                    {
                        DepartureSeatGridBot.Children.Add(seatButton);
                    }
                    else if (floor.Ordinal == 2)
                    {
                        DepartureSeatGridTop.Children.Add(seatButton);
                    }
                }
            }

            if (returnSeatConfig != null)
            {
                var returnfloors = returnSeatConfig.Floors;
                foreach (var floor in returnfloors)
                {
                    if (floor.Ordinal == 1)
                    {
                        ReturnSeatGridBot.Rows = floor.NumRows;
                        ReturnSeatGridBot.Columns = floor.NumCols;
                    }
                    else if (floor.Ordinal == 2)
                    {
                        ReturnSeatGridTop.Rows = floor.NumRows;
                        ReturnSeatGridTop.Columns = floor.NumCols;
                    }
                    foreach (var seat in returnSeatConfig.Floors.FirstOrDefault(f => f.Ordinal == floor.Ordinal).Seats.Take(floor.NumRows * floor.NumCols))
                    {

                        Button seatButton = new Button();
                        seatButton.Tag = seat.SeatId;
                        seatButton.Content = seat.Alias;
                        seatButton.FontSize = 11.5;
                        seatButton.Style = (Style)FindResource("NoHoverButtonStyle");
                        seatButton.Margin = new Thickness(5);
                        seatButton.Click += ReturnSeatButton_Click;
                        ImageBrush brush = new ImageBrush();
                        if (_redisConnection.KeyExistsPattern($"booking:*:seat:{seat.SeatId}") || seat.IsSold == true)
                        {
                            brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_disabled.png"));
                        }
                        else
                        {
                            brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                        }
                        seatButton.Background = brush;
                        if (floor.Ordinal == 1)
                        {
                            ReturnSeatGridBot.Children.Add(seatButton);
                        }
                        else if (floor.Ordinal == 2)
                        {
                            ReturnSeatGridTop.Children.Add(seatButton);
                        }
                    }
                }
            }
        }

        private async void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            if (isRoundTrip)
            {
                if (returnSeatCount <= 0)
                {
                    MessageBox.Show("Bạn chưa chọn ghế chuyến về. Vui lòng chọn ghế! ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (departureSeatCount <= 0)
            {
                string message = isRoundTrip ? "Bạn chưa chọn ghế chuyến đi. Vui lòng chọn ghế! " : "Bạn chưa chọn ghế. Vui lòng chọn ghế! ";

                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidName(TxtName.Text))
            {
                MessageBox.Show("Họ và tên không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            // Kiểm tra số điện thoại
            if (!IsValidPhoneNumber(TxtPhoneNumber.Text))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPhoneNumber.Focus();
                return;
            }

            // Kiểm tra email
            if (!IsValidEmail(TxtEmail.Text))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEmail.Focus();
                return;
            }
            if (!chkAccept.IsChecked ?? false)
            {
                MessageBox.Show("Vui lòng chấp nhận điều khoản trước khi đăng ký.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            foreach (var seatId in departureSeatIds.Concat(returnSeatIds))
            {
                if (_redisConnection.KeyExistsPattern($"booking:*:seat:{seatId}"))
                {
                    MessageBox.Show("Ghế này đã được đặt. Vui lòng chọn ghế khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Email = TxtEmail.Text,
                FullName = TxtName.Text,
                PhoneNumber = TxtPhoneNumber.Text,
            };

            await _postgreSQLConnection.AddNewUserAsync(user);
            var createdAt = DateTime.UtcNow;
            var deapartureBooking = new FutaBuss.Model.Booking()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TripId = Guid.Parse(trip.TripId),
                CreatedAt = createdAt
            };

            _redisConnection.CacheBooking(deapartureBooking.Id, user.Id, departureSeatIds);
            await _cassandraDBConnection.AddBookingAsync(deapartureBooking, departureSeatIds);
            if (isRoundTrip)
            {
                var returnBooking = new FutaBuss.Model.Booking()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TripId = Guid.Parse(returnTrip.TripId),
                    CreatedAt = createdAt
                };
                _redisConnection.CacheBooking(returnBooking.Id, user.Id, returnSeatIds);
                await _cassandraDBConnection.AddBookingAsync(returnBooking, returnSeatIds);
            }
            this.NavigationService.Navigate(new PaymentMethod());
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SearchTrips());
        }

        private void TxtName_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!IsValidName(textBox.Text))
            {
                MessageBox.Show("Họ và tên không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TxtPhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!IsValidPhoneNumber(textBox.Text))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!IsValidEmail(textBox.Text))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length > 1;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^[0-9]{10}$"; // Ví dụ: số điện thoại có 10 chữ số
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


    }
}
