using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using YouTubeCleanup.Ui;
using YouTubeCleanupTool.Domain;
using Screen = System.Windows.Forms.Screen;

namespace YouTubeCleanupWpf
{
    public class WpfSettings : INotifyPropertyChanged, IDebugSettings
    {
        private const int SaveDelayMs = 500;

        [JsonIgnore]
        public YouTubeServiceCreatorOptions YouTubeServiceCreatorOptions { get; set; }

        [JsonIgnore]
        public IErrorHandler ErrorHandler { get; set; }
#pragma warning disable 067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067

        [JsonIgnore]
        public ObservableCollection<string> Monitors { get; set; }

        [JsonIgnore]
        public ObservableCollection<string> Themes { get; set; }

        private List<Screen> ScreenCollection { get; set; }

        [JsonIgnore]
        public Screen CurrentScreen { get; set; }

        private DeferTimer _saveSettingsDeferTimer;

        private bool _showIds;

        public event IDebugSettings.ChangedEventHandler ShowIdsChanged;

        public bool ShowIds
        {
            get => _showIds;
            set
            {
                _showIds = value;
                ShowIdsChanged?.Invoke(value);
                _saveSettingsDeferTimer?.DeferByMilliseconds(SaveDelayMs);
            }
        }

        public WpfSettings(YouTubeServiceCreatorOptions youTubeServiceCreatorOptions, IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            YouTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
        }

        public void InitializeSettings()
        {
            ScreenCollection = Screen.AllScreens.ToList();
            Monitors = new ObservableCollection<string>(ScreenCollection.Select(x => x.DeviceName));
            SetCurrentScreen(SelectedMonitor);
            Themes = new ObservableCollection<string>(new[] {"DarkMode", "Pink", "NoTheme"});
            YouTubeServiceCreatorOptions.DatabasePath = DatabasePath;
            YouTubeServiceCreatorOptions.ClientSecretPath = ClientSecretPath;
            _saveSettingsDeferTimer = new DeferTimer(SaveSetting, ErrorHandler.HandleError);
        }

        private async Task SaveSetting()
        {
            await Task.Run(() => File.WriteAllText("WpfSettings.json", JsonConvert.SerializeObject(this, Formatting.Indented)));
        }

        private string _databasePath = "Application.db";

        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                _databasePath = value;
                if (YouTubeServiceCreatorOptions != null)
                    YouTubeServiceCreatorOptions.DatabasePath = value;
                _saveSettingsDeferTimer?.DeferByMilliseconds(SaveDelayMs);
            }
        }

        private string _clientSecretPath = @"C:\temp\client_secret.json";

        public string ClientSecretPath
        {
            get => _clientSecretPath;
            set
            {
                _clientSecretPath = value;
                if (YouTubeServiceCreatorOptions != null)
                    YouTubeServiceCreatorOptions.ClientSecretPath = value;
                _saveSettingsDeferTimer?.DeferByMilliseconds(SaveDelayMs);
            }
        }

        private string _selectedMonitor;

        public string SelectedMonitor
        {
            get => _selectedMonitor;
            set
            {
                _selectedMonitor = value;
                SetCurrentScreen(value);
                _saveSettingsDeferTimer?.DeferByMilliseconds(SaveDelayMs);
            }
        }

        private string _selectedTheme;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                _saveSettingsDeferTimer?.DeferByMilliseconds(SaveDelayMs);
                LoadTheme(value);
            }
        }

        private void LoadTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
                return;

            var uri = new Uri($"Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute);
            try
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary) Application.LoadComponent(uri));
            }
            catch (Exception ex)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                ErrorHandler.HandleError(ex);
            }
        }

        private void SetCurrentScreen(string selectedMonitor)
        {
            CurrentScreen = ScreenCollection?.FirstOrDefault(x => x.DeviceName == selectedMonitor) ?? ScreenCollection?.FirstOrDefault();
        }
    }
}