using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public class DpapiService : IDpapiService
    {
        private readonly IEntropyService _entropyService;
        private readonly Encoding _defaultEncoding = Encoding.Unicode;

        public DpapiService(IEntropyService entropyService)
        {
            _entropyService = entropyService;
        }

        public async Task<string> Decrypt(string data)
        {
            try
            {
                // TODO: probably change this to AES/DES instead
                Debug.Assert(OperatingSystem.IsWindows());
                var decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(data), await _entropyService.GetEntropy(), DataProtectionScope.CurrentUser);
                return _defaultEncoding.GetString(decryptedData);
            }
            catch
            {
                return "";
            }
        }

        public async Task<string> Encrypt(string data)
        {
            // TODO: probably change this to AES/DES instead
            Debug.Assert(OperatingSystem.IsWindows());
            var encryptedData = ProtectedData.Protect(_defaultEncoding.GetBytes(data), await _entropyService.GetEntropy(), DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public async Task<string> DecryptFromDisk(string path)
        {
            if (!File.Exists(path))
                return null;

            var encrypted = await File.ReadAllTextAsync(path);
            return await Decrypt(encrypted);
        }

        public async Task EncryptAndSaveToDisk(string originalPath, string savePath)
        {
            if (!File.Exists(originalPath))
                return;

            var originalFile = await File.ReadAllTextAsync(originalPath);
            var encrypted = await Encrypt(originalFile);
            await File.WriteAllTextAsync(savePath, encrypted);
        }
    }
}