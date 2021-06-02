using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class EntropyService : IEntropyService
    {
        private readonly IWindowService _windowService;
        private byte[] _entropy;

        public EntropyService([NotNull]IWindowService windowService)
        {
            _windowService = windowService;
        }

        public async Task<byte[]> GetEntropy()
        {
            if (HasEntropy())
                return _entropy;

            _entropy = await _windowService.PromptForEntropy();
            return _entropy;
        }

        public bool HasEntropy()
        {
            return _entropy != null;
        }
    }
}
