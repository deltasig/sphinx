namespace DeltaSigmaPhiWebsite.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;

    public class AddressIndexFilterModel : MemberStatusFilter
    {
        public IEnumerable<Address> Addresses { get; set; }
    }
}