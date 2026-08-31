using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media.Control;

namespace Coverr
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            
            // Check for track changes every 2 seconds
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += (s, e) => UpdateCoverArt();
            _timer.Start();
            
            UpdateCoverArt();
        }

        private async void UpdateCoverArt()
        {
            try 
            {
                var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = manager.GetCurrentSession();

                if (session != null)
                {
                    var mediaProperties = await session.TryGetMediaPropertiesAsync();
                    if (mediaProperties.Thumbnail != null)
                    {
                        using var winrtStream = await mediaProperties.Thumbnail.OpenReadAsync();
                        using var dotNetStream = winrtStream.AsStream();

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = dotNetStream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        CoverArtImage.Source = bitmap;
                        BackgroundImage.Source = bitmap;
                    }
                }
            }
            catch { /* Ignore errors if no media is playing */ }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && this.WindowState == WindowState.Normal)
                this.DragMove();
        }
    }
}
