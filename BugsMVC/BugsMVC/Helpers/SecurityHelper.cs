using BugsMVC.DAL;
using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Helpers
{
    public static class SecurityHelper
    {
        private static List<FuncionRol> FuncionesRoles;
        private static List<Funcion> Funciones;

        public static void Initialize(List<FuncionRol> funcionesRoles, List<Funcion> funciones)
        {
            FuncionesRoles = funcionesRoles;
            Funciones = funciones;
        }

        public static bool IsAllowed(string acceptedRole)
        {
            return IsAllowed(new string[] { acceptedRole });
        }

        public static bool IsAllowed(string[] acceptedRoles)
        {
            bool result = false;
            foreach (var item in acceptedRoles)
            {
                if (HttpContext.Current.User.IsInRole(item)) return true;
            }
            return result;
        }

        public static bool IsAllowed(int idFuncion)
        {
            bool isSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");

            if (isSuperAdmin && idFuncion != (int)Permissions.Consumidor.Index && idFuncion != (int)Permissions.Consumidor.MiCuenta)            
                return true;            

            //var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;

            Funcion funcion = Funciones.First(x => x.Id == idFuncion);
            var controller = funcion.Controller;
            var action = funcion.Action;
            Guid operadorId = ViewHelper.GetCurrentOperadorId();

            var funcionesRoles = FuncionesRoles;
            //var rolesFuncion = funcionesRoles.Where(x => x.IdFuncion == idFuncion
            //    && ((x.Funcion.OperadorId == null || isSuperAdmin) || (x.Funcion.OperadorId != null && x.Funcion.OperadorId == operadorId))).ToList();
            var rolesFuncion = funcionesRoles.Where(x => x.IdFuncion == idFuncion
                && ((x.Funcion.PorOperador == false) || (x.Funcion.PorOperador == true && x.Funcion.FuncionOperador.Any(y=> y.OperadorId == operadorId) ))).ToList();

            bool isAuthorized = false;
            if (rolesFuncion.Count > 0)
            {
                isAuthorized = rolesFuncion.Any(x => HttpContext.Current.User.IsInRole(x.Rol.Name));
            }
            else
            {
                isAuthorized = false;
            }

            return isAuthorized;
        }

        public static bool IsInRole(string rol)
        {
            bool result = false;
            result = HttpContext.Current.User.IsInRole(rol);
            return result;
        }

        //private IList<FuncionRol> GetFuncionesRoles()
        //{
        //    return db.FuncionRoles.ToList();
        //    //if (HttpContext.Current.Session["FuncionesRoles"] == null)
        //    //{
        //    //    HttpContext.Current.Session["FuncionesRoles"] = db.FuncionRoles.ToList();
        //    //}
        //    //return (IList<FuncionRol>)HttpContext.Current.Session["FuncionesRoles"];
        //}
    }
}