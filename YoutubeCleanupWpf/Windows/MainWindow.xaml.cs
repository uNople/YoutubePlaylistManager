using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly WpfSettings _wpfSettings;

        public MainWindow([NotNull]MainWindowViewModel mainWindowViewModel, [NotNull] WpfSettings wpfSettings)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _wpfSettings = wpfSettings;
            DataContext = _mainWindowViewModel;
            this.StartOnSelectedWindow(_wpfSettings);
            InitializeComponent();
        }
        
        public new void Show()
        {
            this.StartOnSelectedWindow(_wpfSettings);
            base.Show();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _mainWindowViewModel.LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }
    }
}
