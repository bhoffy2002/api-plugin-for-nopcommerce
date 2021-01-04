using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Base;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.DTOs.ShipmentItem;
using Nop.Plugin.Api.DTOs.ShipmentItems;
using Nop.Plugin.Api.Validators;

namespace Nop.Plugin.Api.DTOs.Shipments
{
    [JsonObject(Title = "shipment")]
    public class ShipmentDto : BaseDto
    {
        private ICollection<ShipmentItemDto> _shipmentItems;

        [JsonProperty("oder_id")]
        public int OrderId { get; set; }

        [JsonProperty("tracking_number")]
        public string TrackingNumber { get; set; }

        [JsonProperty("total_weight")]
        public decimal? TotalWeight { get; set; }
        
        [JsonProperty("shipped_date_utc")]
        public DateTime? ShippedDateUtc { get; set; }
        
        [JsonProperty("delivery_date_utc")]
        public DateTime? DeliveryDateUtc { get; set; }

        [JsonProperty("admin_comment")]
        public string AdminComment { get; set; }

        [JsonProperty("created_on_utc")]
        public DateTime? CreatedOnUtc { get; set; }


        public ICollection<ShipmentItemDto> ShipmentItems
        {
            get
            {
                if (_shipmentItems == null)
                {
                    _shipmentItems = new List<ShipmentItemDto>();
                }

                return _shipmentItems;
            }
            set { _shipmentItems = value; }
        }

        [JsonProperty("order")]
        public OrderDto Order { get; set; }
    }
}
