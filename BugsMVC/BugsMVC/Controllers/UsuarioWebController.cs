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
using System.Web.Security;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using ReportManagement;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using BugsMVC.Handlers;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using System.Linq.Dynamic;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using BugsMVC.Security;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class UsuarioWebController : PdfViewController
    {
        private BugsContext db = new BugsContext();

        public List<Usuario> UsuariosCargaMasiva = new List<Usuario>();

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

        // GET: Usuario
        [AuthorizeUser(accion = "Index", controlador = "Usuario")]
        public ActionResult Index()
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

        public JsonResult GetAllUsers()
        {
            var operadorID = GetUserOperadorID();

            ApplicationUser currentUser = ViewHelper.GetCurrentUser();
            bool isAdmin = currentUser.Roles.Any(x => x.RoleId == SecurityRoles.Admin);                   

            var query =
               from usuario in db.Usuarios
               join appUser in db.Users on usuario.UsuarioID equals appUser.UsuarioID
               join rol in db.Roles on appUser.Roles.FirstOrDefault().RoleId equals rol.Id
               where rol.Name != "Consumidor" && (!isAdmin || (isAdmin && rol.Name != "Operador")) 
               select usuario;

            var consumidores = query.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID
            ).ToList()
                                           .Select(x => new
                                           {
                                               UsuarioID = x.UsuarioID,
                                               OperadorNombre = x.OperadorID.HasValue? x.Operador.Nombre:string.Empty,
                                               Numero = x.Numero,
                                               Nombre = x.Nombre,
                                               Apellido = x.Apellido,
                                               ClaveTerminal = x.ClaveTerminal,
                                               Email = x.ApplicationUsers.First().Email,
                                               Roles = String.Join(",", (from Rol in x.ApplicationUsers.First().Roles
                                                                         join j in db.Roles
                                                                             on Rol.RoleId equals j.Id
                                                                         select j.Name).Select(y => y))
                                           });


            return Json(consumidores.Where(x=> ViewHelper.GetNameCurrentUserRolSuperior() == "SuperAdmin" || !x.Roles.Contains(ViewHelper.GetNameCurrentUserRolSuperior()) || x.UsuarioID== currentUser.UsuarioID).ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: Consumidor
        [AuthorizeUser(accion = "Reporte", controlador = "Usuario")]
        public ActionResult Reporte()
        {
            var operadorID = GetUserOperadorID();

            if (HttpContext.Session["UsuariosCargaMasiva"] != null)
            {
                var consumidores = (List<Usuario>)HttpContext.Session["UsuariosCargaMasiva"];
                return View(consumidores.ToList());
            }
            else
            {
                var consumidores = db.Users.Where(x => operadorID == Guid.Empty || x.Usuario.OperadorID == operadorID);
                return View(consumidores.ToList());
            }
        }

        // GET: Consumidor
        public ActionResult ReportePDF(string jqGridPostDataPdf)
        {
            string fixedPostData = jqGridPostDataPdf.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            JQGridPostData postData = JsonConvert.DeserializeObject<JQGridPostData>(fixedPostData);

            string filters = "true";

            if (postData.filters != null)
            {
                for (int i = 0; i < postData.filters.rules.Count; i++)
                {
                    string col = postData.filters.rules[i].field;
                    string data = postData.filters.rules[i].data.ToLower();
                    if (i > 0) filters += " && ";
                    else filters = string.Empty;
                    filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";
                }
            }

            var operadorID = GetUserOperadorID();
            var consumidores = GetUsuariosWebQuery().Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).Where(filters);
            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();

            foreach (var item in consumidores)
            {
                var viewModel = UsuarioToViewModel(item);
                ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);
                if (user != null)
                {
                    IList<string> roles = UserManager.GetRoles(user.Id);
                    viewModel.RolesSeleccionados = roles.ToArray();
                    viewModel.ApplicationUser = new RegisterViewModel();
                    viewModel.ApplicationUser.Email = user.Email;
                }
                lista.Add(viewModel);
            }

            return this.ViewPdf("", "ReportePDF", lista);
        }


        private IQueryable<Usuario> GetUsuariosWebQuery()
        {
            var usuariosConsumidores =
              from usuario in db.Usuarios
              join appUser in db.Users on usuario.UsuarioID equals appUser.UsuarioID
              join rol in db.Roles on appUser.Roles.FirstOrDefault().RoleId equals rol.Id
              where rol.Name != "Consumidor"
              select usuario;

            return usuariosConsumidores;
        }

        [Audit]
        public ActionResult ExportData(string jqGridPostDataExcel)
        {
            string fixedPostData = jqGridPostDataExcel.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            JQGridPostData postData = JsonConvert.DeserializeObject<JQGridPostData>(fixedPostData);

            string filters = "true";

            if (postData.filters != null)
            {
                for (int i = 0; i < postData.filters.rules.Count; i++)
                {
                    string col = postData.filters.rules[i].field;
                    string data = postData.filters.rules[i].data.ToLower();
                    if (i > 0) filters += " && ";
                    else filters = string.Empty;
                    filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";
                }
            }

            var operadorID = GetUserOperadorID();
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");
            var query = GetUsuariosWebQuery();

            var consumidores = query.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).ToList()
                                           .Select(x => new
                                           {
                                               OperadorNombre = x.OperadorID.HasValue ? x.Operador.Nombre : string.Empty,
                                               Email = x.ApplicationUsers.First().Email,
                                               Roles = String.Join(",", (from Rol in x.ApplicationUsers.First().Roles
                                                                         join j in db.Roles
                                                                             on Rol.RoleId equals j.Id
                                                                         select j.Name).Select(y => y)),
                                                Nombre = x.Nombre,
                                                Apellido = x.Apellido
                                           }).Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Usuarios Web");
            int amountOfColumns = 0;
            IRow headerRow = sheet.CreateRow(0);

            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Email");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Roles");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Apellido");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var consumidor in consumidores.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(consumidor.OperadorNombre);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Email);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Roles);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Nombre);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Apellido);

                for (int j = 0; j < colIdx; j++)
                {
                    row.Cells[j].CellStyle = defaultCellStyle;
                }
            }

            HSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                sheet.AutoSizeColumn(i);
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1 * 256);
            }

            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Usuarios Web " + DateTime.Now.ToString("dd-MM-yyyy HHmmss") + ".xlsx");
        }

        [AuthorizeUser(accion = "CargaMasiva", controlador = "Usuario")]
        public ActionResult CargaMasiva()
        {
            var operadorID = GetUserOperadorID();

            var operadores = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "OperadorID", "Nombre");
            ViewBag.OperadorID = AddDefaultOption(operadores, "Seleccionar Operador", "");

            ViewBag.OperadorID = GetUserOperadorID();

            ViewBag.LocacionID = new SelectList(string.Empty, "LocacionID", "Nombre");
            ViewBag.JerarquiaID = new SelectList(string.Empty, "JerarquiaID", "Nombre");
            return View();
        }

        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CargaMasiva(int? cantidadConsumidores, DateTime? FechaVencimiento, [Bind(Include = "FechaVencimiento,Efectivo,JerarquiaID,OperadorID,LocacionID")] Usuario usuario)
        {
            //TODO: Hacer un viewmodel para que las validaciones esten en el modelo
            if (cantidadConsumidores == null)
            {
                ModelState.AddModelError("cantidadConsumidores", "El campo Cantidad de Consumidores es obligatorio");
            }

            if (usuario.FechaVencimiento == null)
            {
                ModelState.AddModelError("FechaVencimiento", "El campo Fecha de Vencimiento es obligatorio");
            }

            ValidarCombos(usuario);

            ModelState.Remove("Dni");
            ModelState.Remove("Numero");
            ModelState.Remove("ClaveTerminal");

            usuario.Dni = 1;
            usuario.Numero = 1;
            usuario.ClaveTerminal = 1;

            try
            {
                if (ModelState.IsValid)
                {
                    var maxNumero = 0;

                    if (db.Usuarios.Where(x => x.LocacionID == usuario.LocacionID).Count() > 0)
                    {
                        maxNumero = db.Usuarios.Where(x => x.LocacionID == usuario.LocacionID).Max(x => x.Numero);
                    }

                    Random random = new Random();
                    UsuariosCargaMasiva = new List<Usuario>();

                    var jerarquia = db.Jerarquias.Find(usuario.JerarquiaID);
                    var locacion = db.Locaciones.Find(usuario.LocacionID);
                    var operador = db.Operadores.Find(usuario.OperadorID);

                    while (cantidadConsumidores > 0)
                    {
                        string prefijoUsuario = "Usuario-";
                        maxNumero = maxNumero + 1;

                        Usuario nuevoUsuario = new Usuario();
                        nuevoUsuario.UsuarioID = Guid.NewGuid();
                        nuevoUsuario.Efectivo = usuario.Efectivo;
                        nuevoUsuario.Jerarquia = jerarquia;
                        nuevoUsuario.JerarquiaID = usuario.JerarquiaID;
                        nuevoUsuario.Numero = maxNumero;
                        nuevoUsuario.Nombre = prefijoUsuario + maxNumero.ToString();
                        nuevoUsuario.Dni = 0;
                        nuevoUsuario.ClaveTerminal = random.Next(1000, 9999);
                        nuevoUsuario.Locacion = locacion;
                        nuevoUsuario.LocacionID = usuario.LocacionID;
                        nuevoUsuario.Operador = operador;
                        nuevoUsuario.OperadorID = usuario.OperadorID;
                        nuevoUsuario.FechaVencimiento = usuario.FechaVencimiento;
                        nuevoUsuario.FechaCreacion = DateTime.Now;
                        UsuariosCargaMasiva.Add(nuevoUsuario);
                        db.Usuarios.Add(nuevoUsuario);
                        db.SaveChanges();
                        var user = new ApplicationUser
                        {
                            UserName = prefijoUsuario + nuevoUsuario.Operador.Numero + nuevoUsuario.Locacion.Numero + maxNumero.ToString(),
                            Email = prefijoUsuario + nuevoUsuario.Operador.Numero + nuevoUsuario.Locacion.Numero + maxNumero.ToString() + "@user.com",
                            UsuarioID = nuevoUsuario.UsuarioID
                        };
                        var result = UserManager.Create(user, "RandomPass123");

                        if (result.Succeeded)
                        {
                            UserManager.AddToRoles(user.Id, "Consumidor");
                        }

                        db.SaveChanges();

                        cantidadConsumidores = cantidadConsumidores - 1;
                    }

                    HttpContext.Session["UsuariosCargaMasiva"] = UsuariosCargaMasiva;
                    return RedirectToAction("Reporte");
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }

            var operadorID = GetUserOperadorID();

            if (usuario.OperadorID == Guid.Empty)
            {
                var operadores = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                    "OperadorID", "Nombre");
                ViewBag.OperadorID = AddDefaultOption(operadores, "Seleccionar Operador", "");
                ViewBag.LocacionID = new SelectList(string.Empty, "LocacionID", "Nombre");
                ViewBag.JerarquiaID = new SelectList(string.Empty, "JerarquiaID", "Nombre");
            }
            else
            {
                ViewBag.OperadorID = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                    "OperadorID", "Nombre", usuario.OperadorID);
                ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == usuario.OperadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre", usuario.LocacionID);
                ViewBag.JerarquiaID = new SelectList(db.Jerarquias.Where(x => x.LocacionID == usuario.LocacionID).OrderBy(x => x.Nombre), "JerarquiaID", "Nombre", usuario.JerarquiaID);
            }

            ViewBag.OperadorID = GetUserOperadorID();

            return View(usuario);
        }

        // GET: Usuario/Details/5
        [AuthorizeUser(accion = "Detalles", controlador = "Usuario")]
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario consumidor = db.Usuarios.Find(id);

            UsuarioViewModel viewModel = null;

            if (consumidor == null)
            {
                return HttpNotFound();
            }
            else
            {
                viewModel = UsuarioToViewModel(consumidor);
                ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);
                if (user != null)
                {
                    IList<string> roles = UserManager.GetRoles(user.Id);
                    viewModel.RolesSeleccionados = roles.ToArray();
                    viewModel.ApplicationUser = new RegisterViewModel();
                    viewModel.ApplicationUser.Email = user.Email;
                }
            }
            return View(viewModel);
        }

        // GET: Usuario/Create
        [AuthorizeUser(accion = "Crear", controlador = "Usuario")]
        public ActionResult Create()
        {
            var operadorID = GetUserOperadorID();

            var operadores = new SelectList(db.Operadores.
                Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "OperadorID", "Nombre");

            if (User.Identity.IsAuthenticated && (User.IsInRole("SuperAdmin")))
            {
                ViewBag.OperadorID = AddDefaultOption(operadores, "Seleccionar Operador", "");
            }
            else
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ViewBag.OperadorID = user.Usuario.OperadorID;
            }

            ViewBag.operadorID = operadorID;

            return View(new UsuarioViewModel());
        }

        private IEnumerable<SelectListItem> AddDefaultOption(IEnumerable<SelectListItem> list, string dataTextField, string selectedValue)
        {
            var items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = dataTextField, Value = selectedValue });
            items.AddRange(list);
            return items;
        }

        // POST: Usuario/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UsuarioID,Apellido,Nombre,Legajo,Dni,Numero,ClaveTerminal,FechaVencimiento,Inhibido,FechaInhibido,UltimoUsoVT,Efectivo,CreditoZona1,CreditoZona2,CreditoZona3,CreditoZona4,CreditoZona5,Credit,UltimaRecargaZona1,UltimaRecargaZona2,UltimaRecargaZona3,UltimaRecargaZona4,UltimaRecargaZona5,OperadorID,LocacionID,JerarquiaID,ApplicationUser,RolesSeleccionados")] UsuarioViewModel consumidor)
        {
            ModelState.Remove("Dni");
            ModelState.Remove("Numero");
            ModelState.Remove("ClaveTerminal");

            Usuario usuario = UsuarioVMToEntity(consumidor);

            var operadorID = GetUserOperadorID();

            if (operadorID == Guid.Empty && (consumidor.RolesSeleccionados == null || consumidor.RolesSeleccionados.Count() == 1 && consumidor.RolesSeleccionados.FirstOrDefault(x => x == "SuperAdmin") == null))
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }
            
            var usuarioExiste = db.Users.Any(x => x.Email == consumidor.ApplicationUser.Email);
            if (usuarioExiste)
            {
                ModelState.AddModelError(string.Empty, "Ya existe un usuario con ese Email.");
            }

            if (consumidor.RolesSeleccionados == null)
            {
                ModelState.AddModelError(string.Empty, "Al menos debe seleccionar un Rol.");
            }
            //ValidarCombos(usuario);

            if (ModelState.IsValid)
            {
                usuario.UsuarioID = Guid.NewGuid();
                usuario.OperadorID = operadorID;
                usuario.FechaCreacion = DateTime.Now;

                if (operadorID == Guid.Empty) usuario.OperadorID = null;

                db.Usuarios.Add(usuario);
                db.SaveChanges();

                if (!string.IsNullOrWhiteSpace(consumidor.ApplicationUser.Email))
                {
                    var user = new ApplicationUser
                    {
                        UserName = consumidor.ApplicationUser.Email,
                        Email = consumidor.ApplicationUser.Email,
                        UsuarioID = usuario.UsuarioID
                    };
                    var result = UserManager.Create(user, consumidor.ApplicationUser.Password);

                    if (result.Succeeded)
                    {
                        if (consumidor.RolesSeleccionados != null)
                        {
                            UserManager.AddToRoles(user.Id, consumidor.RolesSeleccionados);
                        }

                        return RedirectToAction("Index");
                    }
                    AddErrors(result);
                }
                return RedirectToAction("Index");
            }

            if (consumidor.OperadorID == Guid.Empty)
            {
                var operadores = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                    "OperadorID", "Nombre");
                ViewBag.OperadorID = AddDefaultOption(operadores, "Seleccionar Operador", "");
            }
            else
            {
                ViewBag.OperadorID = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                    "OperadorID", "Nombre", consumidor.OperadorID);
            }

            ViewBag.operadorID = operadorID;
            return View(consumidor);
        }

        private void RemoveValidationIfIsNull(UsuarioViewModel consumidor)
        {
            if (consumidor.ApplicationUser != null || string.IsNullOrWhiteSpace(consumidor.ApplicationUser.Email) &&
                string.IsNullOrWhiteSpace(consumidor.ApplicationUser.Password))
            {
                ModelState.Remove("ApplicationUser.Email");
                ModelState.Remove("ApplicationUser.Password");
            }
        }

        private static Usuario UsuarioVMToEntity(UsuarioViewModel viewModel)
        {
            Usuario entity = new Usuario();
            entity.Apellido = viewModel.Apellido;
            entity.ClaveTerminal = viewModel.ClaveTerminal;
            entity.CreditoZona1 = viewModel.CreditoZona1;
            entity.CreditoZona2 = viewModel.CreditoZona2;
            entity.CreditoZona3 = viewModel.CreditoZona3;
            entity.CreditoZona4 = viewModel.CreditoZona4;
            entity.CreditoZona5 = viewModel.CreditoZona5;
            entity.Dni = viewModel.Dni;
            entity.Efectivo = viewModel.Efectivo;
            entity.FechaInhibido = viewModel.FechaInhibido;
            entity.FechaVencimiento = viewModel.FechaVencimiento;
            entity.Inhibido = viewModel.Inhibido;
            entity.Jerarquia = viewModel.Jerarquia;
            entity.JerarquiaID = viewModel.JerarquiaID;
            entity.Legajo = viewModel.Legajo;
            entity.Locacion = viewModel.Locacion;
            entity.LocacionID = viewModel.LocacionID;
            entity.Nombre = viewModel.Nombre;
            entity.Numero = viewModel.Numero;
            entity.Operador = viewModel.Operador;
            entity.OperadorID = viewModel.OperadorID;
            entity.Transacciones = viewModel.Transacciones;

            entity.UltimaRecargaZona1 = viewModel.UltimaRecargaZona1;
            entity.UltimaRecargaZona2 = viewModel.UltimaRecargaZona2;
            entity.UltimaRecargaZona3 = viewModel.UltimaRecargaZona3;
            entity.UltimaRecargaZona4 = viewModel.UltimaRecargaZona4;
            entity.UltimaRecargaZona5 = viewModel.UltimaRecargaZona5;
            
            entity.UltimoUsoVT = viewModel.UltimoUsoVT;
            entity.UsuarioID = viewModel.UsuarioID;
            return entity;
        }

        private static UsuarioViewModel UsuarioToViewModel(Usuario entity)
        {
            UsuarioViewModel viewModel = new UsuarioViewModel();
            viewModel.Apellido = entity.Apellido;
            viewModel.ClaveTerminal = entity.ClaveTerminal;
            viewModel.CreditoZona1 = entity.CreditoZona1;
            viewModel.CreditoZona2 = entity.CreditoZona2;
            viewModel.CreditoZona3 = entity.CreditoZona3;
            viewModel.CreditoZona4 = entity.CreditoZona4;
            viewModel.CreditoZona5 = entity.CreditoZona5;
            viewModel.Dni = entity.Dni;
            viewModel.Efectivo = entity.Efectivo;
            viewModel.FechaInhibido = entity.FechaInhibido;
            viewModel.FechaVencimiento = entity.FechaVencimiento;
            viewModel.Inhibido = entity.Inhibido;
            viewModel.Jerarquia = entity.Jerarquia;
            viewModel.JerarquiaID = entity.JerarquiaID;
            viewModel.Legajo = entity.Legajo;
            viewModel.Locacion = entity.Locacion;
            viewModel.LocacionID = entity.LocacionID;
            viewModel.Nombre = entity.Nombre;
            viewModel.Numero = entity.Numero;
            viewModel.Operador = entity.Operador;
            viewModel.OperadorID = entity.OperadorID;
            viewModel.Transacciones = entity.Transacciones;
            viewModel.UltimaRecargaZona1 = entity.UltimaRecargaZona1;
            viewModel.UltimaRecargaZona2 = entity.UltimaRecargaZona2;
            viewModel.UltimaRecargaZona3 = entity.UltimaRecargaZona3;
            viewModel.UltimaRecargaZona4 = entity.UltimaRecargaZona4;
            viewModel.UltimaRecargaZona5 = entity.UltimaRecargaZona5;
            viewModel.UltimoUsoVT = entity.UltimoUsoVT;
            viewModel.UsuarioID = entity.UsuarioID;
            return viewModel;
        }

        // GET: Usuario/Edit/5
        [AuthorizeUser(accion = "Editar", controlador = "Usuario")]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario consumidor = db.Usuarios.Find(id);

            UsuarioViewModel viewModel = null;

            if (consumidor == null)
            {
                return HttpNotFound();
            }
            else
            {
                viewModel = UsuarioToViewModel(consumidor);
                ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);
                if (user != null)
                {
                    IList<string> roles = UserManager.GetRoles(user.Id);
                    viewModel.RolesSeleccionados = roles.ToArray();
                    viewModel.ApplicationUser = new RegisterViewModel();
                    viewModel.ApplicationUser.Email = user.Email;
                }
            }

            ViewBag.operadorID = GetUserOperadorID();

            return View(viewModel);
        }

        // POST: Usuario/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UsuarioID,Apellido,Nombre,Legajo,Dni,Numero,ClaveTerminal,FechaVencimiento,Inhibido,FechaInhibido,UltimoUsoVT,Efectivo,CreditoZona1,CreditoZona2,CreditoZona3,CreditoZona4,CreditoZona5,UltimaRecargaZona1,UltimaRecargaZona2,UltimaRecargaZona3,UltimaRecargaZona4,UltimaRecargaZona5,OperadorID,LocacionID,JerarquiaID,ApplicationUser,RolesSeleccionados")] UsuarioViewModel viewModel)
        {
            //Buscamos si el usuario existe en aspnetusers
            ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);

            if (user != null && viewModel.ApplicationUser != null &&
                string.IsNullOrEmpty(viewModel.ApplicationUser.Password) &&
                string.IsNullOrEmpty(viewModel.ApplicationUser.ConfirmPassword))
            {
                ModelState.Remove("ApplicationUser.Password");
                ModelState.Remove("ApplicationUser.ConfirmPassword");
            }

            if (user != null && ViewHelper.GetCurrentUsuarioId() != user.UsuarioID && viewModel.RolesSeleccionados == null)
            {
                ModelState.AddModelError(string.Empty, "Al menos debe seleccionar un Rol.");
            }

            RemoveValidationIfIsNull(viewModel);
            ModelState.Remove("Dni");
            ModelState.Remove("Numero");
            ModelState.Remove("ClaveTerminal");

            if (ModelState.IsValid)
            {
                //El usuario existe en aspnetusers
                if (user != null)
                {
                    //db.Entry(user).State = EntityState.Modified;
                    user.UserName = viewModel.ApplicationUser.Email;
                    user.Email = viewModel.ApplicationUser.Email;
                    user.Usuario.Nombre = viewModel.Nombre;
                    user.Usuario.Apellido = viewModel.Apellido;

                    if (!string.IsNullOrEmpty(viewModel.ApplicationUser.Password) &&
                        !string.IsNullOrEmpty(viewModel.ApplicationUser.ConfirmPassword))
                    {
                        string token = UserManager.GeneratePasswordResetToken(user.Id);
                        UserManager.ResetPassword(user.Id, token, viewModel.ApplicationUser.Password);
                    }
                    if (viewModel.RolesSeleccionados != null)
                    {
                        foreach (var role in UserManager.GetRoles(user.Id))
                        {
                            UserManager.RemoveFromRoles(user.Id, role);
                        }

                        foreach (var rol in viewModel.RolesSeleccionados)
                        {
                            UserManager.AddToRole(user.Id, rol);
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(viewModel.ApplicationUser.Email))
                    {
                        user = new ApplicationUser
                        {
                            UserName = viewModel.ApplicationUser.Email,
                            Email = viewModel.ApplicationUser.Email,
                            UsuarioID = viewModel.UsuarioID
                        };
                        var result = UserManager.Create(user, viewModel.ApplicationUser.Password);

                        if (result.Succeeded)
                        {
                            if (viewModel.RolesSeleccionados != null)
                            {
                                UserManager.AddToRoles(user.Id, viewModel.RolesSeleccionados);
                            }
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        AddErrors(result);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.operadorID = GetUserOperadorID();

            return View(viewModel);
        }

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario consumidor = db.Usuarios.Find(id);
            if (consumidor == null)
            {
                return HttpNotFound();
            }
            UsuarioViewModel viewModel = UsuarioToViewModel(consumidor);
            ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);
            if (user != null)
            {
                IList<string> roles = UserManager.GetRoles(user.Id);
                viewModel.RolesSeleccionados = roles.ToArray();
                viewModel.ApplicationUser = new RegisterViewModel();
                viewModel.ApplicationUser.Email = user.Email;
            }
            return View(viewModel);
        }

        // POST: Usuario/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();
            Usuario consumidor = db.Usuarios.Find(id);

            var users = db.Users.Where(x => x.Usuario.UsuarioID == consumidor.UsuarioID);
            
            var stocks = db.Stocks.Where(x => x.UsuarioEdicionWeb.UsuarioID==consumidor.UsuarioID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => x.UsuarioID== consumidor.UsuarioID);

            foreach (var stock in stocks)
            {
                stock.UsuarioEdicionWeb = null;
                stock.UsuarioIDEdicionWeb = null;
            }
            
            //Se pone null el usuario de historico, esto en realidad deberia ser una baja logica
            foreach (var stockHistorico in stocksHistoricos)
            {
                stockHistorico.UsuarioID = null;
            }

            var appUser = db.Users.SingleOrDefault(x => x.UsuarioID == consumidor.UsuarioID);

            if (appUser != null)
            {
                foreach (var role in UserManager.GetRoles(appUser.Id))
                {
                    UserManager.RemoveFromRoles(appUser.Id, role);
                }

                UserManager.Delete(appUser);
            }

            List<Transaccion> transaccionList = new List<Transaccion>();

            foreach (var transaccion in consumidor.Transacciones)
            {
                transaccionList.Add(transaccion);
            }

            foreach (var transaccion in transaccionList)
            {
                consumidor.Transacciones.Remove(transaccion);
                db.Entry(transaccion).State = EntityState.Deleted;
            }

            var alarmaConfiguracionDetalle = db.AlarmaConfiguracionDetalle.Where(x => x.UsuarioID == consumidor.UsuarioID);
            alarmaConfiguracionDetalle.ToList().ForEach(x => db.AlarmaConfiguracionDetalle.Remove(x));

            db.Usuarios.Remove(consumidor);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public JsonResult GetUsuariosByLocacionSelectList(string locacionID)
        {
            Guid locacionGuid = (!string.IsNullOrEmpty(locacionID)) ? new Guid(locacionID) : Guid.Empty;
            var ret = db.Usuarios.Where(x => x.LocacionID == locacionGuid).Select(x => new SelectListItem()
            {
                Text = (x.Nombre != null) ? x.Apellido + ((!string.IsNullOrEmpty(x.Apellido) && !string.IsNullOrEmpty(x.Nombre)) ? ", " : "") + x.Nombre : "Consumidor " + x.Numero,
                Value = x.UsuarioID.ToString()
            }).OrderBy(x => x.Text).ToList();
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUsuariosByLocacionMultiSelectList(string locacionID)
        {
            Guid locacionGuid = (!string.IsNullOrEmpty(locacionID)) ? new Guid(locacionID) : Guid.Empty;
            var ret = new MultiSelectList(db.Usuarios.Where(x => x.LocacionID == locacionGuid).Select(x => new SelectListItem()
            {
                Text = (x.Nombre != null) ? x.Apellido + ((!string.IsNullOrEmpty(x.Apellido) && !string.IsNullOrEmpty(x.Nombre)) ? ", " : "") + x.Nombre : "Consumidor " + x.Numero,
                Value = x.UsuarioID.ToString()
            }).OrderBy(x => x.Text).ToList(), "Value", "Text");
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        private void ValidarCombos(Usuario usuario)
        {


            if (usuario.LocacionID == null)
            {
                ModelState.AddModelError("LocacionID", "Debe elegir una locación");
            }

            if (usuario.JerarquiaID == null)
            {
                ModelState.AddModelError("JerarquiaID", "Debe elegir una jerarquía");
            }
        }

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
