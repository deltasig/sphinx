namespace Dsp
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Styles
            bundles.Add(new StyleBundle("~/content/open-sans")
                .Include("~/Content/OpenSans.css"));
            bundles.Add(new StyleBundle("~/content/bootstrap-core")
                .Include("~/Content/bootstrap.css")
                .Include("~/Content/bootstrap-select.css"));
            bundles.Add(new StyleBundle("~/content/custom-css")
                .Include("~/Content/Site.css"));
            bundles.Add(new StyleBundle("~/content/font-awesome")
                .Include("~/Content/font-awesome.css"));

            bundles.Add(new StyleBundle("~/content/bootstrap-table")
                .Include("~/Content/bootstrap-table.css"));
            bundles.Add(new StyleBundle("~/content/datepicker")
                .Include("~/Content/datepicker.css"));
            bundles.Add(new StyleBundle("~/content/datetimepicker")
                .Include("~/Content/bootstrap-datetimepicker-build.css"));
            bundles.Add(new StyleBundle("~/content/multi-select")
                .Include("~/Content/multi-select.css"));
            bundles.Add(new StyleBundle("~/content/markdown")
                .Include("~/Content/wmd.css"));

            // Scripts
            bundles.Add(new ScriptBundle("~/bundles/bootstrap-core")
                .Include("~/Scripts/bootstrap.js")
                .Include("~/Scripts/bootstrap-select.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js")
                .Include("~/Scripts/jquery-ui-{version}.js")
                .Include("~/Scripts/jquery.unobtrusive*")
                .Include("~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-table")
                .Include("~/Scripts/bootstrap-table.js"));
            bundles.Add(new ScriptBundle("~/bundles/datepicker")
                .Include("~/Scripts/bootstrap-datepicker.js"));
            bundles.Add(new ScriptBundle("~/bundles/datetimepicker")
                .Include("~/Scripts/moment.js")
                .Include("~/Scripts/bootstrap-datetimepicker.js"));
            bundles.Add(new ScriptBundle("~/bundles/markdown")
                .Include("~/Scripts/pagedown/Markdown.Converter.js")
                .Include("~/Scripts/pagedown/Markdown.Sanitizer.js")
                .Include("~/Scripts/pagedown/Markdown.Editor.js"));
            bundles.Add(new ScriptBundle("~/bundles/multi-select")
                .Include("~/Scripts/jquery.multi-select.js")
                .Include("~/Scripts/jquery.quicksearch.js"));

            BundleTable.EnableOptimizations = true;
        }
    }
}