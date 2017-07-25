using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;

namespace MigrationSample.Core
{
    static class SaaSController
    {
        private static string GraphUrlWithVersion
        {
            get
            {
                return $"{ConfigurationManager.AppSettings["graph-apiEndpointUri"]}/{ConfigurationManager.AppSettings["graph-apiEndpointUri-version"]}";
            }
        }

        public static ODataResponseListGroup GetGroups()
        {
            using (var client = CreatePowerBIClient())
            {
                return client.Groups.GetGroups();
            }
        }

        public static async Task<ODataResponseListImport> GetImports(string groupId)
        {
            using (var client = CreatePowerBIClient())
            {
                return await client.Imports.GetImportsInGroupAsync(groupId);
            }
        }

        private static PowerBIClient CreatePowerBIClient()
        {
            var credentials = new TokenCredentials(AzureTokenManager.GetPowerBISaaSToken());
            return new PowerBIClient(new Uri($"{ConfigurationManager.AppSettings["powerbi-service-apiEndpointUri"]}"), credentials);
        }

        public static async Task<ODataResponseListReport> GetReports(string groupId)
        {
            using (var client = CreatePowerBIClient())
            {
                return await client.Reports.GetReportsInGroupAsync(groupId);
            }
        }

        public static async Task<string> SendImport(string pbixPath, string groupId, string targetName, string nameConflict)
        {
            using (var client = CreatePowerBIClient())
            {
                using (var file = File.Open(pbixPath, FileMode.Open))
                {
                    return (await client.Imports.PostImportWithFileAsyncInGroup(groupId, file, targetName, nameConflict)).Id;
                }
            }
        }

        public static async Task<Group> CreateGroupAsync(string groupName)
        {
            using (var client = CreatePowerBIClient())
            {
                return await client.Groups.CreateGroupAsync(new GroupCreationRequest(groupName));
            }
        }
    }
}
