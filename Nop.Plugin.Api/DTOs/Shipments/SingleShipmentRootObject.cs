using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Shipments
{

    public class SingleShipmentRootObject
    {
        [JsonProperty("shipment")] 
        public ShipmentDto Shipment { get; set; }
    }
}
