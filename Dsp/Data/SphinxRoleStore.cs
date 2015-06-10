namespace Dsp.Data
{
    using Entities;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity;

    public class SphinxRoleStore : RoleStore<Position, int, Leader>
    {
        public SphinxRoleStore(DbContext context) : base(context) { }
    }
}