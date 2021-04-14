using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace YouTubeCleanupTool.Domain.UnitTests
{
    public class DpapiServiceTests
    {
        [Theory, AutoNSubstituteData]
        public void When_encrypting_and_decrypting_to_disk_Then_data_is_correct(DpapiService dpapiService, string messageToEncrypt)
        {
            // set up the files on disk
            var originalFilePath = Path.GetTempFileName();
            var savePath = Path.GetTempFileName();
            File.WriteAllText(originalFilePath, messageToEncrypt);
            
            // Act
            dpapiService.EncryptAndSaveToDisk(originalFilePath, savePath);

            // Assert - decrypt the encrypted files + verify it's the same
            var decrypted = dpapiService.DecryptFromDisk(savePath);
            decrypted.Should().Be(messageToEncrypt);
            File.ReadAllText(originalFilePath).Should().Be(messageToEncrypt);
            
            // Tidy up
            File.Delete(originalFilePath);
            File.Delete(savePath);
        }
    }
}