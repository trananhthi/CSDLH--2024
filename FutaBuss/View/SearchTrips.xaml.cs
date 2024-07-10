using FutaBuss.DataAccess;
using FutaBuss.Model;
using MongoDB.Driver;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

namespace FutaBuss.View
{
    /// <summary>
    /// Interaction logic for SearchTrips.xaml
    /// </summary>
    public partial class SearchTrips : Page
    {
        private MongoDBConnection _mongoDBConnection;
        private RedisConnection _redisConnection;
        private PostgreSQLConnection _postgreSQLConnection;
        //private MongoClient client;
        //private IMongoDatabase database;
        private IMongoCollection<Trip> tripsCollection;
        private Dictionary<string, string> _provinceDictionary;
        private List<Trip> _originalTrips;
        private List<Trip> _originalRoundTrips;
        private string _departureTripid;
        private string _returnTripid;

        public SearchTrips()
        {
            InitializeComponent();
            InitializeDatabaseConnections();

            tripsCollection = _mongoDBConnection.GetCollection<Trip>("trips");

            CacheProvinceAsync();
            LoadProvinces();
        }

        private void OneWayRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ReturnDatePanel != null)
            {
                Grid inputTripInformationGrid = FindName("inputTripInformationGrid") as Grid;
                inputTripInformationGrid.ColumnDefinitions.Clear();
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                ReturnDatePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void RoundTripRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ReturnDatePanel != null)
            {
                Grid inputTripInformationGrid = FindName("inputTripInformationGrid") as Grid;
                inputTripInformationGrid.ColumnDefinitions.Clear();
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                inputTripInformationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                ReturnDatePanel.Visibility = Visibility.Visible;
            }
        }
        private void SwapButton_Click(object sender, RoutedEventArgs e)
        {
            // Lưu trữ tạm thời giá trị hiện tại của các ComboBox
            var tempDeparture = DepartureComboBox.SelectedValue;
            var tempDestination = DestinationComboBox.SelectedValue;

            // Hoán đổi giá trị
            DepartureComboBox.SelectedValue = tempDestination;
            DestinationComboBox.SelectedValue = tempDeparture;
        }

        private async Task CacheProvinceAsync()
        {
            var listProvince = new List<Province>();
            listProvince = await _postgreSQLConnection.GetProvincesAsync();
            foreach (var province in listProvince)
            {
                var name = _redisConnection.GetString($"province:{province.Code}:name");
                if (name == null)
                {
                    _redisConnection.SetString($"province:{province.Code}:name", province.Name);
                }
            }
        }

        private void InitializeDatabaseConnections()
        {
            try
            {
                _mongoDBConnection = MongoDBConnection.Instance;
                _redisConnection = RedisConnection.Instance;
                _postgreSQLConnection = PostgreSQLConnection.Instance;

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

        private void LoadProvinces()
        {
            // Đây là danh sách mẫu, bạn có thể lấy từ Redis hoặc nguồn khác
            var provinces = _redisConnection.GetAllProvinces();
            _provinceDictionary = provinces.ToDictionary(p => p.Code, p => p.Name);
            // Gán danh sách provinces cho các ComboBox
            DepartureComboBox.ItemsSource = provinces;
            DestinationComboBox.ItemsSource = provinces;
        }

        //private async void SearchTripsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ClearFilters();
        //    TextBlock noResultTextBlock = FindName("NoResultTextBlock") as TextBlock;
        //    noResultTextBlock.Visibility = Visibility.Collapsed;

        //    string departure = DepartureComboBox.SelectedValue as string;
        //    string destination = DestinationComboBox.SelectedValue as string;
        //    DateTime? departureDate = DepartureDatePicker.SelectedDate;
        //    DateTime? returnDate = ReturnDatePicker.SelectedDate;


        //    if (departureDate == null)
        //    {
        //        MessageBox.Show("Vui lòng chọn ngày khởi hành.");
        //        return;
        //    }

        //    string departureDateString = departureDate.Value.ToString("yyyy-MM-dd");
        //    string returnDateString = returnDate.Value.ToString("yyyy-MM-dd");
        //    int ticketCount = int.Parse(((ComboBoxItem)TicketCountComboBox.SelectedItem).Content.ToString());

        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    try
        //    {
        //        List<Trip> trips = await _mongoDBConnection.SearchTripsAsync(departure, destination, departureDateString);
        //        var filteredTrips = trips.Where(trip => CountEmptySeats(trip) >= ticketCount).ToList();
        //        _originalTrips = filteredTrips;
        //        if (RoundTrip.IsChecked == true)
        //        {
        //            List<Trip> roundTrips = await _mongoDBConnection.SearchTripsAsync(destination, departure, returnDateString);
        //            var filteredRoundTrips = roundTrips.Where(trip => CountEmptySeats(trip) >= ticketCount).ToList();

        //            _originalRoundTrips = filteredRoundTrips;
        //            DisplayRoundTrips(filteredTrips, filteredRoundTrips);
        //        }
        //        else
        //        {
        //            DisplayTrips(filteredTrips);
        //        }

        //        stopwatch.Stop();

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi khi tìm kiếm chuyến: {ex.Message}");
        //    }
        //}


        private async void SearchTripsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
            TextBlock noResultTextBlock = FindName("NoResultTextBlock") as TextBlock;
            noResultTextBlock.Visibility = Visibility.Collapsed;

            string departure = DepartureComboBox.SelectedValue as string;
            string destination = DestinationComboBox.SelectedValue as string;
            DateTime? departureDate = DepartureDatePicker.SelectedDate;
            DateTime? returnDate = ReturnDatePicker.SelectedDate;


            if (departureDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày khởi hành.");
                return;
            }

            string departureDateString = departureDate.Value.ToString("yyyy-MM-dd");
            string returnDateString = returnDate.Value.ToString("yyyy-MM-dd");
            int ticketCount = int.Parse(((ComboBoxItem)TicketCountComboBox.SelectedItem).Content.ToString());

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                List<Trip> trips = await _mongoDBConnection.SearchTripsAsync(departure, destination, departureDateString);
                //var filteredTrips = trips.Where(trip => CountEmptySeats(trip) >= ticketCount).ToList();
                _originalTrips = trips;
                if (RoundTrip.IsChecked == true)
                {
                    List<Trip> roundTrips = await _mongoDBConnection.SearchTripsAsync(destination, departure, returnDateString);
                    //var filteredRoundTrips = roundTrips.Where(trip => CountEmptySeats(trip) >= ticketCount).ToList();

                    _originalRoundTrips = roundTrips;
                    DisplayRoundTrips(trips, roundTrips);

                    stopwatch.Stop();
                }
                else
                {
                    DisplayTrips(trips);

                    stopwatch.Stop();
                }
                Debug.WriteLine($"Thời gian tìm chuyến: {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chuyến: {ex.Message}");
            }
        }

        private void SelectTrip(Trip trip, string tripType)
        {
            if (tripType == "Chuyến đi")
            {
                _departureTripid = trip.Id;
            }
            else
            {
                _returnTripid = trip.Id;
            }

            var isRoundTrip = RoundTrip.IsChecked;

            if (isRoundTrip == true)
            {
                if (_departureTripid == null)
                {
                    MessageBox.Show("Bạn chưa chọn chuyến đi. Vui lòng chọn chuyến đi! ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else if (_returnTripid == null)
                {
                    MessageBox.Show("Bạn chưa chọn chuyến về. Vui lòng chọn chuyến về! ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            Debug.WriteLine($"{_departureTripid}, {_returnTripid}");

            this.NavigationService.Navigate(new FutaBuss.View.Booking(_departureTripid, _returnTripid));
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
        }

        private void ClearFilters()
        {
            // Set tất cả các checkbox.IsChecked = false
            EarlyMorningCheckBox.IsChecked = false;
            MorningCheckBox.IsChecked = false;
            AfternoonCheckBox.IsChecked = false;
            EveningCheckBox.IsChecked = false;

            // Set tất cả các button.Tag = "inactive" và cập nhật trạng thái giao diện
            ResetButtonState(SeatedBusButton);
            ResetButtonState(SleeperBusButton);
            ResetButtonState(LimousineButton);
            ResetButtonState(MiddleRowButton);
            ResetButtonState(FrontRowButton);
            ResetButtonState(LastRowButton);
            ResetButtonState(UpperFloorButton);
            ResetButtonState(LowerFloorButton);

            // Gọi lại hàm ApplyFilters để cập nhật kết quả
            ApplyFilters();
        }

        private void ResetButtonState(Button button)
        {
            button.FontWeight = FontWeights.Normal;
            button.BorderThickness = new Thickness(0);
            button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            button.Tag = "inactive";

            var buttonName = button.Name;
            var boderButtonName = $"{buttonName}Border";

            Border buttonBorDer = FindName($"{boderButtonName}") as Border;
            buttonBorDer.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD3D3D3"));
        }

        private int CountEmptySeats(Trip trip)
        {
            int emptySeatCount = 0;
            var floors = trip.SeatConfig.Floors;

            foreach (var floor in floors)
            {

                foreach (var seat in floor.Seats)
                {
                    if (!(_redisConnection.KeyExistsPattern($"booking:*:seat:{seat.SeatId}") || seat.IsSold == true))
                    {
                        emptySeatCount++;
                    }
                }
            }

            return emptySeatCount;
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Lấy button được nhấn
            Button clickedButton = sender as Button;
            clickedButton.Tag = "active";

            clickedButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));
            var buttonName = clickedButton.Name;
            var boderButtonName = $"{buttonName}Border";

            Border buttonBorDer = FindName($"{boderButtonName}") as Border;
            buttonBorDer.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));

            // Gọi lại hàm ApplyFilters để áp dụng bộ lọc
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // Lấy tất cả các điều kiện lọc từ các CheckBox
            bool earlyMorning = EarlyMorningCheckBox.IsChecked ?? false;
            bool morning = MorningCheckBox.IsChecked ?? false;
            bool afternoon = AfternoonCheckBox.IsChecked ?? false;
            bool evening = EveningCheckBox.IsChecked ?? false;
            if (earlyMorning == false && morning == false && afternoon == false && evening == false)
            {
                earlyMorning = true;
                morning = true;
                afternoon = true;
                evening = true;
            }

            bool seatedBus = SeatedBusButton.Tag?.ToString() == "active";
            bool sleeperBus = SleeperBusButton.Tag?.ToString() == "active";
            bool limousine = LimousineButton.Tag?.ToString() == "active";

            if (!seatedBus && !sleeperBus && !limousine)
            {
                seatedBus = true;
                sleeperBus = true;
                limousine = true;
            }

            bool middleRow = MiddleRowButton.Tag?.ToString() == "active";
            bool frontRow = FrontRowButton.Tag?.ToString() == "active";
            bool lastRow = LastRowButton.Tag?.ToString() == "active";

            if (!middleRow && !frontRow && !lastRow)
            {
                middleRow = true;
                frontRow = true;
                lastRow = true;
            }

            bool upperFloor = UpperFloorButton.Tag?.ToString() == "active";
            bool lowerFloor = LowerFloorButton.Tag?.ToString() == "active";

            if (!upperFloor && !lowerFloor)
            {
                upperFloor = true;
                lowerFloor = true;
            }

            // Khởi tạo FilteredTrips là ObservableCollection<BsonDocument>
            List<Trip> FilteredTrips = new List<Trip>();
            List<Trip> FilteredRoundTrips = new List<Trip>();


            // Lặp qua từng chuyến xe trong _originalTrips
            if (_originalTrips != null)
            {
                foreach (var trip in _originalTrips)
                {
                    // Parse departure time and date
                    DateTime departureDate = DateTime.Parse(trip.DepartureDate.ToString());
                    TimeSpan departureTime = TimeSpan.Parse(trip.DepartureTime.ToString());
                    DateTime departureDateTime = departureDate.Add(departureTime);

                    string tripType = trip.TripType.ToString();

                    // Time condition
                    bool timeCondition = (earlyMorning && departureDateTime.Hour >= 0 && departureDateTime.Hour < 6) ||
                                         (morning && departureDateTime.Hour >= 6 && departureDateTime.Hour < 12) ||
                                         (afternoon && departureDateTime.Hour >= 12 && departureDateTime.Hour < 18) ||
                                         (evening && departureDateTime.Hour >= 18 && departureDateTime.Hour < 24);

                    // Type condition
                    bool typeCondition = (seatedBus && tripType == "seated_bus") ||
                                         (sleeperBus && tripType == "sleeper_bus") ||
                                         (limousine && tripType == "limousine");

                    // Row condition
                    bool rowCondition = false;
                    var floorsArray = trip.SeatConfig.Floors;
                    foreach (var floor in floorsArray)
                    {
                        var seatsArray = floor.Seats;
                        if ((middleRow && seatsArray.Any(seat => seat.RowGroup == "mid")) ||
                            (frontRow && seatsArray.Any(seat => seat.RowGroup == "head")) ||
                            (lastRow && seatsArray.Any(seat => seat.RowGroup == "tail")))
                        {
                            rowCondition = true;
                            break;
                        }
                    }

                    // Floor condition
                    bool floorCondition = (upperFloor && floorsArray.Any(floor => floor.Ordinal == 1)) ||
                                          (lowerFloor && floorsArray.Any(floor => floor.Ordinal == 2));

                    // Kiểm tra tất cả các điều kiện
                    if (timeCondition && typeCondition && rowCondition && floorCondition)
                    {
                        // Nếu tất cả điều kiện đều đúng, thêm chuyến xe vào FilteredTrips
                        FilteredTrips.Add(trip);
                    }
                }
            }
            if (RoundTrip.IsChecked == true)
            {

                if (_originalRoundTrips != null)
                {
                    foreach (var trip in _originalRoundTrips)
                    {
                        // Parse departure time and date
                        DateTime departureDate = DateTime.Parse(trip.DepartureDate.ToString());
                        TimeSpan departureTime = TimeSpan.Parse(trip.DepartureTime.ToString());
                        DateTime departureDateTime = departureDate.Add(departureTime);

                        string tripType = trip.TripType.ToString();

                        // Time condition
                        bool timeCondition = (earlyMorning && departureDateTime.Hour >= 0 && departureDateTime.Hour < 6) ||
                                             (morning && departureDateTime.Hour >= 6 && departureDateTime.Hour < 12) ||
                                             (afternoon && departureDateTime.Hour >= 12 && departureDateTime.Hour < 18) ||
                                             (evening && departureDateTime.Hour >= 18 && departureDateTime.Hour < 24);

                        // Type condition
                        bool typeCondition = (seatedBus && tripType == "seated_bus") ||
                                             (sleeperBus && tripType == "sleeper_bus") ||
                                             (limousine && tripType == "limousine");

                        // Row condition
                        bool rowCondition = false;
                        var floorsArray = trip.SeatConfig.Floors;
                        foreach (var floor in floorsArray)
                        {
                            var seatsArray = floor.Seats;
                            if ((middleRow && seatsArray.Any(seat => seat.RowGroup == "mid")) ||
                                (frontRow && seatsArray.Any(seat => seat.RowGroup == "head")) ||
                                (lastRow && seatsArray.Any(seat => seat.RowGroup == "tail")))
                            {
                                rowCondition = true;
                                break;
                            }
                        }
                        // Floor condition
                        bool floorCondition = (upperFloor && floorsArray.Any(floor => floor.Ordinal == 1)) ||
                                              (lowerFloor && floorsArray.Any(floor => floor.Ordinal == 2));

                        // Kiểm tra tất cả các điều kiện
                        if (timeCondition && typeCondition && rowCondition && floorCondition)
                        {
                            // Nếu tất cả điều kiện đều đúng, thêm chuyến xe vào FilteredTrips
                            FilteredRoundTrips.Add(trip);
                        }
                    }
                }

                // Hiển thị các chuyến xe đã lọc

                DisplayRoundTrips(FilteredTrips, FilteredRoundTrips);
                return;
            }
            DisplayTrips(FilteredTrips);
        }

        private void DisplayTrips(List<Trip> trips)
        {
            StackPanel resultPanel = FindName("ResultPanel") as StackPanel;
            TextBlock noResultTextBlock = FindName("NoResultTextBlock") as TextBlock;

            if (trips.Count == 0)
            {
                noResultTextBlock.Visibility = Visibility.Visible;
                return;
            }

            resultPanel.Children.Clear();
            if (trips.Count > 0)
            {
                string departureProvinceCode = trips[0].DepartureProvinceCode.ToString();
                string destinationProvinceCode = trips[0].DestinationProvinceCode.ToString();

                _provinceDictionary.TryGetValue(departureProvinceCode, out string departureProvinceName);
                _provinceDictionary.TryGetValue(destinationProvinceCode, out string destinationProvinceName);

                string tripInfo = $"{(departureProvinceName ?? departureProvinceCode)} - {(destinationProvinceName ?? destinationProvinceCode)} ({trips.Count} chuyến)";

                TextBlock tripInfoTextBlock = new TextBlock
                {
                    Text = tripInfo,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                resultPanel.Children.Add(tripInfoTextBlock);
            }

            foreach (var trip in trips)
            {
                Border tripBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Background = Brushes.White,
                    Padding = new Thickness(10),
                    Margin = new Thickness(10, 0, 10, 10)
                };

                StackPanel tripStack = new StackPanel();

                Grid tripGrid = new Grid();
                tripGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tripGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Thời gian khởi hành
                TextBlock departureTime = new TextBlock
                {
                    Text = trip.DepartureTime.ToString(),
                    Margin = new Thickness(0, 10, 130, 16),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(departureTime, 0);
                Grid.SetColumn(departureTime, 0);
                Grid.SetRowSpan(departureTime, 2);
                tripGrid.Children.Add(departureTime);

                // Thời gian di chuyển
                string travelTime = "---------->";
                TextBlock travelTimeBlock = new TextBlock
                {
                    Text = travelTime,
                    Margin = new Thickness(161, 10, 22, 0),
                    Width = 60,
                    VerticalAlignment = VerticalAlignment.Center
                };
                //Grid.SetRow(travelTimeBlock, 0);
                //Grid.SetColumn(travelTimeBlock, 1);
                tripGrid.Children.Add(travelTimeBlock);

                // Thời gian đến
                TextBlock arrivalTime = new TextBlock
                {
                    Text = trip.ExpectedArrivalTime.ToString(),
                    Margin = new Thickness(0, 10, 130, 16),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(arrivalTime, 0);
                Grid.SetColumn(arrivalTime, 2);
                Grid.SetRowSpan(arrivalTime, 2);
                tripGrid.Children.Add(arrivalTime);

                // Loại xe
                string tripTypeCode = trip.TripType.ToString();
                string tripTypeText = tripTypeCode switch
                {
                    "seated_bus" => "Ghế",
                    "sleeper_bus" => "Giường",
                    "limousine" => "Limousine",
                    _ => tripTypeCode
                };
                TextBlock tripType = new TextBlock
                {
                    Text = tripTypeText,
                    Margin = new Thickness(139, 10, 196, 1),
                    Width = 100,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(tripType, 0);
                Grid.SetColumn(tripType, 2);
                Grid.SetColumnSpan(tripType, 2);
                tripGrid.Children.Add(tripType);

                // Số giường
                int totalEmptySeats = CountEmptySeats(trip);
                string totalEmptySeatsText = tripTypeCode switch
                {
                    "seated_bus" => $"Ghế trống: {totalEmptySeats}",
                    "sleeper_bus" => $"Giường trống: {totalEmptySeats}",
                    "limousine" => $"Giường trống: {totalEmptySeats}",
                    _ => tripTypeCode
                };
                TextBlock seatCount = new TextBlock
                {
                    Text = totalEmptySeatsText,
                    Margin = new Thickness(242, 10, -1, 16),
                    Width = 150,
                    TextAlignment = TextAlignment.Right
                };
                Grid.SetRowSpan(seatCount, 2);
                Grid.SetColumn(seatCount, 2);
                Grid.SetColumnSpan(seatCount, 2);
                tripGrid.Children.Add(seatCount);

                // Nơi đi
                string departureProvinceCode = trip.DepartureProvinceCode;
                _provinceDictionary.TryGetValue(departureProvinceCode, out string departureProvinceName);
                TextBlock departureProvince = new TextBlock
                {
                    Text = departureProvinceName ?? departureProvinceCode,
                    Margin = new Thickness(0, 10, 93, 0),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(departureProvince, 1);
                Grid.SetColumn(departureProvince, 0);
                tripGrid.Children.Add(departureProvince);

                // Nơi đến
                string destinationProvinceCode = trip.DestinationProvinceCode;
                _provinceDictionary.TryGetValue(destinationProvinceCode, out string destinationProvinceName);
                TextBlock destinationProvince = new TextBlock
                {
                    Text = destinationProvinceName ?? destinationProvinceCode,
                    Margin = new Thickness(0, 10, 92, 0),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(destinationProvince, 1);
                Grid.SetColumn(destinationProvince, 2);
                tripGrid.Children.Add(destinationProvince);

                // Giá vé
                TextBlock price = new TextBlock
                {
                    Text = trip.Price.ToString() + " VND",
                    Margin = new Thickness(93, 10, 0, 0),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(price, 1);
                Grid.SetColumn(price, 3);
                tripGrid.Children.Add(price);

                Button selectButton = new Button
                {
                    Content = "Chọn chuyến",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400")),
                    Foreground = Brushes.White,
                    Padding = new Thickness(5, 2, 5, 2)
                };
                selectButton.Click += (sender, e) => SelectTrip(trip, "Chuyến đi");

                Border buttonBorder = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(10),
                    Width = 150,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Child = selectButton
                };

                tripStack.Children.Add(tripGrid);
                tripStack.Children.Add(buttonBorder);

                tripBorder.Child = tripStack;
                resultPanel.Children.Add(tripBorder);
            }
        }
        private void SetButtonActive(Button activeButton, Button inactiveButton)
        {
            activeButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));
            activeButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));

            inactiveButton.Foreground = Brushes.Black;
            inactiveButton.BorderBrush = null;
        }

        private void DisplayRoundTrips(List<Trip> departureTrips, List<Trip> returnTrips)
        {
            DateTime? departureDate = DepartureDatePicker.SelectedDate;
            DateTime? returnDate = ReturnDatePicker.SelectedDate;
            TextBlock noResultTextBlock = FindName("NoResultTextBlock") as TextBlock;

            if (departureTrips.Count == 0 && returnTrips.Count == 0)
            {
                noResultTextBlock.Visibility = Visibility.Visible;
                return;
            }

            string departureDateString = departureDate.Value.ToString("dd/MM");
            string returnDateString = returnDate.Value.ToString("dd/MM");
            string departureButtonContent = $"Chuyến đi - {(departureDateString)}";
            string returnButtonContent = $"Chuyến về - {(returnDateString)}";
            string departureProvinceCode = departureTrips[0].DepartureProvinceCode.ToString();
            string destinationProvinceCode = departureTrips[0].DestinationProvinceCode.ToString();

            _provinceDictionary.TryGetValue(departureProvinceCode, out string departureProvinceName);
            _provinceDictionary.TryGetValue(destinationProvinceCode, out string destinationProvinceName);
            StackPanel resultPanel = FindName("ResultPanel") as StackPanel;
            resultPanel.Children.Clear();

            //ChangeInfotrip(departureTrips, resultPanel);

            TextBlock tripInfoTextBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };

            resultPanel.Children.Add(tripInfoTextBlock);
            tripInfoTextBlock.Text = $"{(departureProvinceName ?? departureProvinceCode)} - {(destinationProvinceName ?? destinationProvinceCode)} ({departureTrips.Count} chuyến)";

            Grid gridContainer = new Grid();
            gridContainer.HorizontalAlignment = HorizontalAlignment.Stretch; // Đặt chiều rộng của grid là 100%

            // Thêm gridContainer vào ResultPanel
            resultPanel.Children.Add(gridContainer);

            gridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button departureButton = new Button
            {
                Content = departureButtonContent,
                Margin = new Thickness(5, 5, 0, 10),
                FontWeight = FontWeights.Bold, // Set default bold font weight
                BorderThickness = new Thickness(0, 0, 0, 2), // Set default bottom border
                Padding = new Thickness(10),
                Background = Brushes.White,
                Foreground = Brushes.Black // Default text color
            };
            Grid.SetColumn(departureButton, 0);

            Button returnButton = new Button
            {
                Content = returnButtonContent,
                Margin = new Thickness(0, 5, 5, 10),
                FontWeight = FontWeights.Bold, // Set default bold font weight
                BorderThickness = new Thickness(0, 0, 0, 2), // Set default bottom border
                Padding = new Thickness(10),
                Background = Brushes.White,
                Foreground = Brushes.Black // Default text color
            };
            Grid.SetColumn(returnButton, 1);

            // Event handlers for button clicks
            departureButton.Click += (sender, e) =>
            {
                ShowTrips(departureTrips, "Chuyến đi");
                tripInfoTextBlock.Text = $"{(departureProvinceName ?? departureProvinceCode)} - {(destinationProvinceName ?? destinationProvinceCode)} ({departureTrips.Count} chuyến)";
                // Reset all buttons to default style
                SetButtonActive(departureButton, returnButton);
                departureButton.Foreground = Brushes.White;
                departureButton.BorderBrush = null;
                returnButton.Foreground = Brushes.Black;
                returnButton.BorderBrush = null;
                // Apply active style to departureButton
                departureButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));
                departureButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));
            };

            returnButton.Click += (sender, e) =>
            {
                ShowTrips(returnTrips, "Chuyến về");
                tripInfoTextBlock.Text = $"{(departureProvinceName ?? departureProvinceCode)} - {(destinationProvinceName ?? destinationProvinceCode)} ({returnTrips.Count} chuyến)";
                // Reset all buttons to default style
                SetButtonActive(returnButton, departureButton);
                departureButton.Foreground = Brushes.Black;
                departureButton.BorderBrush = null;
                returnButton.Foreground = Brushes.White;
                returnButton.BorderBrush = null;
                // Apply active style to returnButton
                returnButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));
                returnButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400"));
            };

            // Add buttons to navPanel
            gridContainer.Children.Add(departureButton);
            gridContainer.Children.Add(returnButton);
            SetButtonActive(departureButton, returnButton);
            //gridContainer.Children.Add(navPanel);

            // Display departure trips by default
            ShowTrips(departureTrips, "Chuyến đi");
        }

        private void ShowTrips(List<Trip> trips, string tripType)
        {
            StackPanel resultPanel = FindName("ResultPanel") as StackPanel;

            // Clear existing trip details except for the first two controls (trip info and nav buttons)
            while (resultPanel.Children.Count > 2)
            {
                resultPanel.Children.RemoveAt(2);
            }

            foreach (var trip in trips)
            {
                Border tripBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10),
                    Margin = new Thickness(10, 0, 10, 10)
                };

                StackPanel tripStack = new StackPanel();

                Grid tripGrid = new Grid();
                tripGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tripGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                tripGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Add other trip details similar to DisplayTrips method
                // ...

                // Thời gian khởi hành
                TextBlock departureTime = new TextBlock
                {
                    Text = trip.DepartureTime.ToString(),
                    Margin = new Thickness(0, 10, 130, 16),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(departureTime, 0);
                Grid.SetColumn(departureTime, 0);
                Grid.SetRowSpan(departureTime, 2);
                tripGrid.Children.Add(departureTime);

                // Thời gian di chuyển
                string travelTime = "---------->";
                TextBlock travelTimeBlock = new TextBlock
                {
                    Text = travelTime,
                    Margin = new Thickness(161, 10, 22, 0),
                    Width = 60,
                    VerticalAlignment = VerticalAlignment.Center
                };
                //Grid.SetRow(travelTimeBlock, 0);
                //Grid.SetColumn(travelTimeBlock, 1);
                tripGrid.Children.Add(travelTimeBlock);

                // Thời gian đến
                TextBlock arrivalTime = new TextBlock
                {
                    Text = trip.ExpectedArrivalTime.ToString(),
                    Margin = new Thickness(0, 10, 130, 16),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(arrivalTime, 0);
                Grid.SetColumn(arrivalTime, 2);
                Grid.SetRowSpan(arrivalTime, 2);
                tripGrid.Children.Add(arrivalTime);

                // Loại xe
                string tripTypeCode = trip.TripType.ToString();
                string tripTypeText = tripTypeCode switch
                {
                    "seated_bus" => "Ghế",
                    "sleeper_bus" => "Giường",
                    "limousine" => "Limousine",
                    _ => tripTypeCode
                };
                TextBlock tripTypeTextBlock = new TextBlock
                {
                    Text = tripTypeText,
                    Margin = new Thickness(139, 10, 196, 1),
                    Width = 100,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(tripTypeTextBlock, 0);
                Grid.SetColumn(tripTypeTextBlock, 2);
                Grid.SetColumnSpan(tripTypeTextBlock, 2);
                tripGrid.Children.Add(tripTypeTextBlock);

                // Số giường
                int totalEmptySeats = CountEmptySeats(trip);
                string totalEmptySeatsText = tripTypeCode switch
                {
                    "seated_bus" => $"Ghế trống: {totalEmptySeats}",
                    "sleeper_bus" => $"Giường trống: {totalEmptySeats}",
                    "limousine" => $"Giường trống: {totalEmptySeats}",
                    _ => tripTypeCode
                };
                TextBlock seatCount = new TextBlock
                {
                    Text = totalEmptySeatsText,
                    Margin = new Thickness(242, 10, -1, 16),
                    Width = 150,
                    TextAlignment = TextAlignment.Right
                };
                Grid.SetRowSpan(seatCount, 2);
                Grid.SetColumn(seatCount, 2);
                Grid.SetColumnSpan(seatCount, 2);
                tripGrid.Children.Add(seatCount);

                // Nơi đi
                string departureProvinceCode = trip.DepartureProvinceCode;
                _provinceDictionary.TryGetValue(departureProvinceCode, out string departureProvinceName);
                TextBlock departureProvince = new TextBlock
                {
                    Text = departureProvinceName ?? departureProvinceCode,
                    Margin = new Thickness(0, 10, 93, 0),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(departureProvince, 1);
                Grid.SetColumn(departureProvince, 0);
                tripGrid.Children.Add(departureProvince);

                // Nơi đến
                string destinationProvinceCode = trip.DestinationProvinceCode;
                _provinceDictionary.TryGetValue(destinationProvinceCode, out string destinationProvinceName);
                TextBlock destinationProvince = new TextBlock
                {
                    Text = destinationProvinceName ?? destinationProvinceCode,
                    Margin = new Thickness(0, 10, 92, 0),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(destinationProvince, 1);
                Grid.SetColumn(destinationProvince, 2);
                tripGrid.Children.Add(destinationProvince);

                // Giá vé
                TextBlock price = new TextBlock
                {
                    Text = trip.Price.ToString() + " VND",
                    Margin = new Thickness(93, 10, 0, 0),
                    Width = 150,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(price, 1);
                Grid.SetColumn(price, 3);
                tripGrid.Children.Add(price);

                Button selectButton = new Button
                {
                    Content = "Chọn chuyến",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFA5400")),
                    Foreground = Brushes.White,
                    Padding = new Thickness(5, 2, 5, 2)
                };

                Debug.WriteLine($"{trip.Id}, {tripType}");
                selectButton.Click += (sender, e) => SelectTrip(trip, tripType);

                Border buttonBorder = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(10),
                    Width = 150,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Child = selectButton
                };

                tripStack.Children.Add(tripGrid);
                tripStack.Children.Add(buttonBorder);

                tripBorder.Child = tripStack;
                resultPanel.Children.Add(tripBorder);
            }
        }
    }
}
