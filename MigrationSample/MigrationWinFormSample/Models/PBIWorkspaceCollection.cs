using Newtonsoft.Json;
using System.Collections.Generic;

namespace MigrationSample.Models
{
    public class PBIWorkspaceCollection
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
    }

    public class PBIWorkspaceCollections
    {
        public IList<PBIWorkspaceCollection> Value { get; set; }
    }
}
