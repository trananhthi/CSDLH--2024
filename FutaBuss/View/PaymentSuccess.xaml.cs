using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZXing.QrCode;
using ZXing;
using ZXing.Windows.Compatibility;
using FutaBuss.DataAccess;
using FutaBuss.Model;
using System.Net.Sockets;

namespace FutaBuss.View
{
    /// <summary>
    /// Interaction logic for PaymentSuccess.xaml
    /// </summary>
    public partial class PaymentSuccess : Page
    {
        public ObservableCollection<TicketItem> Items { get; set; }
        private int currentIndex = 0;
        private int itemsPerPage = 4;

        private List<Ticket> tickets = new List<Ticket>();
        private FutaBuss.Model.Trip trip;

        private MongoDBConnection _mongoDBConnection;
        private PostgreSQLConnection _postgreSQLConnection;
        private CassandraDBConnection _cassandraDBConnection;

        public PaymentSuccess(Customer customer, int totalPrice, string paymentMethod, Guid paymentId)
        {
            InitializeComponent();
            Items = new ObservableCollection<TicketItem>();
            InitializeDatabaseConnections();
            FullName.Text = customer.FullName;
            PhoneNumber.Text = customer.PhoneNumber;
            Email.Text = customer.Email;

            TotalPrice.Text = $"{(totalPrice):#,0}đ";
            PaymentMethod.Text = paymentMethod;
            DataContext = this;
            
            tickets = GetAllTicket(paymentId);



            

            InitializeAsync(tickets);


        }


        private void InitializeDatabaseConnections()
        {
            try
            {
                _mongoDBConnection = MongoDBConnection.Instance;
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


        public async Task InitializeAsync(List<Ticket> tickets)
        {

            for (int index = 0; index < tickets.Count; index++)
            {
                GenerateQRCode(tickets[index].Id.ToString() + tickets[index].CustomerId.ToString() + tickets[index].TripId, index.ToString());
                trip = await GetTripAsync(tickets[index].TripId);


                string departure_province = await GetProvinceName(trip.DepartureProvinceCode);
                string destination_province = await GetProvinceName(trip.DestinationProvinceCode);



                var ticketItem = new TicketItem
                {
                    TicketId = "Mã vé " + tickets[index].Id.ToString().Substring(0, 5).ToUpper(),
                    Buses = departure_province + " - " + destination_province,
                    Time = $"{trip.DepartureTime:hh\\:mm} {trip.DepartureDate:dd/MM/yyyy}",
                    NoSeat = GetSeatAlias(tickets[index].SeatId, trip),
                    Place = GetPickUpPlace(tickets[index].PickUpLocationId, trip),
                    Price = $"{(trip.Price):#,0}đ",
                    ImageSource = loadQR($"{index}qrcode.png")
                };
                Items.Add(ticketItem);
                
            }

            UpdateCarouselView();


        }

        private List<Ticket> GetAllTicket(Guid paymentId)
        {
            try
            {
                return _cassandraDBConnection.GetAllTicketByPaymentId(paymentId);
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


        public void GenerateQRCode(string data,string fileName)
        {

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

            string filePath = System.IO.Path.Combine("..\\..\\..", "Images", $"{fileName}qrcode.png");

            Bitmap qrCodeBitmap = writer.Write(data);
            qrCodeBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        }

        private BitmapImage loadQR(string fileName)
        {
            string imagePath = System.IO.Path.Combine("..\\..\\..", "Images", fileName);
            BitmapImage bitmapImage;

            if (System.IO.File.Exists(imagePath))
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            else
            {
                throw new Exception($"File '{imagePath}' not found.");
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex -= itemsPerPage;
                UpdateCarouselView();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex + itemsPerPage < Items.Count)
            {
                currentIndex += itemsPerPage;
                UpdateCarouselView();
            }
        }

        private void UpdateCarouselView()
        {
            // Clear previous items
            carouselListView.ItemsSource = null;

            // Get items for the current page
            var itemsToShow = Items.Skip(currentIndex).Take(itemsPerPage).ToList();

            // Update ListView
            carouselListView.ItemsSource = itemsToShow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FutaBuss.View.SearchTrips());
        }
    }



    public class TicketItem
    {
        public string TicketId { get; set; }
        public string Buses { get; set; }
        public string Time { get; set; }

        public string NoSeat { get; set; }
        public string Place { get; set; }
        public string Price { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
}
