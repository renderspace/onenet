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
            bundles.Add(new StyleBundle("~/Bundles/AdmCSS").Include(
                "~/Scripts/dropzone/css/dropzone.css").Include(
                "~/adm/codemirror/codemirror.css").Include(
                "~/adm/css/one.css").Include(
                "~/adm/css/dashboard.css").Include(
                "~/Content/bootstrap-datetimepicker.css"));

            bundles.Add(new StyleBundle("~/Bundles/adm/css").IncludeDirectory("~/adm/css", "*.css"));
           
            // END ADMIN

            // JQUERY
            var jQueryBundle = new Bundle("~/Bundles/Jquery", "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.3.min.js");
            jQueryBundle.Include("~/Scripts/jquery-2.1.3.min.js");
            jQueryBundle.Builder = nullBuilder;
            jQueryBundle.Transforms.Add(scriptTransformer);
            jQueryBundle.Orderer = nullOrderer;
            jQueryBundle.CdnFallbackExpression = "window.jquery";
            bundles.Add(jQueryBundle);

            // REGULAR SITE CSS
            bundles.Add(new StyleBundle("~/Bundles/_css").IncludeDirectory("~/site_specific/_css", "*.css"));
            // REGULAR SITE JS

            BundleFileSetOrdering bundleFileSetOrdering = new BundleFileSetOrdering("js");
            bundleFileSetOrdering.Files.Add("jquery*");
            bundleFileSetOrdering.Files.Add("bootstrap.js");
            bundles.FileSetOrderList.Add(bundleFileSetOrdering);
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
        }
    }
}