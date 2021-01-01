using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipments;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs.Errors;
using Nop.Plugin.Api.DTOs.ShipmentItems;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.OrderItemsParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;

namespace Nop.Plugin.Api.Controllers
{
    [ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme,
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ShipmentItemsController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly IOrderApiService _orderApiService;
        private readonly IOrderItemApiService _orderItemApiService;
        private readonly IOrderService _orderService;
        private readonly IShipmentApiService _shipmentApiService;
        private readonly IShipmentItemApiService _shipmentItemApiService;
        private readonly IShipmentService _shipmentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductApiService _productApiService;
        private readonly ITaxService _taxService;

        public ShipmentItemsController(IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IOrderItemApiService orderItemApiService,
            IOrderApiService orderApiService,
            IOrderService orderService,
            IShipmentItemApiService shipmentItemApiService,
            IShipmentApiService shipmentApiService,
            IShipmentService shipmentService,
            IProductApiService productApiService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IPictureService pictureService, IDTOHelper dtoHelper)
            : base(jsonFieldsSerializer,
                aclService,
                customerService,
                storeMappingService,
                storeService,
                discountService,
                customerActivityService,
                localizationService,
                pictureService)
        {
            _orderItemApiService = orderItemApiService;
            _orderApiService = orderApiService;
            _orderService = orderService;
            _shipmentItemApiService = shipmentItemApiService;
            _shipmentApiService = shipmentApiService;
            _shipmentService = shipmentService;
            _productApiService = productApiService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _dtoHelper = dtoHelper;
        }

        [HttpGet]
        [Route("/api/shipments/{shipmentId}/items")]
        [ProducesResponseType(typeof(ShipmentItemsRootObject), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipmentItems(int shipmentId, ShipmentItemsParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "Invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid request parameters");
            }

            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var allShipmentItemsForShipment =
                _shipmentItemApiService.GetShipmentItemsForShipment(shipment, parameters.Limit, parameters.Page,
                    parameters.SinceId);

            var shipmentItemsRootObject = new ShipmentItemsRootObject
            {
                ShipmentItems = allShipmentItemsForShipment.Select(item => _dtoHelper.PrepareShipmentItemDTO(item)).ToList()
            };

            var json = JsonFieldsSerializer.Serialize(shipmentItemsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/shipments/{shipmentId}/items/count")]
        [ProducesResponseType(typeof(ShipmentItemsCountRootObject), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipmentItemsCount(int shipmentId)
        {
            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var shipmentItemsCountForShipment = _shipmentItemApiService.GetShipmentItemsCount(shipment);

            var shipmentItemsCountRootObject = new ShipmentItemsCountRootObject
            {
                Count = shipmentItemsCountForShipment
            };

            return Ok(shipmentItemsCountRootObject);
        }

        [HttpGet]
        [Route("/api/shipments/{shipmentId}/items/{shipmentItemId}")]
        [ProducesResponseType(typeof(ShipmentItemsRootObject), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipmentItemByIdForShipment(int shipmentId, int shipmentItemId, string fields = "")
        {
            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var shipmentItem = _shipmentService.GetShipmentItemById(shipmentItemId);

            if (shipmentItem == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment_item", "not found");
            }

            var shipmentItemDtos = new List<ShipmentItemDto> {_dtoHelper.PrepareShipmentItemDTO(shipmentItem)};

            var shipmentItemsRootObject = new ShipmentItemsRootObject
            {
                ShipmentItems = shipmentItemDtos
            };

            var json = JsonFieldsSerializer.Serialize(shipmentItemsRootObject, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/shipments/{shipmentId}/items")]
        [ProducesResponseType(typeof(ShipmentItemsRootObject), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        public IActionResult CreateShipmentItem(int shipmentId,
            [ModelBinder(typeof(JsonModelBinder<ShipmentItemDto>))]
            Delta<ShipmentItemDto> shipmentItemDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var product = GetProduct(shipmentItemDelta.Dto.ProductId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            //if (product.IsRental)
            //{
            //    if (shipmentItemDelta.Dto.RentalStartDateUtc == null)
            //    {
            //        return Error(HttpStatusCode.BadRequest, "rental_start_date_utc", "required");
            //    }

            //    if (shipmentItemDelta.Dto.RentalEndDateUtc == null)
            //    {
            //        return Error(HttpStatusCode.BadRequest, "rental_end_date_utc", "required");
            //    }

            //    if (shipmentItemDelta.Dto.RentalStartDateUtc > shipmentItemDelta.Dto.RentalEndDateUtc)
            //    {
            //        return Error(HttpStatusCode.BadRequest, "rental_start_date_utc",
            //            "should be before rental_end_date_utc");
            //    }

            //    if (shipmentItemDelta.Dto.RentalStartDateUtc < DateTime.UtcNow)
            //    {
            //        return Error(HttpStatusCode.BadRequest, "rental_start_date_utc", "should be a future date");
            //    }
            //}

            var newShipmentItem = PrepareDefaultShipmentItemFromProduct(shipment, product);
            shipmentItemDelta.Merge(newShipmentItem);

            shipment.ShipmentItems.Add(newShipmentItem);

            _shipmentService.UpdateShipment(shipment);

            OrderActivityService.InsertActivity("AddNewOrderItem",
                LocalizationService.GetResource("ActivityLog.AddNewOrderItem"), newShipmentItem);

            var shipmentItemsRootObject = new ShipmentItemsRootObject();

            shipmentItemsRootObject.ShipmentItems.Add(newShipmentItem.ToDto());

            var json = JsonFieldsSerializer.Serialize(shipmentItemsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/shipments/{shipmentId}/items/{shipmentItemId}")]
        [ProducesResponseType(typeof(ShipmentItemsRootObject), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        public IActionResult UpdateShipmentItem(int shipmentId, int shipmentItemId,
            [ModelBinder(typeof(JsonModelBinder<ShipmentItemDto>))]
            Delta<ShipmentItemDto> shipmentItemDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var ShipmentItemToUpdate = _shipmentService.GetShipmentItemById(shipmentItemId);

            if (ShipmentItemToUpdate == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment_item", "not found");
            }

            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            // This is needed because those fields shouldn't be updatable. That is why we save them and after the merge set them back.
            int? productId = ShipmentItemToUpdate.ProductId;
            //var rentalStartDate = ShipmentItemToUpdate.RentalStartDateUtc;
            //var rentalEndDate = ShipmentItemToUpdate.RentalEndDateUtc;

            shipmentItemDelta.Merge(ShipmentItemToUpdate);

            ShipmentItemToUpdate.ProductId = (int) productId;
            //ShipmentItemToUpdate.RentalStartDateUtc = rentalStartDate;
            //ShipmentItemToUpdate.RentalEndDateUtc = rentalEndDate;

            _shipmentService.UpdateShipment(shipment);

            OrderActivityService.InsertActivity("UpdateShipmentItem",
                LocalizationService.GetResource("ActivityLog.UpdateShipmentItem"), ShipmentItemToUpdate);

            var shipmentItemsRootObject = new ShipmentItemsRootObject();

            shipmentItemsRootObject.ShipmentItems.Add(ShipmentItemToUpdate.ToDto());

            var json = JsonFieldsSerializer.Serialize(shipmentItemsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/shipments/{shipmentId}/items/{shipmentItemId}")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteShipmentItemById(int shipmentId, int shipmentItemId)
        {
            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var shipmentItem = _shipmentService.GetShipmentItemById(shipmentItemId);
            _shipmentService.DeleteShipmentItem(shipmentItem);

            return new RawJsonActionResult("{}");
        }

        [HttpDelete]
        [Route("/api/shipments/{shipmentId}/items")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int) HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteAllShipmentItemsForShipment(int shipmentId)
        {
            var shipment = _shipmentApiService.GetShipmentById(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var shipmentItemsList = shipment.ShipmentItems.ToList();

            foreach (var t in shipmentItemsList)
            {
                _shipmentService.DeleteShipmentItem(t);
            }

            return new RawJsonActionResult("{}");
        }

        private Product GetProduct(int? productId)
        {
            Product product = null;

            if (productId.HasValue)
            {
                var id = productId.Value;

                product = _productApiService.GetProductById(id);
            }

            return product;
        }

        private ShipmentItem PrepareDefaultShipmentItemFromProduct(Shipment shipment, Product product)
        {
            var presetQty = 1;
            var presetPrice =
                _priceCalculationService.GetFinalPrice(product, shipment.Customer, decimal.Zero, true, presetQty);

            var presetPriceInclTax =
                _taxService.GetProductPrice(product, presetPrice, true, shipment.Customer, out _);
            var presetPriceExclTax =
                _taxService.GetProductPrice(product, presetPrice, false, shipment.Customer, out _);

            var shipmentItem = new OrderItem
            {
                OrderItemGuid = new Guid(),
                UnitPriceExclTax = presetPriceExclTax,
                UnitPriceInclTax = presetPriceInclTax,
                PriceInclTax = presetPriceInclTax,
                PriceExclTax = presetPriceExclTax,
                OriginalProductCost = _priceCalculationService.GetProductCost(product, null),
                Quantity = presetQty,
                Product = product,
                Order = shipment
            };

            return shipmentItem;
        }
    }
}