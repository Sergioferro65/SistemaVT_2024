using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugsMVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Public_PageNotFound",
                url: "PaginaNoEncontrada",
                defaults: new { controller = "Home", action = "PageNotFound", id = UrlParameter.Optional }
            );

            routes.MapRoute(
             name: "Public_PageNotFound2",
             url: "PageNotFound",
             defaults: new { controller = "Base", action = "PageNotFound", id = UrlParameter.Optional }
         );

            routes.MapRoute(
                    name: "SeguridadPorRol",
                    url: "SeguridadPorRol",
                    defaults: new { controller = "Seguridad", action = "Create", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "Public_ErrorPage",
                url: "Mantenimiento",
                defaults: new { controller = "Home", action = "ErrorPage", id = UrlParameter.Optional }
            );
        }
    }
}
