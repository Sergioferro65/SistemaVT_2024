﻿using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;
using System.Collections.Generic;
using IO = System.IO;
using System.Linq;

namespace BugsMVC
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js",
            //            "~/Scripts/jquery-ui.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            //// Use the development version of Modernizr to develop with and learn from. Then, when you're
            //// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/bootstrap-datepicker.js",
            //          "~/Scripts/bootstrap-table.js",
            //          "~/Scripts/respond.js"));

            //bundles.Add(new ScriptBundle("~/bundles/chart").Include(
            //         "~/Scripts/chart-data.js",
            //         "~/Scripts/easypiechart.js",
            //         "~/Scripts/easypiechart-data.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap-table.css",
            //          "~/Content/bootstrap-theme.css",
            //          "~/Content/bootstrap.css",
            //          "~/Content/datepicker.css",
            //          "~/Content/datepicker3.css",
            //          "~/Content/site.css",
            //          "~/Content/font-awesome.css",
            //          "~/Content/styles.css"));

            bundles.Add(new StyleFilePathBundle("~/Content/css").Include(
                "~/Vendor/simple-line-icons/css/simple-line-icons.css",
                "~/Content/font-awesome.css",
                "~/Vendor/fontawesome/css/font-awesome.min.css"));

            // App Styles
            bundles.Add(new StyleBundle("~/Content/appCss").Include(
                "~/Content/app/css/app.css"
            ));

            // Bootstrap Styles
            bundles.Add(new StyleBundle("~/Content/bootstrapCss").Include(
                "~/Content/app/css/bootstrap.css", new CssRewriteUrlTransform()
            ));

            bundles.Add(new ScriptBundle("~/bundles/Angle").Include(
                // App init
                "~/Scripts/app/app.init.js",
                // Modules
                "~/Scripts/app/modules/bootstrap-start.js",
                "~/Scripts/app/modules/calendar.js",
                "~/Scripts/app/modules/classyloader.js",
                "~/Scripts/app/modules/clear-storage.js",
                "~/Scripts/app/modules/constants.js",
                "~/Scripts/app/modules/demo",
                "~/Scripts/app/modules/flatdoc.js",
                "~/Scripts/app/modules/fullscreen.js",
                "~/Scripts/app/modules/gmap.js",
                "~/Scripts/app/modules/load-css.js",
                "~/Scripts/app/modules/localize.js",
                "~/Scripts/app/modules/maps-vector.js",
                "~/Scripts/app/modules/navbar-search.js",
                "~/Scripts/app/modules/notify.js",
                "~/Scripts/app/modules/now.js",
                "~/Scripts/app/modules/panel-tools.js",
                "~/Scripts/app/modules/play-animation.js",
                "~/Scripts/app/modules/porlets.js",
                "~/Scripts/app/modules/sidebar.js",
                "~/Scripts/app/modules/skycons.js",
                "~/Scripts/app/modules/slimscroll.js",
                "~/Scripts/app/modules/sparkline.js",
                "~/Scripts/app/modules/table-checkall.js",
                "~/Scripts/app/modules/toggle-state.js",
                "~/Scripts/app/modules/utils.js",
                "~/Scripts/app/modules/chart.js",
                "~/Scripts/app/modules/morris.js",
                "~/Scripts/app/modules/rickshaw.js",
                "~/Scripts/app/modules/chartist.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/paginator").Include(
               "~/Scripts/smartpaginator.js"
           ));

            // Main Vendor

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"
            ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"
            ));

            // Vendor Plugins

            bundles.Add(new ScriptBundle("~/bundles/sparklines").Include(
                "~/Vendor/sparklines/jquery.sparkline.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/ChartJS").Include(
                 "~/Vendor/Chart.js/Chart.js"
            ));

            //bundles.Add(new StyleBundle("~/bundles/simpleLineIcons").Include(
            //  "~/Vendor/simple-line-icons/css/simple-line-icons.css", new CssRewriteUrlTransform().Process(true,HttpContext.Current.Request.ApplicationPath)
            //));

            bundles.Add(new ScriptBundle("~/bundles/storage").Include(
              "~/Vendor/jQuery-Storage-API/jquery.storageapi.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryEasing").Include(
              "~/Vendor/jquery.easing/js/jquery.easing.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
              "~/Vendor/datatables/media/js/jquery.dataTables.min.js",
              "~/Vendor/datatables-colvis/js/dataTables.colVis.js",
              "~/Vendor/datatable-bootstrap/js/dataTables.bootstrap.js",
              "~/Vendor/datatable-bootstrap/js/dataTables.bootstrapPagination.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/datatablesCss")
              .Include("~/Vendor/datatables-colvis/css/dataTables.colVis.css", new CssRewriteUrlTransform())
              .Include("~/Vendor/datatable-bootstrap/css/dataTables.bootstrap.css", new CssRewriteUrlTransform())
            );

            bundles.Add(new ScriptBundle("~/bundles/parsley").Include(
              "~/Vendor/parsleyjs/dist/parsley.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/filestyle").Include(
              "~/Vendor/bootstrap-filestyle/src/bootstrap-filestyle.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/tagsinput").Include(
              "~/Vendor/bootstrap-tagsinput/dist/bootstrap-tagsinput.min.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/tagsinputCss").Include(
              "~/Vendor/bootstrap-tagsinput/dist/bootstrap-tagsinput.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/gmap").Include(
              "~/Vendor/jQuery-gMap/jquery.gmap.min.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/weatherIcons").Include(
              "~/Vendor/weather-icons/css/weather-icons.min.css", new CssRewriteUrlTransform()
            ));

            bundles.Add(new ScriptBundle("~/bundles/skycons").Include(
              "~/Vendor/skycons/skycons.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/whirl").Include(
              "~/Vendor/whirl/dist/whirl.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/classyloader").Include(
              "~/Vendor/jquery-classyloader/js/jquery.classyloader.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/animo").Include(
              "~/Vendor/animo.js/animo.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/fastclick").Include(
              "~/Vendor/fastclick/lib/fastclick.js"
            ));

            bundles.Add(new StyleFilePathBundle("~/bundles/fontawesome").Include(
              "~/Vendor/fontawesome/css/font-awesome.min.css", new CssRewriteUrlTransform()
            ));


            bundles.Add(new ScriptBundle("~/bundles/sliderCtrl").Include(
              "~/Vendor/seiyria-bootstrap-slider/dist/bootstrap-slider.min.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/sliderCtrlCss").Include(
              "~/Vendor/seiyria-bootstrap-slider/dist/css/bootstrap-slider.min.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/wysiwyg").Include(
              "~/Vendor/bootstrap-wysiwyg/bootstrap-wysiwyg.js",
              "~/Vendor/bootstrap-wysiwyg/external/jquery.hotkeys.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/slimscroll").Include(
              "~/Vendor/slimscroll/jquery.slimscroll.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/screenfull").Include(
              "~/Vendor/screenfull/dist/screenfull.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jvectormap").Include(
              "~/Vendor/ika.jvectormap/jquery-jvectormap-1.2.2.min.js",
              "~/Vendor/ika.jvectormap/jquery-jvectormap-world-mill-en.js",
              "~/Vendor/ika.jvectormap/jquery-jvectormap-us-mill-en.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/jvectormapCss").Include(
              "~/Vendor/ika.jvectormap/jquery-jvectormap-1.2.2.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/flot").Include(
              "~/Vendor/flot/jquery.flot.js",
              "~/Vendor/flot.tooltip/js/jquery.flot.tooltip.min.js",
              "~/Vendor/flot/jquery.flot.resize.js",
              "~/Vendor/flot/jquery.flot.pie.js",
              "~/Vendor/flot/jquery.flot.time.js",
              "~/Vendor/flot/jquery.flot.categories.js",
              "~/Vendor/flot-spline/js/jquery.flot.spline.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUi").Include(
                // "~/Vendor/jquery-ui/ui/*.js",
                "~/Vendor/jquery-ui/ui//core.js",
                "~/Vendor/jquery-ui/ui//widget.js",
                "~/Vendor/jquery-ui/ui//mouse.js",
                "~/Vendor/jquery-ui/ui//draggable.js",
                "~/Vendor/jquery-ui/ui//droppable.js",
                "~/Vendor/jquery-ui/ui//sortable.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUiTouchPunch").Include(
              "~/Vendor/jqueryui-touch-punch/jquery.ui.touch-punch.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
              "~/Vendor/moment/min/moment-with-locales.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/inputmask").Include(
              "~/Vendor/jquery.inputmask/dist/jquery.inputmask.bundle.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/flatdoc").Include(
              "~/Vendor/flatdoc/flatdoc.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/chosen").Include(
              "~/Vendor/chosen_v1.2.0/chosen.jquery.min.js"
            ));

            bundles.Add(new StyleFilePathBundle("~/bundles/chosenCss").Include(
              "~/Vendor/chosen_v1.2.0/chosen.min.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/fullcalendar").Include(
              "~/Vendor/fullcalendar/dist/fullcalendar.min.js",
              "~/Vendor/fullcalendar/dist/gcal.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/fullcalendarCss").Include(
              "~/Vendor/fullcalendar/dist/fullcalendar.css"
            ));

            bundles.Add(new StyleBundle("~/bundles/animatecss").Include(
              "~/Vendor/animate.css/animate.min.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/localize").Include(
              "~/Vendor/jquery-localize-i18n/dist/jquery.localize.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/nestable").Include(
              "~/Vendor/nestable/jquery.nestable.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/sortable").Include(
              "~/Vendor/html.sortable/dist/html.sortable.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqGrid").Include(
              "~/Vendor/jqgrid/js/jquery.jqGrid.js",
              "~/Vendor/jqgrid/js/i18n/grid.locale-es.js"
            ));

            bundles.Add(new StyleFilePathBundle("~/bundles/jqGridCss").Include(
                "~/Vendor/jqgrid/css/ui.jqgrid.css",
                "~/Vendor/jquery-ui/themes/smoothness/jquery-ui.css"));

            bundles.Add(new StyleBundle("~/bundles/fileUploadCss").Include(
                "~/Vendor/blueimp-file-upload/css/jquery.fileupload.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/fileUpload").Include(
                "~/Vendor/jquery-ui/ui/widget.js",
                "~/Vendor/blueimp-tmpl/js/tmpl.js",
                "~/Vendor/blueimp-load-image/js/load-image.all.min.js",
                "~/Vendor/blueimp-canvas-to-blob/js/canvas-to-blob.js",
                "~/Vendor/blueimp-file-upload/js/jquery.iframe-transport.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload-process.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload-image.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload-audio.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload-video.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload-validate.js",
                "~/Vendor/blueimp-file-upload/js/jquery.fileupload-ui.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/xEditableCss").Include(
                "~/Vendor/x-editable/dist/bootstrap3-editable/css/bootstrap-editable.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/xEditable").Include(
              "~/Vendor/x-editable/dist/bootstrap3-editable/js/bootstrap-editable.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryValidate").Include(
              "~/Vendor/jquery-validation/dist/jquery.validate.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jquerySteps").Include(
              "~/Vendor/jquery.steps/build/jquery.steps.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/datetimePicker").Include(
              "~/Vendor/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/datetimePickerCss").Include(
                "~/Vendor/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css"
            ));

            bundles.Add(new StyleBundle("~/bundles/RickshawCss").Include(
                "~/Vendor/rickshaw/rickshaw.min.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/Rickshaw").Include(
              "~/Vendor/d3/d3.min.js",
              "~/Vendor/rickshaw/rickshaw.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/ChartistCss").Include(
                "~/Vendor/chartist/dist/chartist.min.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/Chartist").Include(
              "~/Vendor/chartist/dist/chartist.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/MorrisCss").Include(
                "~/Vendor/morris.js/morris.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/Morris").Include(
              "~/Vendor/raphael/raphael.js",
              "~/Vendor/morris.js/morris.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/Spinkit").Include(
                "~/Vendor/spinkit/css/spinkit.css"
            ));

            bundles.Add(new StyleBundle("~/bundles/LoadersCss").Include(
                "~/Vendor/loaders.css/loaders.css"
            ));

        }
    }

    public class StyleFilePathBundle : Bundle
    {
        public StyleFilePathBundle(string virtualPath)
            : base(virtualPath, new IBundleTransform[1]
          {
        (IBundleTransform) new CssMinify()
          })
        {
        }

        public StyleFilePathBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath, new IBundleTransform[1]
          {
        (IBundleTransform) new CssMinify()
          })
        {
        }

        public new Bundle Include(params string[] virtualPaths)
        {
            if (HttpContext.Current.IsDebuggingEnabled && !BundleTable.EnableOptimizations)
            {
                // Debugging. Bundling will not occur so act normal and no one gets hurt.
                base.Include(virtualPaths.ToArray());
                return this;
            }

            // In production mode so CSS will be bundled. Correct image paths.
            var bundlePaths = new List<string>();
            var svr = HttpContext.Current.Server;
            foreach (var path in virtualPaths)
            {
                var pattern = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase);
                var contents = IO.File.ReadAllText(svr.MapPath(path));
                if (!pattern.IsMatch(contents))
                {
                    bundlePaths.Add(path);
                    continue;
                }


                var bundlePath = (IO.Path.GetDirectoryName(path) ?? string.Empty).Replace(@"\", "/") + "/";
                var bundleUrlPath = VirtualPathUtility.ToAbsolute(bundlePath);
                var bundleFilePath = String.Format("{0}{1}.bundle{2}",
                                                   bundlePath,
                                                   IO.Path.GetFileNameWithoutExtension(path),
                                                   IO.Path.GetExtension(path));
                contents = pattern.Replace(contents, "url($1" + bundleUrlPath + "$2$1)");
                IO.File.WriteAllText(svr.MapPath(bundleFilePath), contents);
                bundlePaths.Add(bundleFilePath);
            }
            base.Include(bundlePaths.ToArray());
            return this;
        }

    }
}
