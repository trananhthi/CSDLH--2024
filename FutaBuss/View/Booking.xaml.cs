using FutaBuss.DataAccess;
using FutaBuss.Model;
using MongoDB.Driver;
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
        private Trip selectedTrip;
        List<string> selectedSeatIds = new List<string>();
        int price = 0;
        int seatCount = 0;
        int paymentFee = 0;
        string seatText = string.Empty;

        public Booking()
        {
            InitializeComponent();
            InitializeDatabaseConnections();

            tripsCollection = _mongoDBConnection.GetCollection<Trip>("trips");

            LoadTripData("0a8804db96c34be69f3dd8e10515d170");
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
                        price += selectedTrip.Price;
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
                        price -= selectedTrip.Price;
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
                    PriceTextBlock.Text = $"{price}đ";
                    SeatTextBlock.Text = seatText;
                    SeatPriceTextBlock.Text = $"{price}đ";
                    PaymentFeeTextBlock.Text = $"{paymentFee}đ";
                    TotalPriceTextBlock.Text = $"{price + paymentFee}đ";
                    button.Background = newBrush;
                }
            }
        }

        private void LoadTripData(string tripId)
        {
            selectedTrip = tripsCollection.Find<Trip>(trip => trip.TripId == tripId).FirstOrDefault();

            if (selectedTrip != null)
            {
                var departureProvince = _redisConnection.GetString($"province:{selectedTrip.DepartureProvinceCode}:name");
                if (departureProvince == null)
                {
                    departureProvince = _postgreSQLConnection.GetProvinceByCode(selectedTrip.DepartureProvinceCode)?.Name;
                    if (departureProvince != null)
                    {
                        _redisConnection.SetString($"province:{selectedTrip.DepartureProvinceCode}:name", departureProvince);
                    }
                }
                var destinationProvince = _redisConnection.GetString($"province:{selectedTrip.DestinationProvinceCode}:name");
                if (destinationProvince == null)
                {
                    destinationProvince = _postgreSQLConnection.GetProvinceByCode(selectedTrip.DestinationProvinceCode)?.Name;
                    if (destinationProvince != null)
                    {
                        _redisConnection.SetString($"province:{selectedTrip.DestinationProvinceCode}:name", destinationProvince);
                    }
                }
                // Binding thông tin chuyến đi
                RouteTextBlock.Text = $"{departureProvince} ➜ {destinationProvince}";
                DepartureTimeTextBlock.Text = selectedTrip.DepartureDate.ToString("HH:mm dd/MM/yyyy");
                SeatCountTextBlock.Text = $"{seatCount} Ghế";
                PriceTextBlock.Text = $"{price}đ";
                SeatPriceTextBlock.Text = $"{price}đ";
                PaymentFeeTextBlock.Text = $"{paymentFee}đ";
                TotalPriceTextBlock.Text = $"{price + paymentFee}đ";
                PickUpComboBox.ItemsSource = selectedTrip.Transhipments.PickUp.ToList();
                // Bind drop_off locations to dropOffComboBox
                DropOffComboBox.ItemsSource = selectedTrip.Transhipments.DropOff.ToList();
                // Binding cấu hình ghế ngồi
                LoadSeatConfig(selectedTrip.SeatConfig);

            }
        }

        private void LoadSeatConfig(SeatConfig seatConfig)
        {
            // Tầng dưới
            var floors = seatConfig.Floors;
            foreach (var floor in floors)
            {
                if (floor.Ordinal == 1)
                {
                    SeatGridBot.Rows = floor.NumRows;
                    SeatGridBot.Columns = floor.NumCols;
                }
                else if (floor.Ordinal == 2)
                {
                    SeatGridTop.Rows = floor.NumRows;
                    SeatGridTop.Columns = floor.NumCols;
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
                        SeatGridBot.Children.Add(seatButton);
                    }
                    else if (floor.Ordinal == 2)
                    {
                        SeatGridTop.Children.Add(seatButton);
                    }
                }
            }
        }

        private void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var window = new MainWindow(selectedSeatIds);
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
