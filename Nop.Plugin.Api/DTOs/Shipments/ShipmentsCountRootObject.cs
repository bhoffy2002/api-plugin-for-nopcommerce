using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Shipments
{
    public class ShipmentsCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
