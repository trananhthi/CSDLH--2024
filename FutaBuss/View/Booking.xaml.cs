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
        int price = 0;
        int seatCount = 0;

        public Booking()
        {
            InitializeComponent();
            InitializeDatabaseConnections();

            tripsCollection = _mongoDBConnection.GetCollection<Trip>("trips");
        }

        public Booking(string tripId) : this()
        {
            LoadTripData(tripId);
        }

        private void InitializeDatabaseConnections()
        {
            try
            {
                // Kết nối MongoDB
                _mongoDBConnection = new MongoDBConnection("mongodb+srv://thuannt:J396QWpWuiGDZhOs@thuannt.yzjzr9s.mongodb.net/?appName=ThuanNT", "futabus");

                _redisConnection = new RedisConnection("redis-18667.c8.us-east-1-2.ec2.cloud.redislabs.com:18667", "default", "dVZCrABvG85l0L9JQI9izqn2SDvvTx82");

                _postgreSQLConnection = new PostgreSQLConnection("Host=dpg-cq12053v2p9s73cjijm0-a.singapore-postgres.render.com;Username=root;Password=vTwWs92lObTZrhI9IFcJGXJxZCdzeBas;Database=mds_postpresql");
                _postgreSQLConnection.OpenConnection();
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

        private void LoadTripData(string tripId)
        {
            // Truy vấn dữ liệu chuyến đi từ MongoDB
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
                PriceTextBlock.Text = $"{price}d";

                // Binding cấu hình ghế ngồi
                LoadSeatConfig(selectedTrip.SeatConfig);
            }
        }

        private void LoadSeatConfig(SeatConfig seatConfig)
        {
            // Tầng dưới
            foreach (var seat in seatConfig.Floors.FirstOrDefault(f => f.Ordinal == 1).Seats)
            {
                Button seatButton = new Button
                {
                    Content = seat.Alias,
                    Style = (Style)FindResource("NoHoverButtonStyle"),
                    Margin = new Thickness(5),
                    Background = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(seat.Status == "empty"
                            ? "pack://application:,,,/Images/seat_active.png"
                            : "pack://application:,,,/Images/seat_disabled.png"))
                    }
                };
                seatButton.Click += SeatButton_Click;
                SeatGridBot.Children.Add(seatButton);
            }

            // Tầng trên
            foreach (var seat in seatConfig.Floors.FirstOrDefault(f => f.Ordinal == 2).Seats)
            {
                Button seatButton = new Button
                {
                    Content = seat.Alias,
                    Style = (Style)FindResource("NoHoverButtonStyle"),
                    Margin = new Thickness(5),
                    Background = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(seat.Status == "empty"
                            ? "pack://application:,,,/Images/seat_active.png"
                            : "pack://application:,,,/Images/seat_disabled.png"))
                    }
                };
                seatButton.Click += SeatButton_Click;
                SeatGridTop.Children.Add(seatButton);
            }
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Background is ImageBrush brush)
            {
                ImageBrush newBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(brush.ImageSource.ToString().Contains("seat_active.png")
                        ? "pack://application:,,,/Images/seat_selecting.png"
                        : "pack://application:,,,/Images/seat_active.png"))
                };

                price += brush.ImageSource.ToString().Contains("seat_active.png") ? selectedTrip.Price : -selectedTrip.Price;
                seatCount += brush.ImageSource.ToString().Contains("seat_active.png") ? 1 : -1;

                SeatCountTextBlock.Text = $"{seatCount} Ghế";
                PriceTextBlock.Text = $"{price}d";
                button.Background = newBrush;
            }
        }
    }
}
