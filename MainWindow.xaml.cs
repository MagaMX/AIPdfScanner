using Microsoft.Win32;
using System.Windows;
using static Org.BouncyCastle.Math.Primes;

namespace AIPdfScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PdfScanner _pdfScanner = new PdfScanner();
        private readonly OpenAIService _aiService = new OpenAIService();
        private readonly TranslateService _translateService = new TranslateService();

        public MainWindow()
        {
            InitializeComponent();
            LoadWindow();
        }

        private void LoadWindow()
        {
            TxtOut.Visibility = Visibility.Hidden;
            TranslatedTxtOut.Visibility = Visibility.Hidden;
            BtnAskAI.Visibility = Visibility.Hidden;
            BtnTranslateText.Visibility = Visibility.Hidden;
        }

        private async void BtnLoadPdfFile(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "PDF Files|*.pdf"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string extractedText = await _pdfScanner.ExtractTextFromPdf(openFileDialog.FileName);
                    if (!string.IsNullOrWhiteSpace(extractedText))
                    {
                        TxtOut.Text = extractedText;
                        BtnLoadFile.Content = "Окрыть новый файл";
                        this.WindowState = WindowState.Maximized;
                        TxtOut.Visibility = Visibility.Visible;
                        TxtOut.UpdateLayout();
                        BtnAskAI.Visibility = Visibility.Visible;
                        BtnTranslateText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show(
                           $"Файл пуст!",
                           "Ошибка",
                           MessageBoxButton.OK,
                           MessageBoxImage.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Произошла непредвиденная ошибка: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }

        private async void BtnAskAI_Click(object sender, RoutedEventArgs e)
        {
            ChatBorder.Visibility = (ChatBorder.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            string response = await _aiService.GetChatGPTResponse(TxtOut.Text);
            ChatBox.Text = "ChatGPT: " + response + "\n";
            ChatBox.UpdateLayout();
        }

        private async void BtnTranslateText_Click(object sender, RoutedEventArgs e)
        {
            TranslatedTxtOut.Visibility = Visibility.Visible;

            try
            {
                string translatedText = await _translateService.TranslateTextAsync(TxtOut.Text);
                TranslatedTxtOut.Text = translatedText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перевода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputBox.Text;
            if (string.IsNullOrWhiteSpace(userInput)) return;

            ChatBox.Text += "Вы: " + userInput + "\n";

            string response = await _aiService.GetChatGPTResponse(userInput);
            ChatBox.Text += "ChatGPT: " + response + "\n";

            InputBox.Clear();
        }
    }
}