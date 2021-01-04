using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs;
using Nop.Plugin.Api.DTOs.OrderItems;
using Nop.Plugin.Api.DTOs.Shipments;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.OrdersParameters;
using Nop.Plugin.Api.Services;
using Nop.Plugin.Api.Validators;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Shipping;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Models.ShipmentsParameters;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Controllers
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using DTOs.Errors;
    using JSON.Serializers;

    [ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ShipmentsController : BaseApiController
    {
        private readonly IShipmentApiService _shipmentApiService;
        private readonly IProductService _productService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShippingService _shippingService;
        private readonly IDTOHelper _dtoHelper;        
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IStoreContext _storeContext;
        private readonly IFactory<Shipment> _factory;

        // We resolve the order settings this way because of the tests.
        // The auto mocking does not support concreate types as dependencies. It supports only interfaces.
        private ShippingSettings _shipmentSettings;

        private ShippingSettings ShippingSettings => _shipmentSettings ?? (_shipmentSettings = EngineContext.Current.Resolve<ShippingSettings>());

        public ShipmentsController(IShipmentApiService shipmentApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IProductService productService,
            IFactory<Shipment> factory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IShipmentService shipmentService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IShippingService shippingService,
            IPictureService pictureService,
            IDTOHelper dtoHelper,
            IProductAttributeConverter productAttributeConverter)
            : base(jsonFieldsSerializer, aclService, customerService, storeMappingService,
                 storeService, discountService, customerActivityService, localizationService,pictureService)
        {
            _shipmentApiService = shipmentApiService;
            _factory = factory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _shippingService = shippingService;
            _dtoHelper = dtoHelper;
            _productService = productService;
            _productAttributeConverter = productAttributeConverter;
        }

        /// <summary>
        /// Receive a list of all shipments
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shipments")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipments(ShipmentsParametersModel parameters)
        {
            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid page parameter");
            }

            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid limit parameter");
            }

            var storeId = _storeContext.CurrentStore.Id;

            var shipments = _shipmentApiService.GetShipments(parameters.Ids, parameters.CreatedAtMin,
                parameters.CreatedAtMax,
                parameters.Limit, parameters.Page, parameters.SinceId,
                parameters.OrderId, storeId);

            IList<ShipmentDto> shipmentsAsDtos = shipments.Select(x => _dtoHelper.PrepareShipmentDTO(x)).ToList();

            var ShipmentsRootObject = new ShipmentsRootObject()
            {
                Shipments = shipmentsAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(ShipmentsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Receive a count of all shipments
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shipments/count")]
        [ProducesResponseType(typeof(ShipmentsCountRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipmentsCount(ShipmentsCountParametersModel parameters)
        {
            var storeId = _storeContext.CurrentStore.Id;

            var shipmentsCount = _shipmentApiService.GetShipmentsCount(parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.OrderId, storeId);

            var shipmentsCountRootObject = new ShipmentsCountRootObject()
            {
                Count = shipmentsCount
            };

            return Ok(shipmentsCountRootObject);
        }

        /// <summary>
        /// Retrieve shipment by spcified id
        /// </summary>
        ///   /// <param name="id">Id of the shipment</param>
        /// <param name="fields">Fields from the shipment you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shipments/{id}")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipmentById(int id, string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var shipment = _shipmentApiService.GetShipmentById(id);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var shipmentsRootObject = new ShipmentsRootObject();

            var shipmentDto = _dtoHelper.PrepareShipmentDTO(shipment);
            shipmentsRootObject.Shipments.Add(shipmentDto);

            var json = JsonFieldsSerializer.Serialize(shipmentsRootObject, fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Retrieve all shipments for order
        /// </summary>
        /// <param name="orderId">Id of the order whoes shipments you want to get</param>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shipments/order/{order_id}")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShipmentsByOrderId(int orderId)
        {
            IList<ShipmentDto> shipmentsForOrder = _shipmentApiService.GetShipmentsByOrderId(orderId).Select(x => _dtoHelper.PrepareShipmentDTO(x)).ToList();

            var shipmentsRootObject = new ShipmentsRootObject()
            {
                Shipments = shipmentsForOrder
            };

            return Ok(shipmentsRootObject);
        }

        [HttpPost]
        [Route("/api/shipments")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public IActionResult CreateShipment([ModelBinder(typeof(JsonModelBinder<ShipmentDto>))] Delta<ShipmentDto> shipmentDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            //if (shipmentDelta.Dto.OrderId == null)
            //{
            //    return Error();
            //}

            // We doesn't have to check for value because this is done by the shipment validator.
            var order = _orderService.GetOrderById(shipmentDelta.Dto.OrderId);
            
            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            //var shippingRequired = false;

            //if (shipmentDelta.Dto.ShipmentItems != null)
            //{
            //    var shouldReturnError = AddShipmentItemsToShipment(shipmentDelta.Dto.ShipmentItems, order, shipmentDelta.Dto.StoreId ?? _storeContext.CurrentStore.Id);
            //    if (shouldReturnError)
            //    {
            //        return Error(HttpStatusCode.BadRequest);
            //    }

            //    //shippingRequired = IsShippingAddressRequired(shipmentDelta.Dto.ShipmentItems);
            //}

            //if (shippingRequired)
            //{
            //    var isValid = true;

            //    isValid &= SetShippingOption(shipmentDelta.Dto.ShippingRateComputationMethodSystemName,
            //                                shipmentDelta.Dto.ShippingMethod,
            //                                shipmentDelta.Dto.StoreId ?? _storeContext.CurrentStore.Id,
            //                                customer, 
            //                                BuildShoppingCartItemsFromOrderItemDtos(shipmentDelta.Dto.OrderItems.ToList(), 
            //                                                                        customer.Id, 
            //                                                                        shipmentDelta.Dto.StoreId ?? _storeContext.CurrentStore.Id));

            //    if (!isValid)
            //    {
            //        return Error(HttpStatusCode.BadRequest);
            //    }
            //}

            var newShipment = _factory.Initialize();
            shipmentDelta.Merge(newShipment);

            //customer.BillingAddress = newShipment.BillingAddress;
            //customer.ShippingAddress = newShipment.ShippingAddress;

            newShipment.Order = order;

            //// The default value will be the currentStore.id, but if it isn't passed in the json we need to set it by hand.
            //if (!shipmentDelta.Dto.StoreId.HasValue)
            //{
            //    newShipment.StoreId = _storeContext.CurrentStore.Id;
            //}

            var shipmentDto = _dtoHelper.PrepareShipmentDTO(newShipment);

            //var placeOrderResult = PlaceOrder(newShipment, customer);

            //if (!placeOrderResult.Success)
            //{
            //    foreach (var error in placeOrderResult.Errors)
            //    {
            //        ModelState.AddModelError("order placement", error);
            //    }

            //    return Error(HttpStatusCode.BadRequest);
            //}

            //OrderActivityService.InsertActivity("AddNewShipment",
            //     LocalizationService.GetResource("ActivityLog.AddNewShipment"), newShipment);

            var shipmentsRootObject = new ShipmentsRootObject();

            //var placedOrderDto = _dtoHelper.PrepareOrderDTO(placeOrderResult.PlacedOrder);

            shipmentsRootObject.Shipments.Add(shipmentDto);

            var json = JsonFieldsSerializer.Serialize(shipmentsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/shipments/{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteShipment(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }
            
            var shipmentToDelete = _shipmentApiService.GetShipmentById(id);

            if (shipmentToDelete == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            _shipmentService.DeleteShipment(shipmentToDelete);

            ////activity log
            //OrderActivityService.InsertActivity("DeleteShipment", LocalizationService.GetResource("ActivityLog.DeleteShipment"), shipmentToDelete);

            return new RawJsonActionResult("{}");
        }

        [HttpPut]
        [Route("/api/shipments/{id}")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public IActionResult UpdateShipment([ModelBinder(typeof(JsonModelBinder<ShipmentDto>))] Delta<ShipmentDto> shipmentDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var currentShipment = _shipmentApiService.GetShipmentById(shipmentDelta.Dto.Id);

            if (currentShipment == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var order = currentShipment.Order;

            //var shippingRequired = currentShipment.OrderItems.Any(item => !item.Product.IsFreeShipping);

            //if (shippingRequired)
            //{
            //    var isValid = true;

            //    if (!string.IsNullOrEmpty(shipmentDelta.Dto.ShippingRateComputationMethodSystemName) ||
            //        !string.IsNullOrEmpty(shipmentDelta.Dto.ShippingMethod))
            //    {
            //        var storeId = shipmentDelta.Dto.StoreId ?? _storeContext.CurrentStore.Id;

            //        isValid &= SetShippingOption(shipmentDelta.Dto.ShippingRateComputationMethodSystemName ?? currentShipment.ShippingRateComputationMethodSystemName,
            //            shipmentDelta.Dto.ShippingMethod, 
            //            storeId,
            //            customer, BuildShoppingCartItemsFromOrderItems(currentShipment.OrderItems.ToList(), customer.Id, storeId));
            //    }

            //    if (isValid)
            //    {
            //        currentShipment.ShippingMethod = shipmentDelta.Dto.ShippingMethod;
            //    }
            //    else
            //    {
            //        return Error(HttpStatusCode.BadRequest);
            //    }
            //}

            shipmentDelta.Merge(currentShipment);

            //customer.BillingAddress = currentShipment.BillingAddress;
            //customer.ShippingAddress = currentShipment.ShippingAddress;

            _shipmentService.UpdateShipment(currentShipment);

            //OrderActivityService.InsertActivity("UpdateShipment",
            //     LocalizationService.GetResource("ActivityLog.UpdateShipment"), currentShipment);

            var shipmentsRootObject = new ShipmentsRootObject();

            var placedShipmentDto = _dtoHelper.PrepareShipmentDTO(currentShipment);
            //placedShipmentDto.ShippingMethod = shipmentDelta.Dto.ShippingMethod;

            shipmentsRootObject.Shipments.Add(placedShipmentDto);

            var json = JsonFieldsSerializer.Serialize(shipmentsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        //private bool SetShippingOption(string shippingRateComputationMethodSystemName, string shippingOptionName, int storeId, Customer customer, List<ShoppingCartItem> shoppingCartItems)
        //{
        //    var isValid = true;

        //    if (string.IsNullOrEmpty(shippingRateComputationMethodSystemName))
        //    {
        //        isValid = false;

        //        ModelState.AddModelError("shipping_rate_computation_method_system_name",
        //            "Please provide shipping_rate_computation_method_system_name");
        //    }
        //    else if (string.IsNullOrEmpty(shippingOptionName))
        //    {
        //        isValid = false;

        //        ModelState.AddModelError("shipping_option_name", "Please provide shipping_option_name");
        //    }
        //    else
        //    {
        //        var shippingOptionResponse = _shippingService.GetShippingOptions(shoppingCartItems, customer.ShippingAddress, customer,
        //                shippingRateComputationMethodSystemName, storeId);

        //        if (shippingOptionResponse.Success)
        //        {
        //            var shippingOptions = shippingOptionResponse.ShippingOptions.ToList();

        //            var shippingOption = shippingOptions
        //                .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(shippingOptionName, StringComparison.InvariantCultureIgnoreCase));
                    
        //            _genericAttributeService.SaveAttribute(customer,
        //                NopCustomerDefaults.SelectedShippingOptionAttribute,
        //                shippingOption, storeId);
        //        }
        //        else
        //        {
        //            isValid = false;

        //            foreach (var errorMessage in shippingOptionResponse.Errors)
        //            {
        //                ModelState.AddModelError("shipping_option", errorMessage);
        //            }
        //        }
        //    }

        //    return isValid;
        //}

        //private List<ShoppingCartItem> BuildShoppingCartItemsFromOrderItems(List<OrderItem> orderItems, int orderId, int storeId)
        //{
        //    var shoppingCartItems = new List<ShoppingCartItem>();

        //    foreach (var orderItem in orderItems)
        //    {
        //        shoppingCartItems.Add(new ShoppingCartItem()
        //        {
        //            ProductId = orderItem.ProductId,
        //            orderId = orderId,
        //            Quantity = orderItem.Quantity,
        //            RentalStartDateUtc = orderItem.RentalStartDateUtc,
        //            RentalEndDateUtc = orderItem.RentalEndDateUtc,
        //            StoreId = storeId,
        //            Product = orderItem.Product,
        //            ShoppingCartType = ShoppingCartType.ShoppingCart
        //        });
        //    }

        //    return shoppingCartItems;
        //}

        //private List<ShoppingCartItem> BuildShoppingCartItemsFromOrderItemDtos(List<OrderItemDto> orderItemDtos, int orderId, int storeId)
        //{
        //    var shoppingCartItems = new List<ShoppingCartItem>();

        //    foreach (var orderItem in orderItemDtos)
        //    {
        //        if (orderItem.ProductId != null)
        //        {
        //            shoppingCartItems.Add(new ShoppingCartItem()
        //            {
        //                ProductId = orderItem.ProductId.Value, // required field
        //                orderId = orderId,
        //                Quantity = orderItem.Quantity ?? 1,
        //                RentalStartDateUtc = orderItem.RentalStartDateUtc,
        //                RentalEndDateUtc = orderItem.RentalEndDateUtc,
        //                StoreId = storeId,
        //                Product = _productService.GetProductById(orderItem.ProductId.Value),
        //                ShoppingCartType = ShoppingCartType.ShoppingCart
        //            });
        //        }
        //    }

        //    return shoppingCartItems;
        //}

        //private PlaceOrderResult PlaceOrder(Order newShipment, Customer customer)
        //{
        //    var processPaymentRequest = new ProcessPaymentRequest
        //    {
        //        StoreId = newShipment.StoreId,
        //        orderId = customer.Id,
        //        PaymentMethodSystemName = newShipment.PaymentMethodSystemName
        //    };


        //    var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

        //    return placeOrderResult;
        //}

        //private bool IsShippingAddressRequired(ICollection<OrderItemDto> orderItems)
        //{
        //    var shippingAddressRequired = false;

        //    foreach (var orderItem in orderItems)
        //    {
        //        if (orderItem.ProductId != null)
        //        {
        //            var product = _productService.GetProductById(orderItem.ProductId.Value);

        //            shippingAddressRequired |= product.IsShipEnabled;
        //        }
        //    }

        //    return shippingAddressRequired;
        //}

        //private bool AddOrderItemsToCart(ICollection<OrderItemDto> orderItems, Customer customer, int storeId)
        //{
        //    var shouldReturnError = false;

        //    foreach (var orderItem in orderItems)
        //    {
        //        if (orderItem.ProductId != null)
        //        {
        //            var product = _productService.GetProductById(orderItem.ProductId.Value);

        //            if (!product.IsRental)
        //            {
        //                orderItem.RentalStartDateUtc = null;
        //                orderItem.RentalEndDateUtc = null;
        //            }

        //            var attributesXml = _productAttributeConverter.ConvertToXml(orderItem.Attributes.ToList(), product.Id);                

        //            var errors = _shoppingCartService.AddToCart(customer, product,
        //                ShoppingCartType.ShoppingCart, storeId,attributesXml,
        //                0M, orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
        //                orderItem.Quantity ?? 1);

        //            if (errors.Count > 0)
        //            {
        //                foreach (var error in errors)
        //                {
        //                    ModelState.AddModelError("order", error);
        //                }

        //                shouldReturnError = true;
        //            }
        //        }
        //    }

        //    return shouldReturnError;
        //}
     }
}