namespace Dsp.WebCore.Controllers;

using Dsp.Data;
using Dsp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Web;

public class BaseController : Controller
{
    protected const string SuccessMessageKey = "SuccessMessage";
    protected const string FailureMessageKey = "FailureMessage";

    private DspDbContext _context;
    private UserManager<User> _userManager;
    private SignInManager<User> _signInManager;

    protected DspDbContext Context => _context ??= HttpContext.RequestServices.GetService<DspDbContext>();
    protected UserManager<User> UserManager => _userManager ??= HttpContext.RequestServices.GetService<UserManager<User>>();
    protected SignInManager<User> SignInManager => _signInManager ??= HttpContext.RequestServices.GetService<SignInManager<User>>();

    protected string RenderRazorViewToString(string viewName, object model)
    {
        ViewData.Model = model;
        using (var sw = new StringWriter())
        {
            IViewEngine viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);
            var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());
            viewResult.View.RenderAsync(viewContext);
            return sw.GetStringBuilder().ToString();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context.Dispose();
        }
        base.Dispose(disposing);
    }
}