namespace Dsp.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;

    public class PhoneIndexFilterModel : MemberStatusFilter
    {
        public IEnumerable<PhoneNumber> PhoneNumbers { get; set; }
    }
}