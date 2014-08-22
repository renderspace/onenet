using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;

namespace OneMainWeb
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/Optimized/_js").IncludeDirectory("~/site_specific/_js", "*.js"));

            bundles.Add(new ScriptBundle("~/Optimized/Bootstrap").Include("~/JavaScript/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/Optimized/JavaScript").IncludeDirectory("~/JavaScript", "*.js"));
            bundles.Add(new ScriptBundle("~/Optimized/Scripts").IncludeDirectory("~/Scripts", "*.js"));

            bundles.Add(new StyleBundle("~/Optimized/adm/css").IncludeDirectory("~/adm/css", "*.css"));
            bundles.Add(new StyleBundle("~/Optimized/_css").IncludeDirectory("~/site_specific/_css", "*.css"));

            /*
            bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include(
                            "~/Scripts/WebForms/WebForms.js",
                            "~/Scripts/WebForms/WebUIValidation.js",
                            "~/Scripts/WebForms/MenuStandards.js",
                            "~/Scripts/WebForms/Focus.js",
                            "~/Scripts/WebForms/GridView.js",
                            "~/Scripts/WebForms/DetailsView.js",
                            "~/Scripts/WebForms/TreeView.js",
                            "~/Scripts/WebForms/WebParts.js"));

            // Order is very important for these files to work, they have explicit dependencies
            bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs").Include(
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));

            

            ScriptManager.ScriptResourceMapping.AddDefinition(
                "respond",
                new ScriptResourceDefinition
                {
                    Path = "~/Scripts/respond.min.js",
                    DebugPath = "~/Scripts/respond.js",
                }); */
        }
    }
}