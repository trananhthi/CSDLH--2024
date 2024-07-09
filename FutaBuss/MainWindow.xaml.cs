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
            MainFrame.Navigate(new FutaBuss.View.PaymentMethod(Guid.Parse("36733a6b-c211-42ae-a177-93063cb5b4f5")));
            //MainFrame.Navigate(new FutaBuss.View.PaymentSuccess());
        }
    }
}