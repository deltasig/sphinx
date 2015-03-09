namespace DeltaSigmaPhiWebsite.Areas.House.Controllers
{
	using System.Web.Mvc;
	using DeltaSigmaPhiWebsite.Controllers;

	[Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
	public class RoomsController : BaseController
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}