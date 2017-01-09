using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.ZPos;
using Nop.Core.Domain.Topics;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.Checkout;
using Nop.Services.Payments;
using Nop.Services.Common;

namespace Nop.Admin.Controllers
{
    public partial class ZPosController : BaseAdminController
    {
        #region Fields

        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ITopicTemplateService _topicTemplateService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAclService _aclService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;

        #endregion Fields

        #region Constructors

        public ZPosController(ITopicService topicService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService, 
            ILocalizationService localizationService,
            IPermissionService permissionService, 
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            ITopicTemplateService topicTemplateService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IAclService aclService,
            CatalogSettings catalogSettings,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            OrderSettings orderSettings)
        {
            this._topicService = topicService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._urlRecordService = urlRecordService;
            this._topicTemplateService = topicTemplateService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._productService = productService;
            this._shoppingCartService = shoppingCartService;
            this._storeContext = storeContext;
            this._genericAttributeService = genericAttributeService;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._orderSettings = orderSettings;
        }

        #endregion
        

        #region POS

        public ActionResult Order()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ZposModel();
            model.SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength;

            var customers = _customerService.GetAllCustomers(customerRoleIds: new[] { 3 });

            foreach (var c in customers)
                model.Customers.Add(new SelectListItem
                {
                    Text = c.GetFullName(),
                    Value = c.Id.ToString()
                });

            return View(model);
        }

        [HttpPost]
        public ActionResult Order(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int number, OrderId = 0;
            var model = new ZposModel();
            IList<string> addToCartWarnings = new List<string>();
            var products = _productService.SearchProducts();
            int customerId = Int32.TryParse(form["customers"], out number) ? number : 0;
            int discount = Int32.TryParse(form["discount"], out number) ? number : 0;

            var customer = _customerService.GetCustomerById(customerId);

            //claer previous cart
            customer.ShoppingCartItems.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));

            foreach (var product in products)
            {
                string item = form[string.Format("productId{0}", product.Id)];

                if (!String.IsNullOrEmpty(item))
                {
                    int itemQuantity = Int32.TryParse(form[string.Format("qty{0}", product.Id)], out number) ? number : 0;

                    addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                                        product: product,
                                        shoppingCartType: ShoppingCartType.ShoppingCart,
                                        storeId: _storeContext.CurrentStore.Id,
                                        quantity: itemQuantity);
                    if (addToCartWarnings.Count > 0)
                    {
                        break;
                    }
                }
            }


            if (addToCartWarnings.Count == 0)
            {
                #region Order
                //validation
                customer = _customerService.GetCustomerById(customerId);

                var cart = customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();

                if (!cart.Any())
                    return RedirectToAction("Order");

                if (customer.IsGuest())
                    return new HttpUnauthorizedResult();


                //model
                try
                {
                    var processPaymentRequest = new ProcessPaymentRequest();

                    //prevent 2 orders being placed within an X seconds time frame
                    if (!IsMinimumOrderPlacementIntervalValid(customer))
                        throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

                    //place order
                    processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                    processPaymentRequest.CustomerId = customer.Id;
                    processPaymentRequest.PaymentMethodSystemName = "Payments.PurchaseOrder";
                    var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                    OrderId = placeOrderResult.PlacedOrder.Id;
                    if (placeOrderResult.Success)
                    {
                        return RedirectToRoute("PrintOrderDetails", new { orderId = OrderId });
                    }

                    foreach (var error in placeOrderResult.Errors)
                        model.Warnings.Add(error);
                }
                catch (Exception exc)
                {
                    model.Warnings.Add(exc.Message);
                }

                //If we got this far, something failed, redisplay form
                model.SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength;

                var customers = _customerService.GetAllCustomers(customerRoleIds: new[] { 3 });

                foreach (var c in customers)
                    model.Customers.Add(new SelectListItem
                    {
                        Text = c.GetFullName(),
                        Value = c.Id.ToString()
                    });

                return View(model);
                #endregion
            }
 

            return RedirectToAction("Order");
        }

        #endregion

        [NonAction]
        protected virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: customer.Id, pageSize: 1)
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }
    }
}
