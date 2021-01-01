using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipments;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Constants;

namespace Nop.Plugin.Api.Services
{
    public interface IShipmentApiService
    {
        IList<Shipment> GetShipmentsByOrderId(int orderId);

        IList<Order> GetShipments(IList<int> ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
                               int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, 
                               int sinceId = Configurations.DefaultSinceId, OrderStatus? status = null, PaymentStatus? paymentStatus = null, 
                               ShippingStatus? shippingStatus = null, int? orderId = null, int? storeId = null);

        Order GetShipmentById(int shipmentId);

        int GetShipmentCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null, ShipmentStatus? status = null,
                           PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null,
                           int? orderId = null, int? storeId = null);
    }
}