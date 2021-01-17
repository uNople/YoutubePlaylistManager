using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace YouTubeCleanupWpf
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public WpfSettings WpfSettings { get; set; }
        public ICommand SelectDbPathCommand { get; set; }
        public ICommand SelectClientSecretPathCommand { get; set; }
        private static void ShowError(Exception ex) => MessageBox.Show(ex.ToString());
        

        public SettingsWindowViewModel([NotNull] WpfSettings wpfSettings)
        {
            WpfSettings = wpfSettings;
            SelectDbPathCommand = new RunMethodCommand<string>(SelectDbPath, ShowError);
            SelectClientSecretPathCommand = new RunMethodCommand<string>(SelectClientSecret, ShowError);
        }

        private Task SelectClientSecret(string arg)
        {
            throw new NotImplementedException();
        }

        private Task SelectDbPath(string currentPath)
        {
            throw new NotImplementedException();
        }
    }
}
