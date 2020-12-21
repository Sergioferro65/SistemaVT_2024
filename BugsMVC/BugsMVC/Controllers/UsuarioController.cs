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
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using BugsMVC.Extensions;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using BugsMVC.Models.ViewModels.Grids;
using BugsMVC.Models.ViewModels.Pdf;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class UsuarioController : PdfViewController
    {
        //const string DefaultUserName = "Consumidor 1234456";
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

        [AuthorizeUser(accion = "ClearSession", controlador = "Usuario")]
        public ActionResult ClearSession()
        {
            Session[JQGridFilterRule.Keys.Usuario.FILTERS] = null;
            Session[JQGridFilterRule.Keys.Usuario.SORD] = null;
            Session[JQGridFilterRule.Keys.Usuario.SIDX] = null;
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
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
                var consumidores = db.Usuarios.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID);
                return View(consumidores.ToList());
            }
        }

        // GET: Consumidor
        [AuthorizeUser(accion = "ReportePDF", controlador = "Usuario")]
        public ActionResult ReportePDF()
        {
            var operadorID = GetUserOperadorID();
            List<Usuario> consumidores = null;

            if (HttpContext.Session["UsuariosCargaMasiva"] != null)
            {
                consumidores = (List<Usuario>)HttpContext.Session["UsuariosCargaMasiva"];
            }
            else
            {
                consumidores = db.Usuarios.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).ToList();
            }

            IEnumerable<UsuarioViewModel> listConsumidores = consumidores.Select(x => new UsuarioViewModel
            {
                UsuarioID = x.UsuarioID,
                Legajo = x.Legajo,
                Apellido = x.Apellido,
                Nombre = x.Nombre,
                Dni = x.Dni,
                Numero = x.Numero,
                ClaveTerminal = x.ClaveTerminal,
                Locacion_Description = x.Locacion.Nombre,
                Jerarquia_Description = x.Jerarquia.Nombre,
                Email = x.ApplicationUsers.Any() ? x.ApplicationUsers.First().Email : String.Empty,
                Clave = x.ApplicationUsers.Any() && x.ApplicationUsers.First().Email.StartsWith("consumidor") ? (int?)x.ClaveTerminal : null
            });

            return this.ViewPdf("", "ReportePDF", listConsumidores);
        }

        // GET: Consumidor
        [AuthorizeUser(accion = "ReporteConsumidoresPDF", controlador = "Usuario")]
        public ActionResult ReporteConsumidoresPDF(string jqGridPostDataPdf)
        {
            var consumidores = GetConsumidoresPdfFilterQuery(jqGridPostDataPdf, true);

            return this.ViewPdf("", "ReportePDF", consumidores);
        }

        [Audit]
        public ActionResult ExportData(string jqGridPostDataExcel)
        {
            var consumidores = GetConsumidoresExcelFilterQuery(jqGridPostDataExcel, false);
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");

            XSSFWorkbook workbook = new XSSFWorkbook();

            ISheet sheet = workbook.CreateSheet("Usuarios");

            int amountOfColumns = 0;

            IRow headerRow = sheet.CreateRow(0);

            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Jerarquía");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Número");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Clave Terminal");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Apellido");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Dni");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Legajo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Efectivo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Crédito zona 1");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Crédito zona 2");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Crédito zona 3");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Crédito zona 4");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Crédito zona 5");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha vencimiento");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Inhibido");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Email");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var consumidor in consumidores)
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(consumidor.OperadorNombre);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Locacion);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Jerarquia);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Numero);
                row.CreateCell(colIdx++).SetCellValue(consumidor.ClaveTerminal);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Nombre);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Apellido);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Dni.HasValue ? consumidor.Dni.Value.ToString() : "");
                row.CreateCell(colIdx++).SetCellValue(consumidor.Legajo);
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(consumidor.Efectivo));
                row.CreateCell(colIdx++).SetCellValue(consumidor.Locacion != null && consumidor.NombreZona1 != null ? Convert.ToDouble(consumidor.CreditoZona1).ToString() : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Locacion != null && consumidor.NombreZona2 != null ? Convert.ToDouble(consumidor.CreditoZona2).ToString() : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Locacion != null && consumidor.NombreZona3 != null ? Convert.ToDouble(consumidor.CreditoZona3).ToString() : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Locacion != null && consumidor.NombreZona4 != null ? Convert.ToDouble(consumidor.CreditoZona4).ToString() : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Locacion != null && consumidor.NombreZona5 != null ? Convert.ToDouble(consumidor.CreditoZona5).ToString() : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(consumidor.FechaVencimiento.HasValue ? consumidor.FechaVencimiento.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(consumidor.Inhibido ? "SI" : "NO");
                row.CreateCell(colIdx++).SetCellValue(consumidor.Email);

                for (int j = 0; j < colIdx; j++)
                {
                    row.Cells[j].CellStyle = defaultCellStyle;
                }
            }

            HSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                sheet.AutoSizeColumn(i);
                // Add approx 8px to width
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1 * 256);
            }

            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Consumidores" + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        [AuthorizeUser(accion = "CargaMasiva", controlador = "Usuario")]
        public ActionResult CargaMasiva()
        {
            var operadorID = GetUserOperadorID();

            var operadores = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "OperadorID", "Nombre");
            //ViewBag.OperadorID = AddDefaultOption(operadores, "Seleccionar Operador", "");

            ViewBag.OperadorID = GetUserOperadorID();

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");
            ViewBag.JerarquiaID = new SelectList(string.Empty, "JerarquiaID", "Nombre");
            return View(new UsuarioViewModel());
        }

        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public ActionResult CargaMasiva(int? cantidadConsumidores, DateTime? FechaVencimiento, [Bind(Include = "cantidadConsumidores,FechaVencimiento,Efectivo,JerarquiaID,OperadorID,LocacionID")] Usuario usuario)
        public ActionResult CargaMasiva(UsuarioViewModel viewModel)
        {
            //TODO: Hacer un viewmodel para que las validaciones esten en el modelo
            if (viewModel.CantidadConsumidores == null)
            {
                ModelState.AddModelError("CantidadConsumidores", "El campo Cantidad de Consumidores es obligatorio");
            }

            if (viewModel.LocacionID == null)
            {
                ModelState.AddModelError("LocacionID", "Debe elegir una locación");
            }

            if (viewModel.JerarquiaID == null)
            {
                ModelState.AddModelError("JerarquiaID", "Debe elegir una jerarquía");
            }

            ModelState.Remove("Dni");
            ModelState.Remove("Numero");
            ModelState.Remove("ClaveTerminal");

            viewModel.Dni = 1;
            viewModel.Numero = 1;
            viewModel.ClaveTerminal = 1;

            try
            {
                if (ModelState.IsValid)
                {
                    var maxNumero = 0;

                    if (db.Usuarios.Where(x => x.LocacionID == viewModel.LocacionID).Count() > 0)
                    {
                        maxNumero = db.Usuarios.Where(x => x.LocacionID == viewModel.LocacionID).Max(x => x.Numero);
                    }

                    Random random = new Random();
                    UsuariosCargaMasiva = new List<Usuario>();

                    var jerarquia = db.Jerarquias.Find(viewModel.JerarquiaID);
                    var locacion = db.Locaciones.Find(viewModel.LocacionID);
                    var operador = db.Operadores.Find(viewModel.OperadorID);

                    while (viewModel.CantidadConsumidores > 0)
                    {
                        string prefijoUsuario = "consumidor";
                        maxNumero = maxNumero + 1;

                        Usuario nuevoUsuario = new Usuario();
                        nuevoUsuario.UsuarioID = Guid.NewGuid();
                        nuevoUsuario.Efectivo = viewModel.Efectivo;
                        nuevoUsuario.Jerarquia = jerarquia;
                        nuevoUsuario.JerarquiaID = viewModel.JerarquiaID;
                        nuevoUsuario.Numero = maxNumero;
                        nuevoUsuario.Nombre = null;
                        nuevoUsuario.ClaveTerminal = random.Next(1000, 9999);
                        nuevoUsuario.Locacion = locacion;
                        nuevoUsuario.LocacionID = viewModel.LocacionID;
                        nuevoUsuario.Operador = operador;
                        nuevoUsuario.OperadorID = viewModel.OperadorID;
                        nuevoUsuario.FechaVencimiento = viewModel.FechaVencimiento;
                        nuevoUsuario.FechaCreacion = DateTime.Now;

                        nuevoUsuario.CreditoZona1 = viewModel.CreditoZona1;
                        nuevoUsuario.CreditoZona2 = viewModel.CreditoZona2;
                        nuevoUsuario.CreditoZona3 = viewModel.CreditoZona3;
                        nuevoUsuario.CreditoZona4 = viewModel.CreditoZona4;
                        nuevoUsuario.CreditoZona5 = viewModel.CreditoZona5;

                        UsuariosCargaMasiva.Add(nuevoUsuario);
                        db.Usuarios.Add(nuevoUsuario);
                        db.SaveChanges();

                        if (viewModel.GenerarCredencialesWeb)
                        {
                            var user = new ApplicationUser
                            {
                                UserName = prefijoUsuario + nuevoUsuario.Operador.Numero + nuevoUsuario.Locacion.Numero + maxNumero.ToString() + "@user.com",
                                Email = prefijoUsuario + nuevoUsuario.Operador.Numero + nuevoUsuario.Locacion.Numero + maxNumero.ToString() + "@user.com",
                                UsuarioID = nuevoUsuario.UsuarioID
                            };
                            var result = UserManager.Create(user, nuevoUsuario.ClaveTerminal.ToString());

                            if (result.Succeeded)
                            {
                                UserManager.AddToRoles(user.Id, "Consumidor");
                            }
                        }

                        db.SaveChanges();

                        viewModel.CantidadConsumidores = viewModel.CantidadConsumidores - 1;
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
            var operadorID = GetUserOperadorID() == Guid.Empty ? viewModel.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                                                "LocacionID", "Nombre", viewModel.LocacionID);
            ViewBag.JerarquiaID = new SelectList(db.Jerarquias.Where(x => x.LocacionID == viewModel.LocacionID).OrderBy(x => x.Nombre),
                                    "JerarquiaID", "Nombre", viewModel.JerarquiaID);

            return View(viewModel);
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

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");
            ViewBag.JerarquiaID = new SelectList(string.Empty, "JerarquiaID", "Nombre");

            UsuarioViewModel viewModel = new UsuarioViewModel();
            return View(viewModel);
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
        public ActionResult Create(UsuarioViewModel viewModel)
        {
            RemoveValidationIfIsNull(viewModel);

            Usuario usuario = UsuarioVMToEntity(viewModel);
            var operadorID = GetUserOperadorID() == Guid.Empty ? viewModel.OperadorID : GetUserOperadorID();

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }

            //Validación de usuario y locación.
            if (db.Usuarios.Where(x => x.LocacionID == viewModel.LocacionID && x.Numero == viewModel.Numero && x.UsuarioID != usuario.UsuarioID).Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "Ya existe un consumidor dado de alta con el número ingresado.");
            }

            ValidarCombos(usuario);

            if (ModelState.IsValid)
            {
                usuario.UsuarioID = Guid.NewGuid();
                usuario.FechaCreacion = DateTime.Now;
                db.Usuarios.Add(usuario);
                db.SaveChanges();

                if (viewModel.ApplicationUser != null && !string.IsNullOrWhiteSpace(viewModel.ApplicationUser.Email))
                {
                    var user = new ApplicationUser
                    {
                        UserName = viewModel.ApplicationUser.Email,
                        Email = viewModel.ApplicationUser.Email,
                        UsuarioID = usuario.UsuarioID
                    };
                    var result = UserManager.Create(user, viewModel.ApplicationUser.Password);

                    if (result.Succeeded)
                    {
                        List<string> rol = new List<string>();
                        rol.Add("Consumidor");
                        viewModel.RolesSeleccionados = rol.ToArray();

                        if (viewModel.RolesSeleccionados != null)
                        {
                            UserManager.AddToRoles(user.Id, viewModel.RolesSeleccionados);
                        }

                        return RedirectToAction("Index");
                    }
                    AddErrors(result);
                }
                return RedirectToAction("Index");
            }

            ViewBag.OperadorID = operadorID;

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                                                "LocacionID", "Nombre", usuario.LocacionID);
            ViewBag.JerarquiaID = new SelectList(db.Jerarquias.Where(x => x.LocacionID == usuario.LocacionID).OrderBy(x => x.Nombre),
                                    "JerarquiaID", "Nombre", usuario.JerarquiaID);

            return View(viewModel);
        }

        private void RemoveValidationIfIsNull(UsuarioViewModel consumidor)
        {
            if (consumidor.ApplicationUser == null || string.IsNullOrWhiteSpace(consumidor.ApplicationUser.Email) &&
                string.IsNullOrWhiteSpace(consumidor.ApplicationUser.Password))
            {
                ModelState.Remove("ApplicationUser.Email");
                ModelState.Remove("ApplicationUser.Password");
            }

            if (consumidor.Dni == 0)
            {
                ModelState.Remove("Dni");
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
            entity.FechaCreacion = viewModel.FechaCreacion;
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
            viewModel.FechaCreacion = entity.FechaCreacion;
            viewModel.NombreZona1 = entity.Locacion.NombreZona1;
            viewModel.NombreZona2 = entity.Locacion.NombreZona2;
            viewModel.NombreZona3 = entity.Locacion.NombreZona3;
            viewModel.NombreZona4 = entity.Locacion.NombreZona4;
            viewModel.NombreZona5 = entity.Locacion.NombreZona5;
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

            viewModel = UsuarioToViewModel(consumidor);
            ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);
            if (user != null)
            {
                IList<string> roles = UserManager.GetRoles(user.Id);
                viewModel.RolesSeleccionados = roles.ToArray();
                viewModel.ApplicationUser = new RegisterViewModel();
                viewModel.ApplicationUser.Email = user.Email;
            }
            var operadorID = GetUserOperadorID() == Guid.Empty ? consumidor.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                                                "LocacionID", "Nombre", consumidor.LocacionID);
            ViewBag.JerarquiaID = new SelectList(db.Jerarquias.Where(x => x.LocacionID == consumidor.LocacionID).OrderBy(x => x.Nombre),
                                    "JerarquiaID", "Nombre", consumidor.JerarquiaID);

            //viewModel.TieneDatosOpcionales();
            //viewModel.TieneDatosMonetarios();
            return View(viewModel);
        }

        // POST: Usuario/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public ActionResult Edit([Bind(Include = "OperadorID,LocacionID,JerarquiaID")] UsuarioViewModel viewModel)
        public ActionResult Edit(UsuarioViewModel viewModel)
        {
            //Buscamos si el usuario existe en aspnetusers
            ApplicationUser user = db.Users.SingleOrDefault(x => x.UsuarioID == viewModel.UsuarioID);
            var operadorID = GetUserOperadorID() == Guid.Empty ? viewModel.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;

            if (user != null &&
                string.IsNullOrEmpty(viewModel.ApplicationUser.Password) &&
                string.IsNullOrEmpty(viewModel.ApplicationUser.ConfirmPassword))
            {
                ModelState.Remove("ApplicationUser.Password");
                ModelState.Remove("ApplicationUser.ConfirmPassword");
            }

            //Validación de usuario y locación.
            if (db.Usuarios.Where(x => x.LocacionID == viewModel.LocacionID && x.Numero == viewModel.Numero && x.UsuarioID != viewModel.UsuarioID).Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "Ya existe un consumidor dado de alta con el número ingresado.");
            }

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }
            if (viewModel.LocacionID == null)
            {
                ModelState.AddModelError("LocacionID", "Debe elegir una locación");
            }

            if (viewModel.JerarquiaID == null)
            {
                ModelState.AddModelError("JerarquiaID", "Debe elegir una jerarquía");
            }

            RemoveValidationIfIsNull(viewModel);
            //ModelState.Remove("Dni");

            Usuario consumidor = UsuarioVMToEntity(viewModel);
            if (ModelState.IsValid)
            {
                db.Entry(consumidor).State = EntityState.Modified;

                //El usuario existe en aspnetusers
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(viewModel.ApplicationUser.Password) &&
                        !string.IsNullOrEmpty(viewModel.ApplicationUser.ConfirmPassword))
                    {
                        string token = UserManager.GeneratePasswordResetToken(user.Id);
                        UserManager.ResetPassword(user.Id, token, viewModel.ApplicationUser.Password);
                    }

                    List<string> rolConsumidor = new List<string>();
                    rolConsumidor.Add("Consumidor");
                    viewModel.RolesSeleccionados = rolConsumidor.ToArray();

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
                    if (viewModel.ApplicationUser != null && !string.IsNullOrWhiteSpace(viewModel.ApplicationUser.Email))
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
                            List<string> rol = new List<string>();
                            rol.Add("Consumidor");
                            viewModel.RolesSeleccionados = rol.ToArray();

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

            if (consumidor.OperadorID == Guid.Empty)
            {
                //var operadores = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre), 
                //    "OperadorID", "Nombre");
                //ViewBag.OperadorID = AddDefaultOption(operadores, "Seleccionar Operador", "");
                ViewBag.operadorID = operadorID;
                ViewBag.LocacionID = new SelectList(string.Empty, "LocacionID", "Nombre");
                ViewBag.JerarquiaID = new SelectList(string.Empty, "JerarquiaID", "Nombre");
            }
            else
            {
                //ViewBag.OperadorID = new SelectList(db.Operadores.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                //    "OperadorID", "Nombre", viewModel.OperadorID);
                ViewBag.operadorID = operadorID;
                ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == consumidor.OperadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre", consumidor.LocacionID);
                ViewBag.JerarquiaID = new SelectList(db.Jerarquias.Where(x => x.LocacionID == consumidor.LocacionID).OrderBy(x => x.Nombre), "JerarquiaID", "Nombre", consumidor.JerarquiaID);
            }

            //viewModel.TieneDatosOpcionales();
            //viewModel.TieneDatosMonetarios();
            return View(viewModel);
        }

        // GET: Usuario/Delete/5
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
            return View(consumidor);
        }

        // POST: Usuario/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();
            Usuario consumidor = db.Usuarios.Find(id);

            var appUser = db.Users.SingleOrDefault(x => x.UsuarioID == consumidor.UsuarioID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => x.UsuarioID == consumidor.UsuarioID);

            if (appUser != null)
            {
                foreach (var role in UserManager.GetRoles(appUser.Id))
                {
                    UserManager.RemoveFromRoles(appUser.Id, role);
                }

                UserManager.Delete(appUser);
            }

            foreach (var item in stocksHistoricos)
            {
                consumidor.StocksHistoricos.Remove(item);
                db.Entry(item).State = EntityState.Deleted;
            }

            List<Transaccion> transaccionList = new List<Transaccion>();

            foreach (var transaccion in consumidor.Transacciones.ToList())
            {
                consumidor.Transacciones.Remove(transaccion);
                db.Entry(transaccion).State = EntityState.Deleted;
            }

            db.Usuarios.Remove(consumidor);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public JsonResult GetUsuariosByLocacionSelectList(string locacionID)
        {
            Guid locacionGuid = (!string.IsNullOrEmpty(locacionID)) ? new Guid(locacionID) : Guid.Empty;
            var ret = db.Usuarios.Where(x => x.LocacionID == locacionGuid).Select(x => new SelectListItem()
            {
                Text = (!String.IsNullOrEmpty(x.Nombre)) ? x.Apellido + ((!string.IsNullOrEmpty(x.Apellido) && !string.IsNullOrEmpty(x.Nombre)) ? ", " : "") + x.Nombre : "Consumidor " + x.Numero,
                Value = x.UsuarioID.ToString()
            }).OrderBy(x => x.Text).ToList();
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUsuariosByLocacionesMultiSelectList(string[] locaciones, bool obtenerTodosLocaciones)
        {
            if (locaciones.Length == 1 && locaciones[0] == string.Empty && obtenerTodosLocaciones == false)
            {
                return Json(new MultiSelectList(string.Empty, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }

            List<Guid> locacionesID = new List<Guid>();
            if (obtenerTodosLocaciones)
            {
                var operadorID = GetUserOperadorID();
                locacionesID = db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID).ToList();
            }
            else
            {
                foreach (string item in locaciones)
                {
                    locacionesID.Add(new Guid(item));
                }
            }

            var ret = new MultiSelectList(db.Usuarios.Where(x => x.LocacionID.HasValue && locacionesID.Contains(x.LocacionID.Value)).Select(x => new SelectListItem()
            {
                Text = (x.Nombre != null) ? x.Apellido + ((!string.IsNullOrEmpty(x.Apellido) && !string.IsNullOrEmpty(x.Nombre)) ? ", " : "") + x.Nombre : "Consumidor " + x.Numero,
                Value = x.UsuarioID.ToString()
            }).OrderBy(x => x.Text).ToList(), "Value", "Text");
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ClearSessionPage()
        {
            Session[JQGridFilterRule.Keys.Usuario.PAGE] = null;

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllUsers(string sidx, string sord, int page, int rows, string filters, bool? newPage)
        {
            string operadorID = GetUserOperadorID().ToString();

            if (operadorID == Guid.Empty.ToString())
            {
                operadorID = null;
            }

            int totalPages;
            int totalRecords;
            if (string.IsNullOrEmpty(filters))
            {
                filters = (string)Session[JQGridFilterRule.Keys.Usuario.FILTERS];
                if ((!newPage.HasValue || !newPage.Value) && Session[JQGridFilterRule.Keys.Usuario.PAGE] != null)
                {
                    page = (int)Session[JQGridFilterRule.Keys.Usuario.PAGE];
                }
                else
                {
                    Session[JQGridFilterRule.Keys.Usuario.PAGE] = page;
                }
            }
            else
            {
                Session[JQGridFilterRule.Keys.Usuario.FILTERS] = filters;
                Session[JQGridFilterRule.Keys.Usuario.PAGE] = 1;
            }

            if (string.IsNullOrEmpty(sidx) && Session[JQGridFilterRule.Keys.Usuario.SIDX] != null)
            {
                sidx = (string)Session[JQGridFilterRule.Keys.Usuario.SIDX];
                sord = (string)Session[JQGridFilterRule.Keys.Usuario.SORD];
            }
            else
            {
                Session[JQGridFilterRule.Keys.Usuario.SIDX] = sidx;
                Session[JQGridFilterRule.Keys.Usuario.SORD] = sord;
            }



            int pageIndex = Convert.ToInt32(page) - 1;

            int pageSize = rows;

            IEnumerable<ConsumidoresGridViewModel> consumidores;
            //Armola query principal con dapper.
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                var baseQuery = @"Select
                            count(1)
                            from Usuario u
                            left join Operador o on o.OperadorID = u.OperadorID
                            left join Jerarquia j on j.JerarquiaID = u.JerarquiaID
                            left join Locacion l on l.LocacionID = u.LocacionID
                            left join AspNetUsers anu on anu.UsuarioID = u.UsuarioID
                            left join AspNetUserRoles anur on anur.UserId = anu.Id
                            left join AspNetRoles anr on anr.Id = anur.RoleId";

                JQGridQueryBuilder<ConsumidoresGridViewModel> qb = new JQGridQueryBuilder<ConsumidoresGridViewModel>();
                var query = qb.WithBaseQuery(baseQuery)
                    .AddColumnMap("Nombre", "u.Nombre")
                    .AddColumnMap("OperadorNombre", "o.Nombre")
                    .AddColumnMap("Jerarquia", "j.Nombre")
                    .AddColumnMap("Locacion", "l.Nombre")
                    .AddColumnMap("FechaVencimiento", "u.FechaVencimiento")
                    .AddColumnMap("FechaInibicion", "u.FechaInibicion")
                    .AddColumnMap("FechaCreacion", "u.FechaCreacion")
                    .AddColumnMap("Numero", "u.Numero")
                    .WithFilters(filters)
                    .WithCustomFilters("(anr.Name = 'Consumidor' or anu.Id is null) and(u.OperadorID = '" + operadorID + "' or '" + operadorID + "' = '')")
                    .Query();

                totalRecords = db.Query<int>(query).First();

                baseQuery = @"Select
                            u.UsuarioID,
                            o.Nombre OperadorNombre,
                            j.Nombre Jerarquia,
                            l.Nombre Locacion,
                            u.Nombre,
                            Apellido,
                            Legajo,
                            isNull(u.Dni, 0) Dni,
                            u.Numero,
                            ClaveTerminal,
                            FechaVencimiento FechaVencimiento,
                            Inhibido,
                            FechaInhibido,
                            Efectivo,
                            FechaCreacion FechaCreacion,
                            CreditoZona1,
                            CreditoZona2,
                            CreditoZona3,
                            CreditoZona4,
                            CreditoZona5,
                            anu.Email Email
                            from Usuario u
                            left join Operador o on o.OperadorID = u.OperadorID
                            left join Jerarquia j on j.JerarquiaID = u.JerarquiaID
                            left join Locacion l on l.LocacionID = u.LocacionID
                            left join AspNetUsers anu on anu.UsuarioID = u.UsuarioID
                            left join AspNetUserRoles anur on anur.UserId = anu.Id
                            left join AspNetRoles anr on anr.Id = anur.RoleId";

                qb = new JQGridQueryBuilder<ConsumidoresGridViewModel>();

                query = qb.WithBaseQuery(baseQuery)
                    .WithDefaultSortField("UsuarioID")
                    .AddColumnMap("Nombre", "u.Nombre")
                    .AddColumnMap("Apellido", "u.Apellido")
                    .AddColumnMap("OperadorNombre", "o.Nombre")
                    .AddColumnMap("Jerarquia", "j.Nombre")
                    .AddColumnMap("Locacion", "l.Nombre")
                    .AddColumnMap("FechaVencimiento", "u.FechaVencimiento")
                    .AddColumnMap("FechaInhibido", "u.FechaInhibido")
                    .AddColumnMap("FechaCreacion", "u.FechaCreacion")
                    .AddColumnMap("Numero", "u.Numero")
                    .AddColumnMap("Dni", "u.Dni")
                    .WithFilters(filters)
                    .WithCustomFilters("(anr.Name = 'Consumidor' or anu.Id is null) and(u.OperadorID = '" + operadorID + "' or '" + operadorID + "' = '')")
                    .WithSort(sidx, sord)
                    .WithPagination(page, rows)
                    .Query();

                consumidores = db.Query<ConsumidoresGridViewModel>(query);
            }

            totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);
            var a = filters != null ? JObject.Parse(filters) : null;
            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = consumidores.AsQueryable(),
                filters = Helpers.JQGridFilterRule.Parse(filters)
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<Usuario> GetConsumidoresQuery()
        {
            var usuariosConsumidores =
              from usuario in db.Usuarios
              join appUser in db.Users on usuario.UsuarioID equals appUser.UsuarioID
              join rol in db.Roles on appUser.Roles.FirstOrDefault().RoleId equals rol.Id
              where rol.Name == "Consumidor"
              select usuario;

            var usuariosSinRoles =
                from usuario in db.Usuarios
                where usuario.ApplicationUsers.Count() == 0
                select usuario;

            return usuariosConsumidores.Union(usuariosSinRoles);
        }

        private IEnumerable<ConsumidoresPdfViewModel> GetConsumidoresPdfFilterQuery(string jqGridPostData, bool isPDF)
        {
            var operadorID = GetUserOperadorID().ToString();

            if (operadorID == Guid.Empty.ToString())
            {
                operadorID = null;
            }

            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            string filters = fixedPostData;

            IEnumerable<ConsumidoresPdfViewModel> consumidores = new List<ConsumidoresPdfViewModel>();

            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                var baseQuery = @"Select
                            j.Nombre Jerarquia,
                            u.Nombre Nombre,
                            u.Apellido Apellido,
                            u.Legajo Legajo,
                            u.Numero,
                            u.ClaveTerminal
                            from Usuario u
                            left join Operador o on o.OperadorID = u.OperadorID
                            left join Jerarquia j on j.JerarquiaID = u.JerarquiaID
                            left join Locacion l on l.LocacionID = u.LocacionID
                            left join AspNetUsers anu on anu.UsuarioID = u.UsuarioID
                            left join AspNetUserRoles anur on anur.UserId = anu.Id
                            left join AspNetRoles anr on anr.Id = anur.RoleId";

                JQGridQueryBuilder<ConsumidoresPdfViewModel> qb = new JQGridQueryBuilder<ConsumidoresPdfViewModel>();

                var query = qb.WithBaseQuery(baseQuery)
                    .WithDefaultSortField("UsuarioID")
                    .AddColumnMap("Nombre", "u.Nombre")
                    .AddColumnMap("Apellido", "u.Apellido")
                    .AddColumnMap("OperadorNombre", "o.Nombre")
                    .AddColumnMap("Jerarquia", "j.Nombre")
                    .AddColumnMap("Locacion", "l.Nombre")
                    .AddColumnMap("FechaVencimiento", "u.FechaVencimiento")
                    .AddColumnMap("FechaInibicion", "u.FechaInibicion")
                    .AddColumnMap("FechaCreacion", "u.FechaCreacion")
                    .AddColumnMap("Numero", "u.Numero")
                    .WithFilters(filters)
                    .WithCustomFilters("(anr.Name = 'Consumidor' or anu.Id is null) and(u.OperadorID = '" + operadorID + "' or '" + operadorID + "' = '')")
                    .WithSort("FechaCreacion", "asc")
                    .Query();

                consumidores = db.Query<ConsumidoresPdfViewModel>(query);
            }

            return consumidores;
        }


        private IEnumerable<ConsumidoresExcelViewModel> GetConsumidoresExcelFilterQuery(string jqGridPostData, bool isPDF)
        {
            var operadorID = GetUserOperadorID().ToString();

            if (operadorID == Guid.Empty.ToString())
            {
                operadorID = null;
            }

            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            string filters = fixedPostData;

            IEnumerable<ConsumidoresExcelViewModel> consumidores = new List<ConsumidoresExcelViewModel>();
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                var baseQuery = @"Select
                            u.UsuarioID,
                            o.Nombre OperadorNombre,
                            j.Nombre Jerarquia,
                            l.Nombre Locacion,
		                    l.NombreZona1,
							l.NombreZona2,
							l.NombreZona3,
							l.NombreZona4,
							l.NombreZona5,                          
                            u.Nombre Nombre,        
                            u.Apellido Apellido,
                            u.Legajo Legajo,
                            u.Numero,
                            u.ClaveTerminal,
                            u.FechaVencimiento,
                            u.Inhibido,
                            u.FechaInhibido,
                            u.Efectivo,
                            u.FechaCreacion,
                            u.CreditoZona1,
                            u.CreditoZona2,
                            u.CreditoZona3,
                            u.CreditoZona4,
                            u.CreditoZona5,
                            anu.Email Email
                            from Usuario u
                            left join Operador o on o.OperadorID = u.OperadorID
                            left join Jerarquia j on j.JerarquiaID = u.JerarquiaID
                            left join Locacion l on l.LocacionID = u.LocacionID
                            left join AspNetUsers anu on anu.UsuarioID = u.UsuarioID
                            left join AspNetUserRoles anur on anur.UserId = anu.Id
                            left join AspNetRoles anr on anr.Id = anur.RoleId";

                JQGridQueryBuilder<ConsumidoresExcelViewModel> qb = new JQGridQueryBuilder<ConsumidoresExcelViewModel>();
                var query = qb.WithBaseQuery(baseQuery)
                    .WithDefaultSortField("UsuarioID")
                    .AddColumnMap("Nombre", "u.Nombre")
                    .AddColumnMap("Apellido", "u.Apellido")
                    .AddColumnMap("OperadorNombre", "o.Nombre")
                    .AddColumnMap("Jerarquia", "j.Nombre")
                    .AddColumnMap("Locacion", "l.Nombre")
                    .AddColumnMap("FechaVencimiento", "u.FechaVencimiento")
                    .AddColumnMap("FechaInibicion", "u.FechaInibicion")
                    .AddColumnMap("FechaCreacion", "u.FechaCreacion")
                    .AddColumnMap("Numero", "u.Numero")
                    .WithFilters(filters)
                    .WithCustomFilters("(anr.Name = 'Consumidor' or anu.Id is null) and(u.OperadorID = '" + operadorID + "' or '" + operadorID + "' = '')")
                    .WithSort("FechaCreacion", "asc")
                    .Query();

                consumidores = db.Query<ConsumidoresExcelViewModel>(query);
            }

            return consumidores;
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

        public JsonResult GetLocaciones(string operadorID)
        {
            Guid operadorGuid = new Guid(operadorID);
            List<SelectListItem> locaciones = new List<SelectListItem>();

            var lista = db.Locaciones.Where(x => x.OperadorID == operadorGuid);

            foreach (var item in lista)
            {
                locaciones.Add(new SelectListItem { Text = item.Nombre, Value = item.LocacionID.ToString() });
            }

            return Json(new SelectList(locaciones, "Value", "Text"));
        }

        public JsonResult GetJerarquias(string locacionID)
        {
            Guid locacionGuid = new Guid(locacionID);
            List<SelectListItem> jerarquias = new List<SelectListItem>();

            var lista = db.Jerarquias.Where(x => x.LocacionID == locacionGuid);

            foreach (var item in lista)
            {
                jerarquias.Add(new SelectListItem { Text = item.Nombre, Value = item.JerarquiaID.ToString() });
            }

            return Json(new SelectList(jerarquias, "Value", "Text"));
        }

        public JsonResult GetDatosMonetariosByLocacion(string locacionID)
        {
            Guid locacionGuid = new Guid(locacionID);
            Locacion locacion = db.Locaciones.Where(x => x.LocacionID == locacionGuid).First();

            List<string> nombreZonas = new List<string>();

            nombreZonas.Add(locacion.NombreZona1);
            nombreZonas.Add(locacion.NombreZona2);
            nombreZonas.Add(locacion.NombreZona3);
            nombreZonas.Add(locacion.NombreZona4);
            nombreZonas.Add(locacion.NombreZona5);

            return Json(nombreZonas);
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
