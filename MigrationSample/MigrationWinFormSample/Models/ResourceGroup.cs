using Newtonsoft.Json;

namespace MigrationSample
{
    public class ResourceGroup
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class AzureResourceGroups
    {
        public ResourceGroup[] Value { get; set; }
    }
}
