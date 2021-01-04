using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.DTOs.Shipments;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ShipmentDtoMappings
    {
        public static ShipmentDto ToDto(this Shipment shipment)
        {
            return shipment.MapTo<Shipment, ShipmentDto>();
        }
    }
}
