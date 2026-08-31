using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Windows.Media.Control;

namespace Coverr
{
    public partial class MainWindow : Window
    {
        private GlobalSystemMediaTransportControlsSessionManager? _smtcManager;
        private GlobalSystemMediaTransportControlsSession? _currentSession;
        private string? _lastTrackKey;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSmtc();
        }

        private async void InitializeSmtc()
        {
            try
            {
                _smtcManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                _smtcManager.CurrentSessionChanged += OnCurrentSessionChanged;

                // Hook into the initial session, if any
                AttachSession(_smtcManager.GetCurrentSession());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Coverr] SMTC init failed: {ex}");
            }
        }

        private void OnCurrentSessionChanged(
            GlobalSystemMediaTransportControlsSessionManager sender,
            CurrentSessionChangedEventArgs args)
        {
            AttachSession(sender.GetCurrentSession());
        }

        private void AttachSession(GlobalSystemMediaTransportControlsSession? session)
        {
            // Unsubscribe from the old session
            if (_currentSession != null)
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;

            _currentSession = session;
            _lastTrackKey = null; // force reload for the new session

            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                // Fetch cover art immediately for the new session
                UpdateCoverArt();
            }
            else
            {
                // No active session — clear the display
                Dispatcher.Invoke(() =>
                {
                    CoverArtImage.Source = null;
                    BackgroundImage.Source = null;
                });
            }
        }

        private void OnMediaPropertiesChanged(
            GlobalSystemMediaTransportControlsSession sender,
            MediaPropertiesChangedEventArgs args)
        {
            // Called only when the playing track actually changes
            UpdateCoverArt();
        }

        private async void UpdateCoverArt()
        {
            try
            {
                if (_currentSession == null) return;

                var mediaProperties = await _currentSession.TryGetMediaPropertiesAsync();
                if (mediaProperties?.Thumbnail == null) return;

                // Skip the expensive decode if the same track is still playing
                var trackKey = $"{mediaProperties.Artist}|{mediaProperties.Title}";
                if (trackKey == _lastTrackKey) return;
                _lastTrackKey = trackKey;

                using var winrtStream = await mediaProperties.Thumbnail.OpenReadAsync();
                using var dotNetStream = winrtStream.AsStream();

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = dotNetStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                // Marshal back to the UI thread
                Dispatcher.Invoke(() =>
                {
                    CoverArtImage.Source = bitmap;
                    BackgroundImage.Source = bitmap;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Coverr] UpdateCoverArt failed: {ex}");
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && this.WindowState == WindowState.Normal)
                this.DragMove();
        }
    }
}
