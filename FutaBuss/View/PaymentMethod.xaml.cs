using System.Windows;
using System.Windows.Controls;
using ZXing.Common;
using ZXing;
using System.Drawing;
using System.Windows.Media.Imaging;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using System.IO;
using System.Windows.Threading;

namespace FutaBuss.View
{
    /// <summary>
    /// Interaction logic for PaymentMethod.xaml
    /// </summary>
    public partial class PaymentMethod : Page
    {
        private DispatcherTimer timer;
        private int countdownSeconds = 100; // Thời gian đếm ngược, đơn vị là giây

        public PaymentMethod()
        {
            InitializeComponent();
            futaPayRadioButton.IsChecked = true;
            StartCountdown();
        }


        private void StartCountdown()
        {
            // Tạo một DispatcherTimer với tick interval là 1 giây
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // Bắt đầu đếm ngược
            timer.Start();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            // Giảm số giây còn lại đi 1 và cập nhật lên giao diện
            countdownSeconds--;

            // Tính toán phút và giây từ số giây còn lại
            int minutes = countdownSeconds / 60;
            int seconds = countdownSeconds % 60;

            // Format lại chuỗi để hiển thị dưới dạng "mm:ss"
            string countdownText = $"{minutes:D2}:{seconds:D2}";

            // Hiển thị chuỗi đã định dạng lên TextBlock
            countDownTime.Text = countdownText;

            // Kiểm tra nếu đếm ngược kết thúc
            if (countdownSeconds == 0)
            {
                timer.Stop();
                MessageBox.Show("Countdown finished!");
            }
        }


        public void GenerateQRCode(string data)
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

            string filePath = Path.Combine("..\\..\\..", "Images", "qrcode.png");

            Bitmap qrCodeBitmap = writer.Write(data);
            qrCodeBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        }

        // Method to update the QR code image
        private void UpdateQRCodeImage(string data)
        {
            GenerateQRCode(data);

            string imagePath = Path.Combine("..\\..\\..", "Images", "qrcode.png");

            // Kiểm tra xem tệp có tồn tại không trước khi thiết lập Source
            if (File.Exists(imagePath))
            {
                // Tạo một đối tượng BitmapImage để tải hình ảnh từ tệp
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmapImage.EndInit();

                // Thiết lập Source của control Image
                qrCodeImage.Source = bitmapImage;
            }
            else
            {
                throw new Exception($"File '{imagePath}' not found.");
            }
        }


        private void UpdateQRCodeLogo(string fileName)
        {

            if(fileName == "shopee.png" || fileName == "vietQR.png")
            {
                string imagePath = Path.Combine("..\\..\\..", "Images", fileName);
                imgQRLogo.Visibility = Visibility.Visible;
                svgQRLogo.Visibility= Visibility.Collapsed;

                // Kiểm tra xem tệp có tồn tại không trước khi thiết lập Source
                if (File.Exists(imagePath))
                {
                    // Tạo một đối tượng BitmapImage để tải hình ảnh từ tệp
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    // Thiết lập Source của control Image
                    imgQRLogo.Source = bitmapImage;
                }
                else
                {
                    throw new Exception($"File '{imagePath}' not found.");
                }
            }
            else {
                imgQRLogo.Visibility = Visibility.Collapsed;
                svgQRLogo.Visibility = Visibility.Visible;
                svgQRLogo.Source = new Uri($"pack://application:,,,/Images/{fileName}");
            }

        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton checkedRadioButton = sender as RadioButton;


            string data = "Your QR Code Data"; // Replace with your actual data
            UpdateQRCodeImage(data);
            UpdateQRCodeLogo(checkedRadioButton.Tag.ToString());

        }
    }
}


