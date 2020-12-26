namespace YouTubeApiWrapper.Interfaces
{
    public interface ICredentialManagerWrapper
    {
        string GetApiKey();
        void PromptForKey();
        bool Exists();
    }
}