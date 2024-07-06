using System.Windows;

namespace FutaBuss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(List<string> seatIds)
        {
            InitializeComponent();
            SeatIdsTextBlock.Text = "Selected Seat IDs: " + string.Join(", ", seatIds);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.Visibility = Visibility.Visible;
            this.Close();
        }
    }
}