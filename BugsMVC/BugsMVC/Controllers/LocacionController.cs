using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BugsMVC.DAL;
using BugsMVC.Models;
using Microsoft.AspNet.Identity;
using BugsMVC.Handlers;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Linq.Dynamic;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using BugsMVC.Security;
using BugsMVC.Models.ViewModels;


namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class LocacionController : BaseController
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

        // GET: Locacion
        public ActionResult Index()
        {
            var locaciones = db.Locaciones.Include(l => l.Operador);
            return View(locaciones.ToList());
        }

        // GET: Locacion/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Locacion locacion = db.Locaciones.Find(id);
            if (locacion == null)
            {
                return HttpNotFound();
            }

            LocacionViewModel viewModel = LocacionViewModel.From(locacion);

            return View(viewModel);
        }

        // GET: Locacion/Create
        public ActionResult Create()
        {
            var operadorID = GetUserOperadorID();
            ViewBag.OperadorID = GetUserOperadorID();

            if (operadorID == Guid.Empty)
                ModelState.AddModelError(String.Empty, "Por favor, seleccione un Operador");

            ViewBag.HasZona1 = false;

            LocacionViewModel viewModel = new LocacionViewModel();

            if (User.IsInRole("SuperAdmin"))
                viewModel.EsSuperadmin = true;

            return View(viewModel);
        }

        // POST: Locacion/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LocacionViewModel viewModel)
        {
            ValidarNombreZona(viewModel);
            ModelState.Remove("LocacionID");
            var existe = db.Locaciones.Any(x => x.Nombre == viewModel.Nombre);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre de la locación ya existe.");
            }

            RemoveValidationIfIsNull(viewModel);

            var operadorID = GetUserOperadorID();

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }

            if (String.IsNullOrEmpty(viewModel.NombreZona1))
            {
                ModelState.AddModelError(string.Empty, "Debe completar al menos la zona 1.");
            }

            if (ModelState.IsValid)
            {
                viewModel.LocacionID = Guid.NewGuid();
                viewModel.OperadorID = operadorID;
                Locacion entity = new Locacion();
                viewModel.ToEntity(entity);
                db.Locaciones.Add(entity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OperadorID = operadorID;
            ViewBag.HasZona1 = !String.IsNullOrEmpty(viewModel.NombreZona1);
            return View(viewModel);
        }

        private void ValidarNombreZona(LocacionViewModel locacion)
        {
            List<string> nombreZonas = new List<string>();

            if (!String.IsNullOrEmpty(locacion.NombreZona1))
                nombreZonas.Add(locacion.NombreZona1);
            if (!String.IsNullOrEmpty(locacion.NombreZona2))
                nombreZonas.Add(locacion.NombreZona2);
            if (!String.IsNullOrEmpty(locacion.NombreZona3))
                nombreZonas.Add(locacion.NombreZona3);
            if (!String.IsNullOrEmpty(locacion.NombreZona4))
                nombreZonas.Add(locacion.NombreZona4);
            if (!String.IsNullOrEmpty(locacion.NombreZona5))
                nombreZonas.Add(locacion.NombreZona5);

            var duplicados = nombreZonas.GroupBy(x => x)
                .SelectMany(grp => grp.Skip(1));

            if (duplicados.Count() >= 1)
            {
                ModelState.AddModelError(string.Empty, "Las zonas tienen nombres repetidos.");
            }
        }

        private void RemoveValidationIfIsNull(LocacionViewModel locacion)
        {
            if (!locacion.Zona1Activa)
            {
                ModelState.Remove("NombreZona1");
                //ModelState.Remove("PeriodoRecargaZona1");
            }

            if (!locacion.Zona2Activa)
            {
                ModelState.Remove("NombreZona2");
                //ModelState.Remove("PeriodoRecargaZona2");
            }

            if (!locacion.Zona3Activa)
            {
                ModelState.Remove("NombreZona3");
                //ModelState.Remove("PeriodoRecargaZona3");
            }

            if (!locacion.Zona4Activa)
            {
                ModelState.Remove("NombreZona4");
                //ModelState.Remove("PeriodoRecargaZona4");
            }

            if (!locacion.Zona5Activa)
            {
                ModelState.Remove("NombreZona5");
                //ModelState.Remove("PeriodoRecargaZona5");
            }

        }

        // GET: Locacion/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Locacion locacion = db.Locaciones.Find(id);
            if (locacion == null)
            {
                return HttpNotFound();
            }

            LocacionViewModel viewModel = LocacionViewModel.From(locacion);

            if(User.IsInRole("SuperAdmin"))
                viewModel.EsSuperadmin = true;

            var operadorID = GetUserOperadorID();
            ViewBag.OperadorID = operadorID == Guid.Empty ? locacion.OperadorID : operadorID;
            ViewBag.HasZona1 = !String.IsNullOrEmpty(locacion.NombreZona1);
            return View(viewModel);
        }

        // POST: Locacion/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LocacionViewModel viewModel)
        {
            RemoveValidationIfIsNull(viewModel);
            ValidarNombreZona(viewModel);

            var operadorID = GetUserOperadorID();
            ViewBag.OperadorID = operadorID == Guid.Empty ? viewModel.OperadorID : operadorID;

            var existe = db.Locaciones.Any(x => x.Nombre == viewModel.Nombre && x.LocacionID != viewModel.LocacionID);

            if (existe && viewModel.OperadorID !=operadorID)
            {
                ModelState.AddModelError("Nombre", "El nombre de la locación ya existe.");
            }

            if (String.IsNullOrEmpty(viewModel.NombreZona1))
            {
                ModelState.AddModelError(string.Empty, "Debe completar al menos la zona 1.");
            }

            if (ModelState.IsValid)
            {
                Locacion entity = new Locacion();
                viewModel.ToEntity(entity);

                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HasZona1 = !String.IsNullOrEmpty(viewModel.NombreZona1);
            return View(viewModel);
        }

        // GET: Locacion/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Locacion locacion = db.Locaciones.Find(id);
            if (locacion == null)
            {
                return HttpNotFound();
            }
            return View(locacion);
        }

        // POST: Locacion/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();
            Locacion locacion = db.Locaciones.Find(id);

            var maquinas = locacion.Maquinas;
            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacion.LocacionID);
            var transacciones = db.Transacciones.Where(x => x.LocacionID == locacion.LocacionID);
            var jerarquias = db.Jerarquias.Where(x => x.LocacionID == locacion.LocacionID);
            var usuarios = db.Usuarios.Where(x => x.LocacionID == locacion.LocacionID);
            var users = db.Users.Where(x => x.Usuario.LocacionID == locacion.LocacionID);
            var tablesOfflines = db.TablasOfflines.Where(x => x.LocacionID == locacion.LocacionID);
            var alarmaConfiguracion = db.AlarmaConfiguracion.Where(x => x.LocacionID.HasValue && x.LocacionID.Value == locacion.LocacionID);

            foreach (var user in users.ToList())
            {
                UserManager.RemoveFromRoles(user.Id, new string[] { "SuperAdmin", "Operador", "Consumidor", "Repositor", "Técnico", "Administrador", "Proveedor" });
                UserManager.Delete(user);
            }
            
            //Se eliminan
            alarmaConfiguracion.ToList().ForEach(x => db.AlarmaConfiguracionDetalle.RemoveRange(x.AlarmaConfiguracionDetalles));
            alarmaConfiguracion.ToList().ForEach(x => db.AlarmaConfiguracion.Remove(x));
            articulosAsignaciones.ToList().ForEach(x => db.Stocks.ToList().ForEach(y=> db.StocksHistoricos.RemoveRange(y.StocksHistoricos.ToList())));
            articulosAsignaciones.ToList().ForEach(x => db.Stocks.RemoveRange(x.Stocks.ToList()));
            db.ArticulosAsignaciones.RemoveRange(articulosAsignaciones);
            db.Transacciones.RemoveRange(transacciones);
            db.Usuarios.RemoveRange(usuarios);
            db.Jerarquias.RemoveRange(jerarquias);
            db.TablasOfflines.RemoveRange(tablesOfflines);
            //Se desasignan las máquinas
            maquinas.ToList().ForEach(x=>x.DesasignarMaquina());
            //Finalmente se elimina la locación.
            db.Locaciones.Remove(locacion);

            db.SaveChanges();

            return RedirectToAction("Index");
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

        public JsonResult GetAllLocaciones()
        {
            var operadorID = GetUserOperadorID();

            var locaciones = db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID)
                .Select(x => new
                {
                    LocacionID = x.LocacionID,
                    OperadorNombre= x.Operador.Nombre,
                    CodigoPostal = x.CodigoPostal,
                    Direccion = x.Direccion,
                    Localidad = x.Localidad,
                    Nombre = x.Nombre,
                    NombreZona1 = x.NombreZona1,
                    NombreZona2 = x.NombreZona2,
                    NombreZona3 = x.NombreZona3,
                    NombreZona4 = x.NombreZona4,
                    NombreZona5 = x.NombreZona5,
                    Numero = x.Numero,
                    Provincia = x.Provincia,
                    CUIT = x.CUIT
                });

            return Json(locaciones.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [Audit]
        public ActionResult ExportData(string jqGridPostData)
        {
            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

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

            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");
            var operadorID = GetUserOperadorID();

            var locaciones = db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID)
                .Select(x => new
                {
                    Nombre = x.Nombre,
                    Numero = x.Numero,
                    OperadorNombre = x.Operador.Nombre,
                    Cuit = x.CUIT,
                    Direccion = x.Direccion,
                    Localidad = x.Localidad,
                    Provincia = x.Provincia,
                    CodigoPostal = x.CodigoPostal,
                    NombreZona1 = x.NombreZona1,
                    NombreZona2 = x.NombreZona2,
                    NombreZona3 = x.NombreZona3,
                    NombreZona4 = x.NombreZona4,
                    NombreZona5 = x.NombreZona5,
                })
                .Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            ISheet sheet = workbook.CreateSheet("Locaciones");

            int amountOfColumns = 0;

            IRow headerRow = sheet.CreateRow(0);

            if(esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Número");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 1");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 2");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 3");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 4");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 5");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("CUIT");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Dirección");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Localidad");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Provincia");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Código Postal");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var locacion in locaciones.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;
                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(locacion.OperadorNombre);

                row.CreateCell(colIdx++).SetCellValue(locacion.Nombre);
                row.CreateCell(colIdx++).SetCellValue(locacion.Numero.ToString());
                row.CreateCell(colIdx++).SetCellValue(locacion.NombreZona1);
                row.CreateCell(colIdx++).SetCellValue(locacion.NombreZona2);
                row.CreateCell(colIdx++).SetCellValue(locacion.NombreZona3);
                row.CreateCell(colIdx++).SetCellValue(locacion.NombreZona4);
                row.CreateCell(colIdx++).SetCellValue(locacion.NombreZona5);
                row.CreateCell(colIdx++).SetCellValue(locacion.Cuit);
                row.CreateCell(colIdx++).SetCellValue(locacion.Direccion);
                row.CreateCell(colIdx++).SetCellValue(locacion.Localidad);
                row.CreateCell(colIdx++).SetCellValue(locacion.Provincia);
                row.CreateCell(colIdx++).SetCellValue(locacion.CodigoPostal);

                for (int j = 0; j < colIdx; j++)
                {
                    row.Cells[j].CellStyle = defaultCellStyle;
                    if (row.Cells[j].CellType == CellType.Numeric)
                    {
                        row.Cells[j].CellStyle.DataFormat = doubleFormat;
                    }
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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Locaciones " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
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
