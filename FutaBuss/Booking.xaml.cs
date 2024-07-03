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

            LoadTripData();
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

        private void CreateButtons(bool top = true)
        {
            for (int i = 1; i <= 18; i++)
            {
                Button button = new Button
                {
                    Content = top ? $"B{i:00}" : $"A{i:00}",
                    Style = (Style)FindResource("NoHoverButtonStyle"),
                    Margin = new Thickness(5),
                };
                button.Click += SeatButton_Click;
                if (top)
                {
                    SeatGridTop.Children.Add(button);
                }
                else
                {
                    SeatGridBot.Children.Add(button);
                }

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
                        newBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_selecting.png"));
                        price += selectedTrip.Price;
                        seatCount++;

                    }
                    else if (brush.ImageSource.ToString().Contains("seat_selecting.png"))
                    {
                        newBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_active.png"));
                        price -= selectedTrip.Price;
                        seatCount--;
                    }
                    SeatCountTextBlock.Text = $"{seatCount} Ghế";
                    PriceTextBlock.Text = $"{price}d";
                    button.Background = newBrush;
                }
            }
        }

        private void LoadTripData()
        {
            // Giả sử bạn đã có tripId từ đâu đó (ví dụ: người dùng chọn từ danh sách chuyến)
            string tripId = "1df7695e3bf3415fb9b29c903fbe438a";  // Thay thế bằng ID thực tế

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
                Button seatButton = new Button();
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
                SeatGridBot.Children.Add(seatButton);
            }

            // Tầng trên
            foreach (var seat in seatConfig.Floors.FirstOrDefault(f => f.Ordinal == 2).Seats)
            {
                Button seatButton = new Button();
                seatButton.Content = seat.Alias;
                seatButton.Style = (Style)FindResource("NoHoverButtonStyle");
                seatButton.Margin = new Thickness(5);
                seatButton.Click += SeatButton_Click;
                ImageBrush brush = new ImageBrush();
                if (seat.Status == "empty")
                {
                    brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_disabled.png"));
                }
                else
                {
                    brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/seat_disabled.png"));
                }
                seatButton.Background = brush;
                SeatGridTop.Children.Add(seatButton);
            }
        }
    }
}
