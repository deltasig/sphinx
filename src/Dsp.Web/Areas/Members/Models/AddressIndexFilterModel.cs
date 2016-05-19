namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class AddressIndexFilterModel : MemberStatusFilter
    {
        public IEnumerable<Address> Addresses { get; set; }
    }
}