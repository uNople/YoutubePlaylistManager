using System;
using System.ComponentModel;
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
        private readonly IAppClosingCancellationToken _appClosingCancellationToken;
        private readonly WindowExtensions _windowExtensions;

        public MainWindow([NotNull]MainWindowViewModel mainWindowViewModel,
            [NotNull] WpfSettings wpfSettings,
            [NotNull] IAppClosingCancellationToken appClosingCancellationToken,
            [NotNull] WindowExtensions windowExtensions)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _wpfSettings = wpfSettings;
            _appClosingCancellationToken = appClosingCancellationToken;
            _windowExtensions = windowExtensions;
            DataContext = _mainWindowViewModel;
            Task.Run(async () => await _windowExtensions.StartOnSelectedWindow(this, _wpfSettings));
            InitializeComponent();
        }
        
        public new async Task Show()
        {
            await _windowExtensions.StartOnSelectedWindow(this, _wpfSettings);
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

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _appClosingCancellationToken.CancellationTokenSource.Cancel();
        }
    }
}
