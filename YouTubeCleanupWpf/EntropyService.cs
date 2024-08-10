using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf;

public class EntropyService([NotNull] IWindowService windowService) : IEntropyService
{
    private byte[] _entropy;

    public async Task<byte[]> GetEntropy()
    {
        if (HasEntropy())
            return _entropy;

        _entropy = await windowService.PromptForEntropy();
        return _entropy;
    }

    public bool HasEntropy()
    {
        return _entropy != null;
    }
}