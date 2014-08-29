using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using BundleTransformer.SassAndScss.Translators;
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
        public static void RegisterBundles(BundleCollection bundles)
        {
            var saasTransformer = new SassAndScssTranslator();
            var nullBuilder = new NullBuilder();
            var scriptTransformer = new ScriptTransformer();
            var styleTransformer = new StyleTransformer();
            var nullOrderer = new NullOrderer();

            // ADMIN
            bundles.Add(new StyleBundle("~/Bundles/BoostrapCSS").Include("~/adm/css/bootstrap.css"));
            bundles.Add(new ScriptBundle("~/Bundles/BootstrapJS").Include("~/adm/js/bootstrap.js"));
            bundles.Add(new ScriptBundle("~/Bundles/AdmJS").IncludeDirectory("~/adm/js", "*.js"));
            bundles.Add(new ScriptBundle("~/Bundles/Scripts").IncludeDirectory("~/Scripts", "*.js").Include("~/Scripts/dropzone/dropzone.js"));
            bundles.Add(new StyleBundle("~/Bundles/adm/css").IncludeDirectory("~/adm/css", "*.css"));
            // END ADMIN

            // JQUERY
            var jQueryBundle = new Bundle("~/Bundles/Jquery", "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.1.min.js");
            jQueryBundle.Include("~/Scripts/jquery-{version}.js");
            jQueryBundle.Builder = nullBuilder;
            jQueryBundle.Transforms.Add(scriptTransformer);
            jQueryBundle.Orderer = nullOrderer;
            jQueryBundle.CdnFallbackExpression = "window.jquery";
            bundles.Add(jQueryBundle);

            // REGULAR SITE CSS
            bundles.Add(new StyleBundle("~/Bundles/_css").IncludeDirectory("~/site_specific/_css", "*.css"));
            // REGULAR SITE JS
            bundles.Add(new ScriptBundle("~/Bundles/_js").IncludeDirectory("~/site_specific/_js", "*.js"));
            // REGULAR SaSS
            var saasBundle = new Bundle("~/Bundles/Sass").IncludeDirectory("~/site_specific/_sass", "*.sass").IncludeDirectory("~/site_specific/_sass", "*.scss");
            saasBundle.Builder = nullBuilder;
            saasBundle.Transforms.Add(styleTransformer);
            saasBundle.Orderer = nullOrderer;
            bundles.Add(saasBundle);

            if (PresentBasePage.ReadPublishFlag())
            {
                BundleTable.EnableOptimizations = true;
            }

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