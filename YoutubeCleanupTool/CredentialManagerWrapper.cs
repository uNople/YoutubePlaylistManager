using AdysTech.CredentialManager;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class CredentialManagerWrapper : ICredentialManagerWrapper
    {
        private const string _googleApiKeyCredentialName = "YouTubeCleanupTool_googleapikey";
        public string GetApiKey()
        {
            return CredentialManager.GetICredential(_googleApiKeyCredentialName).UserName;
        }

        public void PromptForKey()
        {
            // NOTE: if the credential manager errors for any reason, the entire program will crash. That's cause
            // .net treats this as an entirely unhandleable error, and won't catch it. The thing that could have
            // caught it has been removed in .net core.
            // For more reading, see:
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.exceptionservices.handleprocesscorruptedstateexceptionsattribute
            Console.WriteLine("Username can be anything you want - it's not used. The password is the API key");
            Console.WriteLine("Go to https://console.developers.google.com/?pli=1 to get your API key");
            if (Exists())
            {
                CredentialManager.RemoveCredentials(_googleApiKeyCredentialName);
            }
            CredentialManager.PromptForCredentialsConsole(_googleApiKeyCredentialName);
        }

        public bool Exists()
        {
            try
            {
                return GetApiKey() != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
