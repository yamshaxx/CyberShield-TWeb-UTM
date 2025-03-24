using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace CyberShieldWeb.App_Start
{
	public class BundleConfig
	{
        public static void RegisterBundle(BundleCollection bundles)
        {
            bundles.Clear();

            // CSS Bundles
            var cssBundle = new StyleBundle("~/Content/css");
            cssBundle.Include(
                "~/Content/bootstrap.min.css",
                "~/Content/bootstrap-grid.min.css",
                "~/Content/bootstrap-reboot.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/css/site.css",
                "~/Content/css/index.css");
            bundles.Add(cssBundle);

            // Favicon Bundle
            var faviconBundle = new StyleBundle("~/Content/favicon");
            faviconBundle.Include("~/favicon.ico");
            bundles.Add(faviconBundle);

            // Site Scripts Bundle
            var siteBundle = new ScriptBundle("~/bundles/site");
            siteBundle.Include(
                "~/Scripts/site.js",
                "~/Scripts/js/site.js",
                "~/Scripts/js/main.js");
            bundles.Add(siteBundle);

            BundleTable.EnableOptimizations = false;
        }
    }
}
