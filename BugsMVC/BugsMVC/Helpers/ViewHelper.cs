using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using BugsMVC.DAL;
using BugsMVC.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugsMVC
{
    public static class ViewHelper
    {
        public static string GetCurrentUserName()
        {
            BugsContext db = new BugsContext();
            string userId = HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

            if (currentUser != null)
            {
                return currentUser.Usuario.Nombre;
            }

            return HttpContext.Current.User.Identity.GetUserName();
        }

        public static string GetNameCurrentUserRolSuperior()
        {
            BugsContext db = new BugsContext();
            string userId = HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);
            string[] guids = currentUser.Roles.Select(x=>x.RoleId).ToArray();
            string nameRolSuperior = db.Roles.Where(x => guids.Contains(x.Id)).OrderBy(x=>x.Weight).First().Name; 

            return nameRolSuperior;
        }

        public static Guid GetCurrentOperadorId()
        {
            BugsContext db = new BugsContext();
            string userId = HttpContext.Current.User.Identity.GetUserId();
            var currentAppUser = db.Users.SingleOrDefault(x => x.Id == userId);
            var currentUser = db.Usuarios.Where(x => x.UsuarioID == currentAppUser.UsuarioID).FirstOrDefault();
            return currentUser.OperadorID?? new Guid();
        }

        public static Guid GetCurrentUsuarioId()
        {
            BugsContext db = new BugsContext();
            string userId = HttpContext.Current.User.Identity.GetUserId();
            var currentAppUser = db.Users.SingleOrDefault(x => x.Id == userId);
            var currentUser = db.Usuarios.Where(x => x.UsuarioID == currentAppUser.UsuarioID).FirstOrDefault();
            return currentUser.UsuarioID;
        }

        public static string GetCurrentOperadorName()
        {
            BugsContext db = new BugsContext();
            string userId = HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

            if (currentUser != null)
            {
                return db.Usuarios.SingleOrDefault(x => x.UsuarioID == currentUser.UsuarioID).Operador.Nombre;
            }

            return String.Empty;
        }

        public static ApplicationUser GetCurrentUser()
        {
            BugsContext db = new BugsContext();
            string userId = HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

            return currentUser;
        }

        public static string ToJsArray<T>(this IEnumerable<T> list, bool convertToString = false)
        {
            return GetJsArray(list, convertToString);
        }

        public static string GetJsArray<T>(IEnumerable<T> list, bool convertToString = false)
        {
            if (convertToString)
                return "[" + string.Join(",", list.Select(x => string.Format("'{0}'", x)).ToArray()) + "]";

            return "[" + string.Join(",", list.Select(x => ParseDecimal(x)).ToArray()) + "]";
        }

        public static string ParseDecimal(object value, int decimals = 2)
        {
            var result = value.ToString();
            if (value is double || value is decimal)
            {
                result = string.Format("{0:f" + decimals.ToString() + "}", value);
            }

            return result.Replace(",", ".");
        }
    }
}
