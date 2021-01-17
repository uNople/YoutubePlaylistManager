using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfSettings : INotifyPropertyChanged
    {
        private IConfigurationRoot _configuration;
        private YouTubeServiceCreatorOptions _youTubeServiceCreatorOptions;

        public WpfSettings(IConfigurationRoot configuration, YouTubeServiceCreatorOptions youTubeServiceCreatorOptions)
        {
            _configuration = configuration;
            _youTubeServiceCreatorOptions = youTubeServiceCreatorOptions;

            _youTubeServiceCreatorOptions.DatabasePath = _configuration["WpfSettings:DatabasePath"];
            _youTubeServiceCreatorOptions.ClientSecretPath = _configuration["WpfSettings:ClientSecretPath"];

        }
        public event PropertyChangedEventHandler PropertyChanged;

        public string DatabasePath
        {
            get => _configuration["WpfSettings:DatabasePath"];
            set
            {
                _configuration["WpfSettings:DatabasePath"] = value;
                _youTubeServiceCreatorOptions.DatabasePath = value;
            }
        }

        public string ClientSecretPath
        {
            get => _configuration["WpfSettings:ClientSecretPath"];
            set
            {
                _configuration["WpfSettings:ClientSecretPath"] = value;
                _youTubeServiceCreatorOptions.ClientSecretPath = value;
            }
        }

        public string SelectedMonitor
        {
            get => _configuration["WpfSettings:SelectedMonitor"];
            set => _configuration["WpfSettings:SelectedMonitor"] = value;
        }

        public string SelectedTheme
        {
            get => _configuration["WpfSettings:SelectedMonitor"];
            set => _configuration["WpfSettings:SelectedMonitor"] = value;
        }
    }
}
