using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YouTubeCleanupWpf
{
    public class MessageBoxErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
