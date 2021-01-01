using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Api.Services
{
    public interface IShipmentItemApiService
    {
        IList<ShipmentItem> GetShipmentItemsForShipment(Shipment shipment, int limit, int page, int sinceId);
        int GetShipmentItemsCount(Shipment shipment);
    }
}