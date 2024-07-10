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
            //MainFrame.Navigate(new FutaBuss.View.PaymentMethod(Guid.Parse("38a6253a-05d7-4341-8ddc-f8ec44af3fa6"), Guid.Parse("38a6253a-05d7-4341-8ddc-f8ec44af3fa6")));

            //FutaBuss.Model.Customer customer = new FutaBuss.Model.Customer(

            //    );

            //customer.FullName = "Trần Anh Thi";
            //customer.PhoneNumber = "0123456789";
            //customer.Email = "tran@gmail.com";
            
            //MainFrame.Navigate(new FutaBuss.View.PaymentSuccess(customer, 200000, "zalopay", Guid.Parse("420e13f7-e60a-4594-965d-5a9fa6ab6883")));

            MainFrame.Navigate(new FutaBuss.View.SearchTrips());

        }
    }
}