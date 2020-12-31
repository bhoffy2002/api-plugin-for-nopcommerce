using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace Nop.Plugin.Api.DTOs.ShipmentItems
{

    public class ShipmentItemsRootObject
    {
        public ShipmentItemsRootObject()
        {
            ShipmentItems = new List<ShipmentItemDto>();
        }

        [JsonProperty("shipment_items")]
        public IList<ShipmentItemDto> ShipmentItems { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "shipment_items";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ShipmentItemDto);
        }
    }
}
