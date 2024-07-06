using FutaBuss.DataAccess;
using FutaBuss.Model;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FutaBuss
{
    /// <summary>
    /// Interaction logic for Booking.xaml
    /// </summary>
    public partial class Booking : Window
    {
        private MongoDBConnection _mongoDBConnection;
        private RedisConnection _redisConnection;
        private PostgreSQLConnection _postgreSQLConnection;
        private MongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<Trip> tripsCollection;
        private Trip trip;
        private Trip returnTrip;
        List<string> selectedSeatIds = new List<string>();
        int seatCount = 0;
        int returnSeatCount = 0;
        int departureSeatPrice = 0;
        int returnSeatPrice = 0;
        int paymentFee = 0;
        string seatText = string.Empty;
        string returnSeatText = string.Empty;
        public Booking()
        {
            InitializeComponent();
            InitializeDatabaseConnections();

            tripsCollection = _mongoDBConnection.GetCollection<Trip>("trips");

            LoadTripData("0a8804db96c34be69f3dd8e10515d170", "1df7695e3bf3415fb9b29c903fbe438a");
        }

        private void InitializeDatabaseConnections()
        {
            try
            {
                // Kết nối MongoDB
                _mongoDBConnection = new MongoDBConnection("mongodb+srv://thuannt:J396QWpWuiGDZhOs@thuannt.yzjzr9s.mongodb.net/?appName=ThuanNT", "futabus");

                _redisConnection = new RedisConnection("redis-18667.c8.us-east-1-2.ec2.cloud.redislabs.com:18667", "default", "dVZCrABvG85l0L9JQI9izqn2SDvvTx82");

                _postgreSQLConnection = new PostgreSQLConnection("Host=dpg-cq12053v2p9s73cjijm0-a.singapore-postgres.render.com;Username=root;Password=vTwWs92lObTZrhI9IFcJGXJxZCdzeBas;Database=mds_postpresql");


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
                        if (seatCount > 4)
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
                        selectedSeatIds.Add((string)button.Tag);
                        seatCount++;

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
                        selectedSeatIds.Remove((string)button.Tag);
                        seatCount--;
                    }
                    SeatCountTextBlock.Text = $"{seatCount} Ghế";
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
                        selectedSeatIds.Add((string)button.Tag);
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
                        selectedSeatIds.Remove((string)button.Tag);
                        returnSeatCount--;
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

        private void LoadTripData(string tripId, string? returnTripId = null)
        {
            trip = tripsCollection.Find<Trip>(trip => trip.TripId == tripId).FirstOrDefault();

            if (trip != null)
            {
                var departureProvince = _redisConnection.GetString($"province:{trip.DepartureProvinceCode}:name");
                if (departureProvince == null)
                {
                    departureProvince = _postgreSQLConnection.GetProvinceByCode(trip.DepartureProvinceCode)?.Name;
                    if (departureProvince != null)
                    {
                        _redisConnection.SetString($"province:{trip.DepartureProvinceCode}:name", departureProvince);
                    }

                }
                var destinationProvince = _redisConnection.GetString($"province:{trip.DestinationProvinceCode}:name");
                if (destinationProvince == null)
                {
                    destinationProvince = _postgreSQLConnection.GetProvinceByCode(trip.DestinationProvinceCode)?.Name;
                    if (destinationProvince != null)
                    {
                        _redisConnection.SetString($"province:{trip.DestinationProvinceCode}:name", destinationProvince);
                    }
                }
                if (returnTripId != null)
                {
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
                SeatCountTextBlock.Text = $"{seatCount} Ghế";
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
                foreach (var seat in seatConfig.Floors.FirstOrDefault(f => f.Ordinal == floor.Ordinal).Seats)
                {
                    Button seatButton = new Button();
                    seatButton.Tag = seat.SeatId;
                    seatButton.Content = seat.Alias;
                    seatButton.Style = (Style)FindResource("NoHoverButtonStyle");
                    seatButton.Margin = new Thickness(5);
                    seatButton.Click += SeatButton_Click;
                    ImageBrush brush = new ImageBrush();
                    if (seat.Status == "empty")
                    {
                        brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                    }
                    else
                    {
                        brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_disabled.png"));
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
                        DepartureSeatGridBot.Rows = floor.NumRows;
                        DepartureSeatGridBot.Columns = floor.NumCols;
                    }
                    else if (floor.Ordinal == 2)
                    {
                        DepartureSeatGridTop.Rows = floor.NumRows;
                        DepartureSeatGridTop.Columns = floor.NumCols;
                    }
                    foreach (var seat in returnSeatConfig.Floors.FirstOrDefault(f => f.Ordinal == floor.Ordinal).Seats)
                    {
                        Button seatButton = new Button();
                        seatButton.Tag = seat.SeatId;
                        seatButton.Content = seat.Alias;
                        seatButton.Style = (Style)FindResource("NoHoverButtonStyle");
                        seatButton.Margin = new Thickness(5);
                        seatButton.Click += ReturnSeatButton_Click;
                        ImageBrush brush = new ImageBrush();
                        if (seat.Status == "empty")
                        {
                            brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                        }
                        else
                        {
                            brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_disabled.png"));
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

        private void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidName(TxtName.Text))
            {
                MessageBox.Show("Họ và tên không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra số điện thoại
            if (!IsValidPhoneNumber(TxtPhoneNumber.Text))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra email
            if (!IsValidEmail(TxtEmail.Text))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!chkAccept.IsChecked ?? false)
            {
                MessageBox.Show("Vui lòng chấp nhận điều khoản trước khi đăng ký.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            User user = new User()
            {
                Email = TxtEmail.Text,
                FullName = TxtName.Text,
                PhoneNumber = TxtPhoneNumber.Text,
            };
            int? userId = _postgreSQLConnection.AddNewUser(user);
            if (userId == null)
            {
                MessageBox.Show("Có lỗi xảy ra trong hệ thống", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Guid booking_id = Guid.NewGuid();
            _redisConnection.CacheBooking(booking_id, userId, selectedSeatIds);
            this.Hide();
            var window = new MainWindow(selectedSeatIds);
            window.Owner = this;
            window.ShowDialog();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
