using BugsMVC.DAL;
using BugsMVC.Helpers;
using BugsMVC.ModelBinderClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BugsMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ClientDataTypeModelValidatorProvider.ResourceClassKey = "Messages";
            DefaultModelBinder.ResourceClassKey = "Messages";

            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));
            //ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
        }

        protected void Session_Start() {
            BugsContext db = new BugsContext();
            SecurityHelper.Initialize(db.FuncionRoles.ToList(), db.Funciones.ToList());
        } 
    }
}
