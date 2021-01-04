using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.ShipmentItems
{
    public class ShipmentItemsCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}