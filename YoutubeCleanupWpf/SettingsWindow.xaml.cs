using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YouTubeCleanupWpf
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsWindowViewModel _settingsWindowViewModel;
        private readonly WpfSettings _wpfSettings;

        public SettingsWindow([NotNull] SettingsWindowViewModel settingsWindowViewModel, [NotNull] WpfSettings wpfSettings)
        {
            _settingsWindowViewModel = settingsWindowViewModel;
            DataContext = _settingsWindowViewModel;
            _wpfSettings = wpfSettings;
            this.StartOnSelectedWindow(_wpfSettings);
            InitializeComponent();
        }

        public new void Show()
        {
            this.StartOnSelectedWindow(_wpfSettings);
            base.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
