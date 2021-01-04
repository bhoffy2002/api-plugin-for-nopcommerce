using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTOs.Base;
using Nop.Plugin.Api.DTOs.Shipments;
using Nop.Plugin.Api.Validators;

namespace Nop.Plugin.Api.DTOs.ShipmentItem
{
    [JsonObject(Title = "shipment_item")]
    public class ShipmentItemDto : BaseDto
    {
        [JsonProperty("shipment")]
        [DoNotMap]
        public ShipmentDto  Shipment { get; set; }

        [JsonProperty("order_item`")]
        [DoNotMap]
        public OrderItem OrderItem { get; set; }

        [JsonProperty("shipment_id")]
        public int ShipmentId { get; set; }

        [JsonProperty("order_item_id")]
        public int OrderItemId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("warehouse_id")]
        public int WarehouseId { get; set; }
    }
}
