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
