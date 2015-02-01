namespace DeltaSigmaPhiWebsite.App_Start
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Styles
            bundles.Add(new StyleBundle("~/Content/bootstrap")
                .Include("~/Content/bootstrap/css/bootstrap.css"));
            bundles.Add(new StyleBundle("~/Content/custom-css")
                .Include("~/Content/Site.css"));
            bundles.Add(new StyleBundle("~/Content/font-awesome")
                .Include("~/Content/font-awesome.css"));
            bundles.Add(new StyleBundle("~/Content/Multiselect")
                .Include("~/Content/multi-select.css"));

            // Scripts
            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Scripts/bootstrap.js")
                .Include("~/Scripts/moment.js")
                .Include("~/Scripts/bootstrap-datetimepicker.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/DataTables-1.10.4/media/js/jquery.dataTables.js")
                .Include("~/Scripts/jquery-{version}.js")
                .Include("~/Scripts/jquery-ui-{version}.js")
                .Include("~/Scripts/jquery.unobtrusive*")
                .Include("~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/Multiselect")
                .Include("~/Scripts/jquery.multi-select.js")
                .Include("~/Scripts/jquery.quicksearch.js"));
        }
    }
}