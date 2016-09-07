using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Topics;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.ZPos
{
    public partial class ZposModel : BaseNopEntityModel
    {
        public ZposModel()
        {
            Customers = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Customers")]
        [AllowHtml]
        public IList<SelectListItem> Customers { get; set; }

        public int SearchTermMinimumLength { get; set; }

    }
}