using BugsMVC.Commands.Alarma;
using BugsMVC.DAL;
using BugsMVC.Models;
using BugsMVC.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugsMVC.Controllers
{
    public class AlarmaController : BaseController
    {
        private BugsContext db = new BugsContext();

        public ActionResult Create()
        {
            var operadorID = GetUserOperadorID();
            AlarmaViewModel viewModel = new AlarmaViewModel();

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(y => operadorID == Guid.Empty || y.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");
            ViewBag.Usuarios = new MultiSelectList(string.Empty, "UsuarioID", "Nombre");
            ViewBag.TipoDeAlarmaID = new SelectList(db.TipoDeAlarma.OrderBy(x => x.Descripcion), "TipoDeAlarmaID", "Descripcion");

            viewModel.OperadorID = operadorID;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TipoDeAlarmaID,LocacionID,Usuarios,OperadorID")] AlarmaViewModel viewModel)
        {
            var operadorID = GetUserOperadorID();
            viewModel.OperadorID = operadorID;
           // ViewBag.OperadorID = operadorID;

            if (viewModel.OperadorID == Guid.Empty)
            {
                ModelState.AddModelError("TipoDeAlarmaID", "Debe seleccionar un operador");
            }

            if (ModelState.IsValid)
            {                  
                List<Guid> usuarios = (viewModel.Usuarios != null) ? viewModel.Usuarios.Select(x => new Guid(x)).ToList() : new List<Guid>();               

                var command = new CreateAlarmaCommand();
                command.Configure(viewModel.TipoDeAlarmaID, viewModel.LocacionID,viewModel.OperadorID, usuarios, db).Execute();

                return RedirectToAction("CreateSuccess");
            }

            var query =
               from usuario in db.Usuarios
               join appUser in db.Users on usuario.UsuarioID equals appUser.UsuarioID
               join rol in db.Roles on appUser.Roles.FirstOrDefault().RoleId equals rol.Id
               join locacion in db.Locaciones on usuario.LocacionID equals locacion.LocacionID into nuevo
               from locacion in nuevo.DefaultIfEmpty()
               where rol.Name != "Consumidor"
               select new { usuario = usuario, LocacionID = locacion == null ? Guid.Empty : locacion.LocacionID };



            ViewBag.Usuarios = new MultiSelectList(query.Where(x=>x.usuario.OperadorID == Guid.Empty || x.usuario.OperadorID == operadorID).ToList().Select(x => new SelectListItem()
            {
                            Text = x.usuario.ApplicationUsers.FirstOrDefault().Email + '(' +
                 String.Join(",", (from Rol in x.usuario.ApplicationUsers.FirstOrDefault().Roles
                                   join j in db.Roles
                                       on Rol.RoleId equals j.Id
                                   select j.Name).Select(y => y))
                + ')',
                            Value = x.usuario.UsuarioID.ToString()
                        }).OrderBy(x => x.Text).ToList(), "Value", "Text");

            ViewBag.TipoDeAlarmaID = new SelectList(db.TipoDeAlarma.OrderBy(x => x.Descripcion), "TipoDeAlarmaID", "Descripcion", viewModel.TipoDeAlarmaID);
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(y=>operadorID == Guid.Empty || y.OperadorID==operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre",viewModel.LocacionID);


            return View("Create",viewModel);
        }

        public ActionResult CreateSuccess()
        {
            return View();
        }

        public Guid GetUserOperadorID()
        {
            string userId = User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

            Guid operadorID = Guid.Empty;
            if (User.IsInRole("SuperAdmin"))
            {
                operadorID = (!String.IsNullOrEmpty((string)HttpContext.Session["AdminOperadorID"])) ? new Guid((string)HttpContext.Session["AdminOperadorID"]) : Guid.Empty;
            }
            else
            {
                operadorID = (currentUser.Usuario != null && currentUser.Usuario.OperadorID.HasValue) ? currentUser.Usuario.OperadorID.Value : Guid.Empty;
            }

            return operadorID;
        }

        public JsonResult GetUsuariosByLocacionesMultiSelectList(int tipoDeAlarmaID, string[] locaciones)
        {
            List<Guid?> locacionesID = new List<Guid?>();
            Guid operadorID = GetUserOperadorID();

            if (locaciones[0] != string.Empty)
                locaciones.ToList().ForEach(item =>locacionesID.Add(new Guid(item)));
            else
            {
                locacionesID.Add(null);
            }

            var query =
               from usuario in db.Usuarios
               join appUser in db.Users on usuario.UsuarioID equals appUser.UsuarioID
               join rol in db.Roles on appUser.Roles.FirstOrDefault().RoleId equals rol.Id
               join locacion in db.Locaciones on usuario.LocacionID equals locacion.LocacionID into nuevo
               from locacion in nuevo.DefaultIfEmpty()
               where rol.Name != "Consumidor" && usuario.OperadorID.HasValue
               select new { usuario = usuario, LocacionID = locacion == null ? Guid.Empty : locacion.LocacionID };
            
            var valoresSeleccionados = db.AlarmaConfiguracionDetalle.Where(x =>(x.AlarmaConfiguracion.OperadorID == Guid.Empty || x.AlarmaConfiguracion.OperadorID == operadorID) && x.AlarmaConfiguracion.TipoDeAlarmaID == tipoDeAlarmaID 
                            && (locacionesID.Contains(x.AlarmaConfiguracion.LocacionID)))
                            .Select(x => x.UsuarioID.ToString()).ToArray();


            var ret = new MultiSelectList(query.Where(x => operadorID == Guid.Empty || !x.usuario.OperadorID.HasValue || x.usuario.OperadorID == operadorID).ToList().Select(x => new SelectListItem()
            {
                Text = x.usuario.ApplicationUsers.FirstOrDefault().Email + '(' +
                 String.Join(",", (from Rol in x.usuario.ApplicationUsers.FirstOrDefault().Roles
                                   join j in db.Roles
                                       on Rol.RoleId equals j.Id
                                   select j.Name).Select(y => y))
                + ')',
                Value = x.usuario.UsuarioID.ToString()
            }).OrderBy(x => x.Text).ToList(), "Value", "Text");

            return Json(new { usuarios = ret, selectValue = valoresSeleccionados }, JsonRequestBehavior.AllowGet);
        }


    }
}