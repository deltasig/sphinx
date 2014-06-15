namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class CreateAddressModel
    {
        public IEnumerable<SelectListItem> Users { get; set; }
        public Address Address { get; set; }
    }
}