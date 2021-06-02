using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for PasswordPrompt.xaml
    /// </summary>
    public partial class PasswordPrompt : Window, IPasswordPrompt
    {
        private SecureString _password;
        public PasswordPrompt()
        {
            InitializeComponent();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _password = ((PasswordBox) sender).SecurePassword;
        }

        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Return)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        public byte[] GetEntropy()
        {
            var unicodeBytesPtr = Marshal.SecureStringToGlobalAllocUnicode(_password);
            var length = _password.Length * 2;
            var bytes = new byte[length];
            try
            {
                var unicodeBytes = new byte[length];

                for (var i = 0; i < unicodeBytes.Length; ++i)
                {
                    bytes[i] = Marshal.ReadByte(unicodeBytesPtr, i);
                }

                return bytes;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unicodeBytesPtr);
            }
        }
    }
}
