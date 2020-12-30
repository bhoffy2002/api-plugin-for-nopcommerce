using System;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Api.Models.ShipmentsParameters
{
    // JsonProperty is used only for swagger
    public class BaseShipmentsParametersModel
    {
        public BaseShipmentsParametersModel()
        {
            CreatedAtMax = null;
            CreatedAtMin = null;
        }


        /// <summary>
        /// Show orders created after date (format: 2008-12-31 03:00)
        /// </summary>
        [JsonProperty("created_at_min")]
        public DateTime? CreatedAtMin { get; set; }

        /// <summary>
        /// Show orders created before date(format: 2008-12-31 03:00)
        /// </summary>
        [JsonProperty("created_at_max")]
        public DateTime? CreatedAtMax { get; set; }


        /// <summary>
        /// Show all the shipments for this order
        /// </summary>
        [JsonProperty("order_id")]
        public int? OrderId { get; set; }
    }
}
