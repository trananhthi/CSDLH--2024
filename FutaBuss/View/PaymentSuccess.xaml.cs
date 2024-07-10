using SharpVectors.Dom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FutaBuss.View
{
    /// <summary>
    /// Interaction logic for PaymentSuccess.xaml
    /// </summary>
    public partial class PaymentSuccess : Page
    {
        public ObservableCollection<string> Items { get; set; }
        private int currentIndex = 0;
        private int itemsPerPage = 4;

        public PaymentSuccess()
        {
            InitializeComponent();
            DataContext = this;

            // Populate items for carousel
            Items = new ObservableCollection<string>
            {
                "Item 1", "Item 2", "Item 3", "Item 4", "Item 5",
                "Item 6", "Item 7", "Item 8", "Item 9", "Item 10"
            };

            UpdateCarouselView();
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


    }
}
