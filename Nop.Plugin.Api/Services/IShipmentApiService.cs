using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Constants;

namespace Nop.Plugin.Api.Services
{
    public interface IShipmentApiService
    {
        IList<Shipment> GetShipmentsByOrderId(int orderId);

        IList<Shipment> GetShipments(IList<int> ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
                               int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, 
                               int sinceId = Configurations.DefaultSinceId, int? orderId = null, int? storeId = null);

        Shipment GetShipmentById(int shipmentId);

        int GetShipmentsCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null, int? orderId = null, int? storeId = null);
    }
}