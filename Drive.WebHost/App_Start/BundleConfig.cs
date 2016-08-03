﻿using System.Web.Optimization;

namespace Drive.WebHost
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/Libs/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/Libs/jquery.validate*"));
            
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/Libs/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/App/"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/Libs/bootstrap.js",
                "~/Scripts/Libs/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                       "~/Scripts/Libs/angular.js",
                       "~/Scripts/Libs/angular-route.js",
                       "~/Scripts/Libs/angular-sanitize.js"));

            bundles.Add(new ScriptBundle("~/bundles/route").Include(
                       "~/Scripts/App/Routing.js"));

            bundles.Add(new ScriptBundle("~/bundles/controllers").Include(
                       "~/Scripts/App/Home/HomeController.js"));
        }
    }
}