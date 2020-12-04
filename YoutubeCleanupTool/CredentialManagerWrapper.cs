using AdysTech.CredentialManager;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class CredentialManagerWrapper : ICredentialManagerWrapper
    {
        private const string _googleApiKeyCredentialName = "googleapikey";
        public string GetApiKey()
        {
            return CredentialManager.GetICredential(_googleApiKeyCredentialName).UserName;
        }
    }
}
