using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugsMVC.DAL;
using BugsMVC.Models;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using BugsMVC.Handlers;
using BugsMVC.Security;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class OperadorController : BaseController
    {
        private BugsContext db = new BugsContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Operador
        public ActionResult Index()
        {
            return View(db.Operadores.ToList());
        }

        // GET: Operador/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operador operador = db.Operadores.Find(id);
            if (operador == null)
            {
                return HttpNotFound();
            }
            return View(operador);
        }

        // GET: Operador/Create
        public ActionResult Create()
        {
            OperadorViewModel viewModel = new OperadorViewModel();
            viewModel.IsCreate = true;

            return View(viewModel);
        }

        // POST: Operador/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OperadorID,Nombre,Numero,ApplicationUser,TiempoAvisoInhibicion,TiempoAvisoConexion,ClientId,SecretToken,IsCreate")] OperadorViewModel viewModel)
        {
            var existe = db.Operadores.Any(x => x.Nombre == viewModel.Nombre);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre seleccionado ya existe.");
            }

            if (ModelState.IsValid)
            {
                //Esto se comenta, porque siempre esta poniendo NULL, ya que el unico usuario que puede crear operadores es el super admin
                //var usuarioActualEsOperador = (User.IsInRole("Operador") || User.IsInRole("Admin")) && !User.IsInRole("SuperAdmin");

                viewModel.OperadorID = Guid.NewGuid();
                //viewModel.OperadorAdminID = null;//usuarioActualEsOperador ? GetUserOperadorID() : (Guid?)null;

                Operador entity = new Operador();
                viewModel.ToEntity(entity);

                db.Operadores.Add(entity);
                db.SaveChanges();

                if (!string.IsNullOrWhiteSpace(viewModel.ApplicationUser.Email))
                {
                    Random random = new Random();

                    Usuario usuario = new Usuario();

                    usuario.UsuarioID = Guid.NewGuid();
                    usuario.Efectivo = 0;
                    usuario.Legajo = 1.ToString();
                    usuario.Numero = 1;
                    usuario.Nombre = entity.Nombre.Length > 14 ? entity.Nombre.Substring(0, 14) : entity.Nombre;
                    usuario.Apellido = entity.Nombre.Length > 14 ? entity.Nombre.Substring(0, 14) : entity.Nombre;
                    usuario.Dni = 1;
                    usuario.ClaveTerminal = random.Next(1000, 9999);
                    usuario.OperadorID = entity.OperadorID;
                    usuario.FechaVencimiento = DateTime.Now.AddMonths(6);
                    usuario.FechaCreacion = DateTime.Now;

                    db.Usuarios.Add(usuario);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            foreach (var ve in eve.ValidationErrors)
                            {
                                var i = ve.ErrorMessage;
                            }
                        }
                        throw;
                    }

                    var user = new ApplicationUser
                    {
                        UserName = viewModel.ApplicationUser.Email,
                        Email = viewModel.ApplicationUser.Email,
                        UsuarioID = usuario.UsuarioID
                    };

                    var result = UserManager.Create(user, viewModel.ApplicationUser.Password);

                    if (result.Succeeded)
                    {
                        UserManager.AddToRoles(user.Id, "Operador");

                        return RedirectToAction("Index");
                    }
                    AddErrors(result);
                }
                return RedirectToAction("Index");

            }

            return View(viewModel);
        }

        // GET: Operador/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Operador operador = db.Operadores.Find(id);

            if (operador == null)
            {
                return HttpNotFound();
            }

            OperadorViewModel viewModel = OperadorViewModel.From(operador);
            viewModel.IsCreate = false;

            return View(viewModel);
        }

        // POST: Operador/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OperadorID,Nombre,Numero,ApplicationUser,TiempoAvisoInhibicion,TiempoAvisoConexion,ClientId,SecretToken,IsCreate")] OperadorViewModel viewModel)
        {
            var existe = db.Operadores.Any(x => x.Nombre == viewModel.Nombre && x.OperadorID != viewModel.OperadorID);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre seleccionado ya existe.");
            }

            Operador operador = db.Operadores.Find(viewModel.OperadorID);

            if (ModelState.IsValid)
            {
                viewModel.ToEntity(operador);

                db.Entry(operador).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(viewModel);
        }

        // GET: Operador/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operador operador = db.Operadores.Find(id);
            if (operador == null)
            {
                return HttpNotFound();
            }
            return View(operador);
        }

        // POST: Operador/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();

            Operador operador = db.Operadores.Find(id);
            var maquinas = operador.Maquinas;
            var maquinasID = maquinas.Select(x => x.MaquinaID);
            var mercadoPagoRegs = db.MercadoPago.Where(x => maquinasID.Contains(x.MaquinaId));
            var locaciones = operador.Locaciones.Select(x => x.LocacionID);
            var articulos = db.Articulos.Where(x => x.OperadorID == operador.OperadorID);
            var articulosID = articulos.Select(x => x.ArticuloID);
            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => articulosID.Contains(x.ArticuloID) || locaciones.Contains(x.LocacionID));
            var articulosAsignacionesID = articulosAsignaciones.Select(x => x.Id);
            var transacciones = db.Transacciones.Where(x => x.OperadorID == operador.OperadorID);
            var terminales = db.Terminales.Where(x => x.OperadorID == operador.OperadorID);
            var jerarquias = db.Jerarquias.Where(x => locaciones.Contains(x.LocacionID));
            var jerarquiasID = jerarquias.Select(x => x.JerarquiaID);
            var usuarios = db.Usuarios.Where(x => x.OperadorID == operador.OperadorID);
            var users = db.Users.Where(x => x.Usuario.OperadorID == operador.OperadorID);
            var stocks = db.Stocks.Where(x => articulosAsignacionesID.Contains(x.ArticuloAsignacionID));
            var stocksID = stocks.Select(x => x.StockID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => x.StockID.HasValue && stocksID.Contains(x.StockID.Value));
            var zonas = db.Zonas.Where(x => jerarquiasID.Contains(x.JerarquiaID));
            //List<ApplicationUser> usuariosEliminar = new List<ApplicationUser>();
            var alarmaConfiguracion = db.AlarmaConfiguracion.Where(x => x.OperadorID == operador.OperadorID || x.LocacionID.HasValue && locaciones.Contains(x.LocacionID.Value));
            var funcionesOperador = db.FuncionOperador.Where(x => x.OperadorId == id);

            var transaccionesMal = operador.TransaccionesMal;
            var tablesOfflines = db.TablasOfflines.Where(x => locaciones.Contains(x.LocacionID.Value));


            foreach (var user in users.ToList())
            {
                UserManager.RemoveFromRoles(user.Id, new string[] { "SuperAdmin", "Operador", "Consumidor", "Repositor", "Técnico", "Administrador", "Proveedor" });
                UserManager.Delete(user);
            }
            //Se desasignan
            terminales.ToList().ForEach(x => x.OperadorID = null);
            //Se eliminan
            alarmaConfiguracion.ToList().ForEach(x => db.AlarmaConfiguracionDetalle.RemoveRange(x.AlarmaConfiguracionDetalles));
            alarmaConfiguracion.ToList().ForEach(x => db.AlarmaConfiguracion.Remove(x));
            db.AlarmaConfiguracionDetalle.RemoveRange(db.AlarmaConfiguracionDetalle.Where(x => x.Usuario.OperadorID == id));

            db.StocksHistoricos.RemoveRange(stocksHistoricos);
            db.Stocks.RemoveRange(stocks);
            db.ArticulosAsignaciones.RemoveRange(articulosAsignaciones);
            db.Transacciones.RemoveRange(transacciones);
            db.MercadoPago.RemoveRange(mercadoPagoRegs);
            db.Zonas.RemoveRange(zonas);
            db.Usuarios.RemoveRange(usuarios);
            db.Jerarquias.RemoveRange(jerarquias);//transaccines tiene valores pero jerarquias no mmhhh...
            db.Articulos.RemoveRange(articulos);
            db.FuncionOperador.RemoveRange(funcionesOperador);
            db.TablasOfflines.RemoveRange(tablesOfflines);
            db.Locaciones.RemoveRange(operador.Locaciones);
            db.TransaccionesMal.RemoveRange(operador.TransaccionesMal);
            db.Maquinas.RemoveRange(operador.Maquinas);

            //db.Terminales.RemoveRange(terminales);
            db.Operadores.Remove(operador);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        #region Aux Methods

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

        public JsonResult SetOperadorID(string operadorID)
        {
            HttpContext.Session["AdminOperadorID"] = operadorID;
            return Json(operadorID, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllOperadores(bool forSelector = false)
        {
            Guid selectedID = (!String.IsNullOrEmpty((string)HttpContext.Session["AdminOperadorID"])) ? new Guid((string)HttpContext.Session["AdminOperadorID"]) : Guid.Empty;

            IQueryable<Operador> operadores;
            Guid usuarioActualOperadorId = GetUserOperadorID();
            // Si es SuperAdmin y no es para el combo del header, mostrar todo
            // Si es SuperAdmin y es para el combo mostrar operadores tipo SuperAdmin
            // Si no es SuperAdmin mostrar el operador actual y sus dependientes
            if (User.IsInRole("SuperAdmin"))
            {
                operadores = db.Operadores;
            }
            else
            {
                operadores = db.Operadores.Where(x => (x.OperadorID != usuarioActualOperadorId)
                    || (x.OperadorID == usuarioActualOperadorId));
            }

            var operadoresFiltrado = operadores.Select(x => new
            {
                OperadorID = x.OperadorID,
                Nombre = x.Nombre,
                Numero = x.Numero,
                Selected = (x.OperadorID == selectedID)
            });

            return Json(operadoresFiltrado.ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
