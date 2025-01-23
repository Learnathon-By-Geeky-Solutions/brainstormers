// In synchronous programming, tasks are executed sequentially, one after the other.
// This means that each task must finish before the next one begins. If a task is time-consuming
// (like downloading a file or reading a large database), it blocks the execution of the program until it's completed.

// In asynchronous programming, tasks can be executed concurrently without blocking the execution of other tasks.
// This means that the program can initiate a task and then move on to other tasks, while the original task
// continues to run in the background. Once the asynchronous task is complete, the program can handle the result.
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Asynchronous_Programming_With_Async__Await
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Button Click event for the first button with pop-up message
        private async void Button_Click_With_Message(object sender, RoutedEventArgs e)
        {
            try
            {
                await DownloadHtmlAsync("http://msdn.microsoft.com");
                // Show success message after the download is complete
                MessageBox.Show("HTML downloaded successfully and saved to file.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Show error message in case of any exception
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Button Click event for the second button without pop-up message
        private async void Button_Click_Without_Message(object sender, RoutedEventArgs e)
        {
            try
            {
                await DownloadHtmlAsync("http://msdn.microsoft.com");
            }
            catch (Exception ex)
            {
                // Handle exception (optional)
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Download HTML asynchronously and save it to a file
        public async Task DownloadHtmlAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                // Asynchronously download the HTML
                var html = await httpClient.GetStringAsync(url);

                // Specify the file path where you want to save the downloaded HTML content
                var filePath = @"C:\Users\t440s\Downloads\result.html";

                using (var streamWriter = new StreamWriter(filePath))
                {
                    await streamWriter.WriteLineAsync(html);
                }
            }
        }
    }
}
