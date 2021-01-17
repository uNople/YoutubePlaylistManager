using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfSettings : INotifyPropertyChanged
    {
        private IConfigurationRoot _configuration;
        [JsonIgnore]
        public YouTubeServiceCreatorOptions YouTubeServiceCreatorOptions { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        
        [JsonIgnore]
        public ObservableCollection<string> Monitors { get; set; }
        
        [JsonIgnore]
        public ObservableCollection<string> Themes { get; set; }
        private List<Screen> ScreenCollection { get; set; }
        
        [JsonIgnore]
        public Screen CurrentScreen { get; set; }
        private static void ShowError(Exception ex) => MessageBox.Show(ex.ToString());
        private DeferTimer _saveSettingsDeferTimer;

        public WpfSettings(YouTubeServiceCreatorOptions youTubeServiceCreatorOptions)
        {
            YouTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
        }

        public void InitializeSettings()
        {
            ScreenCollection = Screen.AllScreens.ToList();
            Monitors = new ObservableCollection<string>(ScreenCollection.Select(x => x.DeviceName));
            SetCurrentScreen(SelectedMonitor);
            Themes = new ObservableCollection<string>(new[] {"Dark", "Light"});
            YouTubeServiceCreatorOptions.DatabasePath = DatabasePath;
            YouTubeServiceCreatorOptions.ClientSecretPath = ClientSecretPath;
            _saveSettingsDeferTimer = new DeferTimer(SaveSetting, ShowError);
        }

        private async Task SaveSetting()
        {
            await Task.Run(() => File.WriteAllText("WpfSettings.json", JsonConvert.SerializeObject(this, Formatting.Indented)));
        }

        private string _databasePath;
        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                _databasePath = value;
                if (YouTubeServiceCreatorOptions != null)
                    YouTubeServiceCreatorOptions.DatabasePath = value;
                _saveSettingsDeferTimer?.DeferByMilliseconds(5000);
            }
        }

        private string _clientSecretPath;
        public string ClientSecretPath
        {
            get => _clientSecretPath;
            set
            {
                _clientSecretPath = value;
                if (YouTubeServiceCreatorOptions != null)
                    YouTubeServiceCreatorOptions.ClientSecretPath = value;
                _saveSettingsDeferTimer?.DeferByMilliseconds(5000);
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
                _saveSettingsDeferTimer?.DeferByMilliseconds(5000);
            }
        }

        private string _selectedTheme;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                _saveSettingsDeferTimer?.DeferByMilliseconds(5000);
                // TODO: Update the theme across all windows - event?
            }
        }

        private void SetCurrentScreen(string selectedMonitor)
        {
            CurrentScreen = ScreenCollection?.FirstOrDefault(x => x.DeviceName == selectedMonitor) ?? ScreenCollection?.FirstOrDefault();
        }
    }
}
