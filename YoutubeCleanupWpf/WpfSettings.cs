﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.ViewModels;
using Screen = System.Windows.Forms.Screen;

namespace YouTubeCleanupWpf
{
    public class WpfSettings : INotifyPropertyChanged
    {
        private const int SaveDelayMs = 500;
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
            Themes = new ObservableCollection<string>(new[] {"DarkMode", "Pink", "NoTheme"});
            YouTubeServiceCreatorOptions.DatabasePath = DatabasePath;
            YouTubeServiceCreatorOptions.ClientSecretPath = ClientSecretPath;
            _saveSettingsDeferTimer = new DeferTimer(SaveSetting, MainWindowViewModel.ShowError);
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
                _saveSettingsDeferTimer?.DeferByMilliseconds(SaveDelayMs);
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
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(uri));
            }
            catch (Exception ex)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                MainWindowViewModel.ShowError(ex);
            }
        }

        private void SetCurrentScreen(string selectedMonitor)
        {
            CurrentScreen = ScreenCollection?.FirstOrDefault(x => x.DeviceName == selectedMonitor) ?? ScreenCollection?.FirstOrDefault();
        }
    }
}