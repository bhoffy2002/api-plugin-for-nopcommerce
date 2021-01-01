using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipments;
using Nop.Plugin.Api.DataStructures;

namespace Nop.Plugin.Api.Services
{
    public class ShipmentItemApiService : IShipmentItemApiService
    {
        public IList<ShipmentItem> GetShipmentItemsForShipment(Shipment shipment, int limit, int page, int sinceId)
        {
            var shipmentItems = shipment.ShipmentItems.AsQueryable();

            return new ApiList<ShipmentItem>(shipmentItems, page - 1, limit);
        }
        
        public int GetShipmentItemsCount(Shipment shipment)
        {
            var shipmentItemsCount = shipment.ShipmentItems.Count();

            return shipmentItemsCount;
        }
    }
}