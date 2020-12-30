using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Shipments
{
    public class ShipmentsRootObject : ISerializableObject
    {
        public ShipmentsRootObject()
        {
            Shipments = new List<ShipmentDto>();
        }

        [JsonProperty("shipments")]
        public IList<ShipmentDto> Shipments { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "shipments";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ShipmentDto);
        }
    }
}
