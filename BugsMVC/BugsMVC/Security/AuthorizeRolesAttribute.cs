using BugsMVC.DAL;
using BugsMVC.Helpers;
using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugsMVC.Security
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        private static BugsContext db = new BugsContext();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var routeValues = httpContext.Request.RequestContext.RouteData.Values;
            var controller = routeValues["Controller"].ToString();
            var action = routeValues["Action"].ToString();
            Guid operadorId = ViewHelper.GetCurrentOperadorId();
            bool isSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");

            var funcionesRoles = GetFuncionesRoles(httpContext);

            var strComparison = StringComparison.InvariantCultureIgnoreCase;
            var rolesController = funcionesRoles.Where(x => x.Funcion.Controller.Equals(controller, strComparison) && x.Funcion.Action == null ).ToList();

            //separado
            var a = funcionesRoles.Where(x => x.Funcion.Controller.Equals(controller, strComparison)).ToList();
            //var b = 
            //    && ((action == null && x.Funcion.Action == null) || (x.Funcion.Action != null && x.Funcion.Action.Equals(action, strComparison)))).ToList();
            //

            var rolesAction = funcionesRoles.Where(x => x.Funcion.Controller.Equals(controller, strComparison) 
                     && ((action == null && x.Funcion.Action == null) || (x.Funcion.Action != null && x.Funcion.Action.Equals(action, strComparison)))).ToList();
            //  && ((x.Funcion.OperadorId == null || isSuperAdmin) || (x.Funcion.OperadorId != null && x.Funcion.OperadorId == operadorId))).ToList();

            bool isAuthorized = false;

            if (rolesAction.Count > 0)
            {
                //Guid? operadorIdFunction = rolesAction.First().Funcion.OperadorId;
                var op = rolesAction.First().Funcion.FuncionOperador.Where(x => x.OperadorId == operadorId).FirstOrDefault();
                Guid? operadorIdFunction = rolesAction.First().Funcion.PorOperador && rolesAction.First().Funcion.FuncionOperador.Count() != 0? (op != null ? op.OperadorId : Guid.Empty) : Guid.Empty;

                if (operadorIdFunction != Guid.Empty)
                {
                    isAuthorized = isSuperAdmin || operadorIdFunction.Value == operadorId;
                }
                else
                {
                    isAuthorized = rolesAction.Any(x => httpContext.User.IsInRole(x.Rol.Name));
                }
            }
            else if (rolesController.Count > 0)
            {
                isAuthorized = rolesController.Any(x => httpContext.User.IsInRole(x.Rol.Name));
            }
            else
            {
                isAuthorized = false;
            }

            return isAuthorized && base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            filterContext.Result = new RedirectToRouteResult(
                                       new RouteValueDictionary 
                                   {
                                       { "action", "AccesoDenegado" },
                                       { "controller", "Account" }
                                   });
        }

        private IList<FuncionRol> GetFuncionesRoles(HttpContextBase httpContext)
        {
            if (httpContext.Session["FuncionesRoles"] == null)
            {
                httpContext.Session["FuncionesRoles"] = db.FuncionRoles.ToList();
            }
            return (IList<FuncionRol>)httpContext.Session["FuncionesRoles"];
        }
    }
}