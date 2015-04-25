using System.Web;
using System.Web.Optimization;

namespace OpenSourceScrumTool
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery-ui-touchpunch.js",
                        "~/Scripts/jquery.debounce-1.0.5.js"));

            bundles.Add(new ScriptBundle("~/bundles/tinymce").Include(
                        "~/Scripts/tinymce/tinymce.min.js",
                        "~/Scripts/jquery.tinymce.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                        "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/ajax").Include(
                        "~/Scripts/jquery.unobtrusive-ajax.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-datepicker.js",
                      "~/Scripts/bootbox.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                        "~/Scripts/knockout-{version}.js",
                        "~/Scripts/knockout.mapping-latest.js"));

            bundles.Add(new ScriptBundle("~/bundles/site")
                .Include("~/Scripts/Site.js",
                         "~/Scripts/Scrum/DelegatedAccess.js",
                         "~/Scripts/Scrum/LiveUpdate.js",
                         "~/Scripts/Scrum/Sprint.js",
                         "~/Scripts/Scrum/Task.js",
                         "~/Scripts/Scrum/TextEdit.js"));

            bundles.Add(new ScriptBundle("~/bundles/custombootstrap").Include(
                        "~/Scripts/modalForm.js",
                        "~/Scripts/ajaxForm.js",
                        "~/Scripts/dismissablePanel.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap-flatly.css",
                      "~/Content/datepicker3.css",
                      "~/Content/site.css",
                      "~/Content/Scrum.css",
                      "~/Content/css/font-awesome.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = false;
        }
    }
}
