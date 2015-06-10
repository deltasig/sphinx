namespace Dsp.Data
{
    using Entities;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity;

    public class SphinxUserStore : UserStore<Member, Position, int, SphinxUserLogin, Leader, SphinxUserClaim>
    {
        public SphinxUserStore(DbContext context) : base(context) { }
    } 
}