using Newtonsoft.Json;

namespace MigrationSample.Model
{
    public class WorkspaceCollectionKeys
    {
        [JsonProperty(PropertyName = "key1")]
        public string Key1 { get; set; }

        [JsonProperty(PropertyName = "key2")]
        public string Key2 { get; set; }
    }
}
