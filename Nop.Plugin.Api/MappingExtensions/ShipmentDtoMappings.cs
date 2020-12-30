using Nop.Core.Domain.Shipments;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.Orders;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ShipmentDtoMappings
    {
        public static ShipmentDto ToDto(this Shipment shipment)
        {
            return shipment.Map<Shipment, ShipmentDto>();
        }
    }
}
