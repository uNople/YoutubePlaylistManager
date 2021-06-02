using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeCleanupWpf.Windows
{
    public interface IPasswordPrompt
    {
        byte[] GetEntropy();
        bool? ShowDialog();
    }
}
