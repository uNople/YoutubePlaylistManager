using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using YouTubeCleanupWpf;

namespace YoutubeCleanupWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly WpfSettings _wpfSettings;

        public MainWindow([NotNull]MainWindowViewModel mainWindowViewModel, [NotNull] WpfSettings wpfSettings)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _wpfSettings = wpfSettings;
            DataContext = _mainWindowViewModel;
            this.StartOnSelectedWindow(wpfSettings);
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => await _mainWindowViewModel.LoadData());
        }
    }
}
