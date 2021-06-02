using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public interface IEntropyService
    {
        Task<byte[]> GetEntropy();
        bool HasEntropy();
    }
}