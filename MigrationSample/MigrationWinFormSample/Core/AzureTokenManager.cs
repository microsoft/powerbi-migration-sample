using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MigrationWinFormSample.Dialogs;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace MigrationSample.Core
{
    static class AzureTokenManager
    { 
        private static readonly string clientId = "ea0616ba-638b-4df5-95b9-636659ae5121";
        private static readonly Uri redirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

        static readonly string resourceUri = ConfigurationManager.AppSettings["AADPowerBIResourceId"];
        static readonly string authorityUri = $"{ConfigurationManager.AppSettings["powerbi-service-loginWindowsUrl"]}/common/oauth2/authorize";

        static readonly string graphResourceUri = ConfigurationManager.AppSettings["graph-apiEndpointUri"];
        static readonly string graphAuthorityUri = $"{ConfigurationManager.AppSettings["powerbi-service-loginWindowsUrl"]}/common/";

        private static DateTime ProdAzureTokenExpiration;
        private static string ProdAzureToken = null;

        private static AuthenticationResult PBISaaSAuthResult { get; set; }

        private static AuthenticationResult GraphAuthResult { get; set; }

        public static string GetPowerBISaaSToken()
        {
            if (PBISaaSAuthResult == null || PBISaaSAuthResult.ExpiresOn < DateTime.UtcNow)
            {
                var dialog = new HelpDialog(
                    "Master account required",
                    "In order to create groups and upload content, you need to sign in with your master account.",
                    "https://powerbi.microsoft.com/en-us/documentation/powerbi-developer-migrate-from-powerbi-embedded/#accounts-within-azure-ad"
                    );

                var dialogResult = dialog.ShowDialog();

                AuthenticationContext authContext = new AuthenticationContext(authorityUri);
                PBISaaSAuthResult = authContext.AcquireToken(resourceUri, clientId, redirectUri, PromptBehavior.Always);
            }

            return PBISaaSAuthResult.AccessToken;
        }

        private static void RefreshGraphAuthResult()
        {
            if (GraphAuthResult == null || GraphAuthResult.ExpiresOn < DateTime.UtcNow)
            {
                AuthenticationContext authContext = new AuthenticationContext(graphAuthorityUri);

                GraphAuthResult = authContext.AcquireToken(
                    graphResourceUri, clientId,
                    redirectUri,
                    PromptBehavior.Auto,
                    new UserIdentifier(
                        PBISaaSAuthResult.UserInfo.DisplayableId,
                        UserIdentifierType.RequiredDisplayableId));
            }
        }

        public static string GetGraphUserUniqueId()
        {
            RefreshGraphAuthResult();

            return PBISaaSAuthResult.UserInfo.UniqueId;
        }

        public static string GetGraphToken()
        {
            RefreshGraphAuthResult();

            return GraphAuthResult.AccessToken;
        }

        public static void LoadAppData()
        {
            ProdAzureToken = MigrationWinFormSample.Properties.Settings.Default.prodAzureToken;
            ProdAzureTokenExpiration = MigrationWinFormSample.Properties.Settings.Default.prodAzureTokenExpiration;
        }

        public static void SetAppData()
        {
            MigrationWinFormSample.Properties.Settings.Default.prodAzureToken = ProdAzureToken;
            MigrationWinFormSample.Properties.Settings.Default.prodAzureTokenExpiration = ProdAzureTokenExpiration;
        }

        public static string GetAzureToken(MigrationEnvironment env)
        {
            if (ProdAzureTokenExpiration != null && ProdAzureTokenExpiration > DateTime.UtcNow && ConfigurationManager.AppSettings["ignorePaaSAzureTokenCache"] == null)
            {
                return ProdAzureToken;
            }

            return null;
        }

        public static void SetAzureToken(AuthenticationResult authenticationResult, MigrationEnvironment env)
        {
            ProdAzureTokenExpiration = authenticationResult.ExpiresOn.DateTime;
            ProdAzureToken = authenticationResult.AccessToken;
        }
    }
}
