namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Data.UnitOfWork;

    public class BaseController : Controller
    {
        protected readonly IUnitOfWork uow = new UnitOfWork();
        public BaseController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        protected IEnumerable<SelectListItem> GetUserIdListAsFullName()
        {
            var members = uow.MemberRepository.GetAll().OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected IEnumerable<SelectListItem> GetUserIdListAsFullNameWithNone()
        {
            var members = uow.MemberRepository.GetAll().OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }

        protected override void Dispose(bool disposing)
        {
            uow.Dispose();
            base.Dispose(disposing);
        }
    }
}