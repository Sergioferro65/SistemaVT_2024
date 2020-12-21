using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
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
using BugsMVC.Models.ViewModels;
using BugsMVC.Security;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class ArticuloController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: Articulo
        [AuthorizeUser(accion = "Index", controlador = "Articulo")]
        public ActionResult Index()
        {
            var articulosViewModel = db.Articulos.Include(l => l.Operador).ToList().Select(x => ArticuloViewModel.From(x));

            return View(articulosViewModel);
        }

        // GET: Articulo/Details/5
        [AuthorizeUser(accion = "Detalles", controlador = "Articulo")]
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulos.Find(id);
            if (articulo == null)
            {
                return HttpNotFound();
            }
            return View(articulo);
        }

        // GET: Articulo/Create
        [AuthorizeUser(accion = "Crear", controlador = "Articulo")]
        public ActionResult Create()
        {
            ViewBag.OperadorID = GetUserOperadorID();
            ArticuloViewModel viewModel = new ArticuloViewModel();
            return View(viewModel);
        }

        private IEnumerable<SelectListItem> AddDefaultOption(IEnumerable<SelectListItem> list, string dataTextField, string selectedValue)
        {
            var items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = dataTextField, Value = selectedValue });
            items.AddRange(list);
            return items;
        }

        // POST: Articulo/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //        public ActionResult Create([Bind(Include = "ArticuloID,OperadorID,Nombre,CostoReal,UnidasMedida,Marca,Modelo,Certificacion")] Articulo articulo)
        public ActionResult Create(ArticuloViewModel viewModel)
        {
            var operadorID = GetUserOperadorID();

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }

            var existe = db.Articulos.Any(x => x.OperadorID== operadorID && x.Nombre == viewModel.Nombre);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre seleccionado ya existe.");
            }

            if (ModelState.IsValid)
            {
                viewModel.ArticuloID = Guid.NewGuid();
                viewModel.OperadorID = operadorID;
                Articulo entity = new Articulo();
                viewModel.ToEntity(entity);

                db.Articulos.Add(entity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OperadorID = operadorID;
            return View(viewModel);
        }

        // GET: Articulo/Edit/5
        [AuthorizeUser(accion = "Editar", controlador = "Articulo")]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Articulo entity = db.Articulos.Find(id);

            if (entity == null)
            {
                return HttpNotFound();
            }
            ViewBag.OperadorID = GetUserOperadorID();

            var viewModel = ArticuloViewModel.From(entity);

            return View(viewModel);
        }

        // POST: Articulo/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "ArticuloID,Nombre,AsociarAPrecio,CostoReal,Marca,Modelo,Certificacion,LocacionID,NroZona")] Articulo articulo)
        public ActionResult Edit(ArticuloViewModel viewModel)
        {
            var operadorID = GetUserOperadorID();

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }

            var existe = db.Articulos.Any(x => x.Nombre == viewModel.Nombre && x.OperadorID==operadorID && x.ArticuloID != viewModel.ArticuloID);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre que desea guardar ya existe.");
            }
            
            viewModel.OperadorID = operadorID;
            if (ModelState.IsValid)
            {
                Articulo entity = new Articulo();
                viewModel.ToEntity(entity);

                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OperadorID = GetUserOperadorID();

            return View(viewModel);
        }

        // GET: Articulo/Delete/5
        [AuthorizeUser(accion = "Eliminar", controlador = "Articulo")]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulos.Find(id);

            if (articulo == null)
            {
                return HttpNotFound();
            }
            
            ArticuloViewModel viewModel = ArticuloViewModel.From(articulo);
            return View(viewModel);
        }

        // POST: Articulo/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Articulo articulo = db.Articulos.Find(id);

            var transacciones = db.Transacciones.Where(x => x.ArticuloID == articulo.ArticuloID);
            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => x.ArticuloID == articulo.ArticuloID);
            var articulosAsignacionesID = articulosAsignaciones.Select(x => x.Id);
            var stocks = db.Stocks.Where(x => articulosAsignacionesID.Contains(x.ArticuloAsignacionID));
            var stocksID = stocks.Select(x => x.StockID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => stocksID.Contains(x.StockID.Value));

            db.StocksHistoricos.RemoveRange(stocksHistoricos);
            db.Stocks.RemoveRange(stocks);
            db.ArticulosAsignaciones.RemoveRange(articulosAsignaciones);
            db.Transacciones.RemoveRange(transacciones);
            db.Articulos.Remove(articulo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetZonas(string locacionID)
        {
            if (locacionID == string.Empty)
                return Json(new SelectList(string.Empty, "Value", "Text"));

            Guid locacionGuid = new Guid(locacionID);
            return Json(new SelectList(GetZonasList(locacionGuid), "Value", "Text"));
        }

        public List<SelectListItem> GetZonasList(Guid locacionGuid)
        {
            List<SelectListItem> zonas = new List<SelectListItem>();

            var locacion = db.Locaciones.SingleOrDefault(x => x.LocacionID == locacionGuid);

            if (locacion != null)
            {
                if (!string.IsNullOrEmpty(locacion.NombreZona1)) zonas.Add(new SelectListItem { Text = locacion.NombreZona1, Value = "1" });
                if (!string.IsNullOrEmpty(locacion.NombreZona2)) zonas.Add(new SelectListItem { Text = locacion.NombreZona2, Value = "2" });
                if (!string.IsNullOrEmpty(locacion.NombreZona3)) zonas.Add(new SelectListItem { Text = locacion.NombreZona3, Value = "3" });
                if (!string.IsNullOrEmpty(locacion.NombreZona4)) zonas.Add(new SelectListItem { Text = locacion.NombreZona4, Value = "4" });
                if (!string.IsNullOrEmpty(locacion.NombreZona5)) zonas.Add(new SelectListItem { Text = locacion.NombreZona5, Value = "5" });
            }

            return zonas;
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

        public JsonResult GetAllArticulos()
        {
            var operadorID = GetUserOperadorID();

            var articulosViewModel = db.Articulos.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).ToList().Select(x => ArticuloViewModel.From(x));
            return Json(articulosViewModel, JsonRequestBehavior.AllowGet);
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
            var articulosViewModel = db.Articulos.Where(x => operadorID == Guid.Empty || x.OperadorID==operadorID).ToList().Select(x => ArticuloViewModel.From(x)).Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            ISheet sheet = workbook.CreateSheet("Artículos");

            int amountOfColumns = 0;

            IRow headerRow = sheet.CreateRow(0);

            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Costo Real");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Unidad Medida");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Marca");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Modelo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Certificación");
            
            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var item in articulosViewModel.ToList())
            {

                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(item.OperadorNombre);
                row.CreateCell(colIdx++).SetCellValue(item.Nombre);
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(item.CostoReal));
                row.CreateCell(colIdx++).SetCellValue(item.UnidadMedida.ToString());

                row.CreateCell(colIdx++).SetCellValue(item.Marca);
                row.CreateCell(colIdx++).SetCellValue(item.Modelo);
                row.CreateCell(colIdx++).SetCellValue(item.Certificacion);

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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Artículos "+ DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        //public JsonResult GetArticulosByLocacionMultiSelectList(string locacionID)
        //{
        //    Guid locacionGuid = (!string.IsNullOrEmpty(locacionID)) ? new Guid(locacionID) : Guid.Empty;
        //    var locacion = db.Locaciones.Find(locacionGuid);
        //    //var ret = new MultiSelectList(db.Articulos.Where(x => x.OperadorID == locacion.OperadorID).OrderBy(x => x.Nombre), "ArticuloID", "Nombre");

        //    var ret = new MultiSelectList(db.Articulos.Select(x => new SelectListItem()
        //    {
        //        Text = (x.Nombre != null) ? x.Nombre:string.Empty,
        //        Value = x.ArticuloID.ToString()
        //    }).OrderBy(x => x.Text).ToList(), "Value", "Text");
        //    return Json(ret, JsonRequestBehavior.AllowGet);
        //}

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
