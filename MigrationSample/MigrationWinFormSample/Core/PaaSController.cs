using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Api.V1.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MigrationSample.Models;
using System.IO;
using MigrationSample.Model;

namespace MigrationSample.Core
{
    public class PaaSController
    {
        private static readonly string version = "?api-version=2016-01-29";
        private static readonly string clientId = "ea0616ba-638b-4df5-95b9-636659ae5121";
        private static readonly Uri redirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

        private static Dictionary<string, WorkspaceCollectionKeys> accessKeysPerWSC = new Dictionary<string, WorkspaceCollectionKeys>();

        public PBIProvisioningContext Context { get; set; }

        private string TenantsUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["tenantsUrl"];
            }
        }

        private string SubscriptionsUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["subscriptionsUrl"];
            }
        }

        private string ArmResource
        {
            get
            {
                return ConfigurationManager.AppSettings["armResource"];
            }
        }

        private string ApiEndpointUri {
            get
            {
                return ConfigurationManager.AppSettings[string.Format("{0}-apiEndpointUri", Context.Environment)];
            }
        }

        public PaaSController(PBIProvisioningContext context)
        {
            this.Context = context;
        }

        public void SetAzureToken(AuthenticationResult authenticationResult)
        {
            AzureTokenManager.SetAzureToken(authenticationResult, Context.Environment);
        }

        public async Task<string> GetResourceGroups()
        {
            if (Context.Subscription != null)
            {
                return await GetFromUrl(string.Format("{0}/subscriptions/{1}/resourcegroups{2}", GetAzureEndpointUrl(), Context.Subscription.Id, "?api-version=2016-09-01"));
            }
            return null;
        }

        public string GetAzureToken()
        {
            return AzureTokenManager.GetAzureToken(Context.Environment);
        }

        public async Task<HttpOperationResponse<Stream>> ExportToFile(string workspaceId, string reportId, string path)
        {
            using (var client = await CreateAppKeyClient())
            {
                var response = await client.Reports.ExportReportWithHttpMessagesAsync(Context.WorkspaceCollection.Name, workspaceId, reportId);

                if (response.Response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = await response.Response.Content.ReadAsStreamAsync();

                    using (FileStream fileStream = File.Create(path))
                    {
                        stream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                }

                return response;
            }
        }

        public async Task<IList<PBIWorkspaceCollection>> GetRelevantWorkspaceCollections()
        {
            IList<PBIWorkspaceCollection> wscs = (await GetWorkspaceCollections()).Value;

            string dxtLocation = ConfigurationManager.AppSettings["dxt-location-friendly"];
            string msitLocation = ConfigurationManager.AppSettings["msit-location-friendly"];

            if (Context.Environment.Equals(MigrationEnvironment.prod))
            {
                return wscs.Where(wsc => (!wsc.Location.Equals(dxtLocation)) && (!wsc.Location.Equals(msitLocation))).ToList();
            }
            else if (Context.Environment.Equals(MigrationEnvironment.msit))
            {
                return wscs.Where(wsc => wsc.Location.Equals(msitLocation)).ToList();
            }
            else if (Context.Environment.Equals(MigrationEnvironment.dxt))
            {
                return wscs.Where(wsc => wsc.Location.Equals(dxtLocation)).ToList();
            }
            else
            {
                return wscs;
            }
        }

        public async Task<PBIWorkspaceCollections> GetWorkspaceCollections()
        {
            var responseContent = await GetWorkspaceCollectionsJson();
            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<PBIWorkspaceCollections>(responseContent);
        }

        private async Task<string> GetWorkspaceCollectionsJson()
        {
            var url = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.PowerBI/workspaceCollections{3}",
                GetAzureEndpointUrl(), Context.Subscription.Id, Context.ResourceGroupName, version);
            HttpClient client = new HttpClient();

            using (client)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Set authorization header from you acquired Azure AD token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAzureAccessTokenAsync());
                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new Exception(message);
                }

                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
        }

        public async Task<string> GetSubscriptions()
        {
            return await GetFromUrl(SubscriptionsUrl);
        }

        /// <summary>
        /// Gets a list of Power BI Workspace Collections workspaces within the specified collection
        /// </summary>
        /// <param name="workspaceCollectionName">The Power BI workspace collection name</param>
        /// <returns></returns>
        public async Task<IList<Workspace>> GetWorkspaces()
        {
            using (var client = await CreateAppKeyClient())
            {
                var response = await client.Workspaces.GetWorkspacesByCollectionNameAsync(Context.WorkspaceCollection.Name);
                return response.Value;
            }
        }

        public async Task<ODataResponseListDatasource> GetDatasources(string datasetId)
        {
            using (var client = await CreateAppKeyClient())
            {
                try
                {
                    return client.Datasets.GetDatasources(Context.WorkspaceCollection.Name, Context.WorkspaceId, datasetId);
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<ODataResponseListImport> GetImports()
        {
            using (var client = await CreateAppKeyClient())
            {
                return await client.Imports.GetImportsAsync(Context.WorkspaceCollection.Name, Context.WorkspaceId);
            }
        }

        public async Task<IList<Dataset>> GetDatasets()
        {
            using (var client = await CreateAppKeyClient())
            {
                var response = await client.Datasets.GetDatasetsAsync(Context.WorkspaceCollection.Name, Context.WorkspaceId);
                return response.Value;
            }
        }

        public async Task<IList<Report>> GetReports()
        {
            using (var client = await CreateAppKeyClient())
            {
                var response = await client.Reports.GetReportsAsync(Context.WorkspaceCollection.Name, Context.WorkspaceId);
                return response.Value;
            }
        }

        public async Task<WorkspaceCollectionKeys> GetAccessKeys()
        {
            if (!accessKeysPerWSC.ContainsKey(Context.WorkspaceCollection.Name))
            {
                accessKeysPerWSC[Context.WorkspaceCollection.Name] = await ListWorkspaceCollectionKeys();
            }

            return accessKeysPerWSC[Context.WorkspaceCollection.Name];
        }

        private async Task<PowerBIClient> CreateAppKeyClient()
        {
            // Create a token credentials with "AppKey" type
            var credentials = new TokenCredentials((await GetAccessKeys()).Key1, "AppKey");

            // Instantiate your Power BI client passing in the required credentials
            var client = new PowerBIClient(credentials);

            // Override the api endpoint base URL.  Default value is https://api.powerbi.com
            client.BaseUri = new Uri(ApiEndpointUri);

            return client;
        }

        private async Task<string> GetFromUrl(string url)
        {
            HttpClient client = new HttpClient();

            using (client)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Set authorization header from you acquired Azure AD token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAzureAccessTokenAsync());
                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new Exception(message);
                }

                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
        }

        private async Task<string> GetAzureAccessTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(GetAzureToken()))
            {
                return GetAzureToken();
            }

            var commonToken = GetCommonAzureAccessToken();
            var tenantId = (await GetTenantIdsAsync(commonToken.AccessToken)).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new InvalidOperationException("Unable to get tenant id for user account");
            }

            var authority = string.Format("{0}/{1}/oauth2/authorize", GetWindowsLoginUrl(), tenantId);
            var authContext = new AuthenticationContext(authority);
            var authenticationResult = await authContext.AcquireTokenByRefreshTokenAsync(commonToken.RefreshToken, clientId, ArmResource);

            SetAzureToken(authenticationResult);

            return authenticationResult.AccessToken;
        }

        private async Task<IEnumerable<string>> GetTenantIdsAsync(string commonToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + commonToken);
                
                var response = await httpClient.GetStringAsync(TenantsUrl);

                var tenantsJson = JsonConvert.DeserializeObject<JObject>(response);
                var tenants = tenantsJson["value"] as JArray;

                return tenants.Select(t => t["tenantId"].Value<string>());
            }
        }

        private AuthenticationResult GetCommonAzureAccessToken()
        {
            string urlPattern = "{0}/common/oauth2/authorize";

            var authContext = new AuthenticationContext(string.Format(urlPattern, GetWindowsLoginUrl()));

            AuthenticationResult result = authContext.AcquireToken(
                resource: ArmResource,
                clientId: clientId,
                redirectUri: redirectUri,
                promptBehavior: PaaSAuthPromptBehavior());

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result;
        }

        private string GetAzureEndpointUrl()
        {
            return ConfigurationManager.AppSettings[string.Format("{0}-azureApiEndpoint", Context.Environment)];
        }

        private string GetWindowsLoginUrl()
        {
            return ConfigurationManager.AppSettings["loginWindowsUrl"];
        }

        private PromptBehavior PaaSAuthPromptBehavior()
        {
            if (ConfigurationManager.AppSettings["PaaSAuthPromptBehavior"] == "Always")
            {
                return PromptBehavior.Always;
            }
            return PromptBehavior.Auto;
        }

        /// <summary>
        /// Gets the workspace collection access keys for the specified collection
        /// </summary>
        /// <param name="subscriptionId">The azure subscription id</param>
        /// <param name="resourceGroup">The azure resource group</param>
        /// <param name="workspaceCollectionName">The Power BI workspace collection name</param>
        /// <returns></returns>
        private async Task<WorkspaceCollectionKeys> ListWorkspaceCollectionKeys()
        {
            var url = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.PowerBI/workspaceCollections/{3}/listkeys{4}", 
                GetAzureEndpointUrl(), 
                Context.Subscription.Id, 
                Context.ResourceGroupName, 
                Context.WorkspaceCollection.Name, 
                version);

            HttpClient client = new HttpClient();

            using (client)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                // Set authorization header from you acquired Azure AD token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAzureAccessTokenAsync());

                request.Content = new StringContent(string.Empty);
                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var message = string.Format("Status: {0}, Reason: {1}, Message: {2}", response.StatusCode, response.ReasonPhrase, responseText);
                    throw new Exception(message);
                }

                var json = await response.Content.ReadAsStringAsync();
                return SafeJsonConvert.DeserializeObject<WorkspaceCollectionKeys>(json);
            }
        }
    }
}
