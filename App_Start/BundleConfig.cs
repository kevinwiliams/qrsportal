﻿using System.Web;
using System.Web.Optimization;

namespace QRSPortal2
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/moment.min.js"
                        ));

            bundles.Add(new Bundle("~/bundles/jqueryval").Include(
                       "~/Scripts/jquery.validate.min.js",
                       "~/Scripts/jquery.validate.unobtrusive.min.js",
                       "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/Scripts/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.js"));

            bundles.Add(new Bundle("~/Scripts/js").Include(
                     //"~/Content/chart.js/chart.umd.js",
                      "~/Scripts/sweetalert.min.js",
                     "~/Content/simple-datatables/simple-datatables.js",
                     "~/Content/apexcharts/apexcharts.min.js",
                     "~/Scripts/main.js",
                     "~/Scripts/addons.js"));

            bundles.Add(new StyleBundle("~/Content/vendor").Include(
                    "~/Content/bootstrap-icons/bootstrap-icons.css",
                    "~/Content/boxicons/css/boxicons.min.css",
                    "~/Content/apexcharts/apexcharts.css",
                    "~/Content/simple-datatables/style.css?v=1.1"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/sweetalert.css",
                      "~/Content/style.css",
                      "~/Content/site.css"));
        }
    }
}
