using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace YoutubeCleanupWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MainWindow([NotNull]MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            DataContext = _mainWindowViewModel;
            StartOnNonPrimaryScreen();
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => await _mainWindowViewModel.LoadData());
        }

        private void StartOnNonPrimaryScreen()
        {
            // TODO: Save the screen we want to start on
            var screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                var secondaryScreen = screens.FirstOrDefault(x => !x.Primary);
                Top = secondaryScreen.Bounds.Top + 100;
                Left = secondaryScreen.Bounds.Left + 100;
            }
        }
    }
}
