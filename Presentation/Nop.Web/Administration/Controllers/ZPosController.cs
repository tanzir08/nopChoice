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
        private readonly CatalogSettings _catalogSettings;

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
            CatalogSettings catalogSettings)
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

        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public ActionResult Create(TopicModel model, bool continueEditing)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
        //        return AccessDeniedView();

        //    if (ModelState.IsValid)
        //    {
        //        if (!model.IsPasswordProtected)
        //        {
        //            model.Password = null;
        //        }

        //        var topic = model.ToEntity();
        //        _topicService.InsertTopic(topic);
        //        //search engine name
        //        model.SeName = topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true);
        //        _urlRecordService.SaveSlug(topic, model.SeName, 0);
        //        //ACL (customer roles)
        //        SaveTopicAcl(topic, model);
        //        //Stores
        //        SaveStoreMappings(topic, model);
        //        //locales
        //        UpdateLocales(topic, model);

        //        SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Added"));

        //        //activity log
        //        _customerActivityService.InsertActivity("AddNewTopic", _localizationService.GetResource("ActivityLog.AddNewTopic"), topic.Title ?? topic.SystemName);

        //        if (continueEditing)
        //        {
        //            //selected tab
        //            SaveSelectedTabName();

        //            return RedirectToAction("Edit", new { id = topic.Id });
        //        }
        //        return RedirectToAction("List");

        //    }

        //    //If we got this far, something failed, redisplay form

        //    //templates
        //    PrepareTemplatesModel(model);
        //    //ACL
        //    PrepareAclModel(model, null, true);
        //    //Stores
        //    PrepareStoresMappingModel(model, null, true);
        //    return View(model);
        //}

        //public ActionResult Edit(int id)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
        //        return AccessDeniedView();

        //    var topic = _topicService.GetTopicById(id);
        //    if (topic == null)
        //        //No topic found with the specified id
        //        return RedirectToAction("List");

        //    var model = topic.ToModel();
        //    model.Url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, "http");
        //    //templates
        //    PrepareTemplatesModel(model);
        //    //ACL
        //    PrepareAclModel(model, topic, false);
        //    //Store
        //    PrepareStoresMappingModel(model, topic, false);
        //    //locales
        //    AddLocales(_languageService, model.Locales, (locale, languageId) =>
        //    {
        //        locale.Title = topic.GetLocalized(x => x.Title, languageId, false, false);
        //        locale.Body = topic.GetLocalized(x => x.Body, languageId, false, false);
        //        locale.MetaKeywords = topic.GetLocalized(x => x.MetaKeywords, languageId, false, false);
        //        locale.MetaDescription = topic.GetLocalized(x => x.MetaDescription, languageId, false, false);
        //        locale.MetaTitle = topic.GetLocalized(x => x.MetaTitle, languageId, false, false);
        //        locale.SeName = topic.GetSeName(languageId, false, false);
        //    });

        //    return View(model);
        //}

        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public ActionResult Edit(TopicModel model, bool continueEditing)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
        //        return AccessDeniedView();

        //    var topic = _topicService.GetTopicById(model.Id);
        //    if (topic == null)
        //        //No topic found with the specified id
        //        return RedirectToAction("List");

        //    if (!model.IsPasswordProtected)
        //    {
        //        model.Password = null;
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        topic = model.ToEntity(topic);
        //        _topicService.UpdateTopic(topic);
        //        //search engine name
        //        model.SeName = topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true);
        //        _urlRecordService.SaveSlug(topic, model.SeName, 0);
        //        //ACL (customer roles)
        //        SaveTopicAcl(topic, model);
        //        //Stores
        //        SaveStoreMappings(topic, model);
        //        //locales
        //        UpdateLocales(topic, model);

        //        SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Updated"));

        //        //activity log
        //        _customerActivityService.InsertActivity("EditTopic", _localizationService.GetResource("ActivityLog.EditTopic"), topic.Title ?? topic.SystemName);

        //        if (continueEditing)
        //        {
        //            //selected tab
        //            SaveSelectedTabName();

        //            return RedirectToAction("Edit",  new {id = topic.Id});
        //        }
        //        return RedirectToAction("List");
        //    }


        //    //If we got this far, something failed, redisplay form

        //    model.Url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, "http");
        //    //templates
        //    PrepareTemplatesModel(model);
        //    //ACL
        //    PrepareAclModel(model, topic, true);
        //    //Store
        //    PrepareStoresMappingModel(model, topic, true);
        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult Delete(int id)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
        //        return AccessDeniedView();

        //    var topic = _topicService.GetTopicById(id);
        //    if (topic == null)
        //        //No topic found with the specified id
        //        return RedirectToAction("List");

        //    _topicService.DeleteTopic(topic);

        //    SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Deleted"));

        //    //activity log
        //    _customerActivityService.InsertActivity("DeleteTopic", _localizationService.GetResource("ActivityLog.DeleteTopic"), topic.Title ?? topic.SystemName);

        //    return RedirectToAction("List");
        //}

        #endregion
    }
}
