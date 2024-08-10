using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace YouTubeCleanupTool.Domain.UnitTests;

public class DpapiServiceTests
{
    [Theory, AutoNSubstituteData]
    public async Task When_encrypting_and_decrypting_to_disk_Then_data_is_correct(DpapiService dpapiService, string messageToEncrypt)
    {
            // set up the files on disk
            var originalFilePath = Path.GetTempFileName();
            var savePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(originalFilePath, messageToEncrypt);
            
            // Act
            await dpapiService.EncryptAndSaveToDisk(originalFilePath, savePath);

            // Assert - decrypt the encrypted files + verify it's the same
            var decrypted = await dpapiService.DecryptFromDisk(savePath);
            decrypted.Should().Be(messageToEncrypt);
            (await File.ReadAllTextAsync(originalFilePath)).Should().Be(messageToEncrypt);
            
            // Tidy up
            File.Delete(originalFilePath);
            File.Delete(savePath);
        }
}