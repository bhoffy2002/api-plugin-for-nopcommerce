using System;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Api.Factories
{
    public class ShipmentFactory : IFactory<Shipment>
    {
        public Shipment Initialize()
        {
            var shipment = new Shipment();

            shipment.CreateOnUtc = DateTime.UtcNow;

            return shipment;
        }
    }
}
