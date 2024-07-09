using System.Windows;

namespace FutaBuss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void closeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimizeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new FutaBuss.View.Booking("b86d8901d5c045f9b19a0a4939d46a25", "b86d8901d5c045f9b19a0a4939d46a25"));
            MainFrame.Navigate(new FutaBuss.View.PaymentMethod(Guid.Parse("4ce0ad4b-4cdc-46c0-8fb4-5dcd92cd3e06")));
            //MainFrame.Navigate(new FutaBuss.View.SearchTrips());
        }
    }
}