using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;

namespace Nop.Plugin.Api.Services
{
    public class ShipmentApiService : IShipmentApiService
    {
        private readonly IRepository<Shipment> _shipmentRepository;

        public ShipmentApiService(IRepository<Shipment> shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public IList<Shipment> GetShipmentsByOrderId(int orderId)
        {
            var query = from shipment in _shipmentRepository.Table
                        where shipment.OrderId == orderId 
                        orderby shipment.Id
                        select shipment;

            return new ApiList<Shipment>(query, 0, Configurations.MaxLimit);
        }

        public IList<Shipment> GetShipments(IList<int> ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
           int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId, 
           int? orderId = null, int? storeId = null)
        {
            var query = GetShipmentsQuery(createdAtMin, createdAtMax, ids, orderId, storeId);

            if (sinceId > 0)
            {
                query = query.Where(shipment => shipment.Id > sinceId);
            }

            return new ApiList<Shipment>(query, page - 1, limit);
        }

        public Shipment GetShipmentById(int shipmentId)
        {
            if (shipmentId <= 0)
                return null;

            return _shipmentRepository.Table.FirstOrDefault(shipment => shipment.Id == shipmentId);
        }

        public int GetShipmentsCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null, int? orderId = null, int? storeId = null)
        {
            var query = GetShipmentsQuery(createdAtMin, createdAtMax, orderId: orderId, storeId: storeId);

            return query.Count();
        }

        private IQueryable<Shipment> GetShipmentsQuery(DateTime? createdAtMin = null, DateTime? createdAtMax = null, IList<int> ids = null, 
            int? orderId = null, int? storeId = null)
        {
            var query = _shipmentRepository.Table;
            
            if (orderId != null)
            {
                query = query.Where(shipment => shipment.OrderId == orderId);
            }

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(c => ids.Contains(c.Id));
            }
            
            //if (status != null)
            //{
            //    query = query.Where(shipment => shipment.OrderStatusId == (int)status);
            //}
            
            //if (paymentStatus != null)
            //{
            //    query = query.Where(order => order.PaymentStatusId == (int)paymentStatus);
            //}
            
            //if (shippingStatus != null)
            //{
            //    query = query.Where(order => order.ShippingStatusId == (int)shippingStatus);
            //}

            //query = query.Where(shipment => !shipment.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(shipment => shipment.CreatedOnUtc > createdAtMin.Value.ToUniversalTime());
            }

            if (createdAtMax != null)
            {
                query = query.Where(shipment => shipment.CreatedOnUtc < createdAtMax.Value.ToUniversalTime());
            }

            //if (storeId != null)
            //{
            //    query = query.Where(shipment => shipment.StoreId == storeId);
            //}

            query = query.OrderBy(shipment => shipment.Id);

            //query = query.Include(c => c.Customer);
            //query = query.Include(c => c.BillingAddress);
            //query = query.Include(c => c.ShippingAddress);
            //query = query.Include(c => c.PickupAddress);
            //query = query.Include(c => c.RedeemedRewardPointsEntry);
            //query = query.Include(c => c.DiscountUsageHistory);
            //query = query.Include(c => c.GiftCardUsageHistory);
            //query = query.Include(c => c.OrderNotes);
            //query = query.Include(c => c.OrderItems);
            //query = query.Include(c => c.Shipments);

            return query;
        }
    }
}