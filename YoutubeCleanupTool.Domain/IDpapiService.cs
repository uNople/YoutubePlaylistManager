using System;
using System.IO;
using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public interface IDpapiService
    {
        Task<string> Decrypt(string data);
        Task<string> Encrypt(string data);
        Task<string> DecryptFromDisk(string path);
        Task EncryptAndSaveToDisk(string originalPath, string savePath);
    }
}