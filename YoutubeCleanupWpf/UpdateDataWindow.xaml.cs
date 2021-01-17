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
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YouTubeCleanupWpf
{
    /// <summary>
    /// Interaction logic for UpdateDataWindow.xaml
    /// </summary>
    public partial class UpdateDataWindow : Window
    {
        private readonly UpdateDataViewModel _updateDataViewModel;
        private readonly WpfSettings _wpfSettings;

        public UpdateDataWindow([NotNull] UpdateDataViewModel updateDataViewModel, [NotNull] WpfSettings wpfSettings)
        {
            _updateDataViewModel = updateDataViewModel;
            _updateDataViewModel.ParentWindow = this;
            DataContext = _updateDataViewModel;
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
