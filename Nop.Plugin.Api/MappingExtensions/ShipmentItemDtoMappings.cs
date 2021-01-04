using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.OrderItems;
using Nop.Plugin.Api.DTOs.ShipmentItem;

namespace Nop.Plugin.Api.MappingExtensions
{

    public static class ShipmentItemDtoMappings
    {
        public static ShipmentItemDto ToDto(this ShipmentItem shipmentItem)
        {
            return shipmentItem.MapTo<ShipmentItem, ShipmentItemDto>();
        }
    }
}
