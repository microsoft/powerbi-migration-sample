using Newtonsoft.Json;
using System;

namespace MigrationSample
{
    public class Subscription
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "subscriptionId")]
        public Guid Id { get; set; }
    }

    public class AzureSubscriptions
    {
        public Subscription[] Value { get; set; }
    }
}
