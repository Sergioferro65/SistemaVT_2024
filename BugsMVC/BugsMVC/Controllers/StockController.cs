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
using Microsoft.AspNet.Identity;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Linq.Dynamic;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Globalization;
using BugsMVC.Commands;
using BugsMVC.Security;
using BugsMVC.Handlers;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class StockController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: Stocks
        public ActionResult Index()
        {
            var viewModel = db.Stocks.Where(x=>x.ArticuloAsignacion.ControlStock == true).ToList().Select(x => StockViewModel.From(x));
            return View(viewModel);
        }

        //public JsonResult GetStockByArticuloAsignacion(Guid articuloAsignacionID)
        //{
        //    var stocks = db.Stocks.Where(x => x.ArticuloAsignacionID == articuloAsignacionID).ToList()
        //                            .Select(x => new
        //                            {
        //                                StockID = x.StockID,
        //                                Cantidad = x.Cantidad,
        //                                ArticuloAsignacionID = x.ArticuloAsignacionID,
        //                                FechaAviso = x.FechaAviso,
        //                                FechaEdicionWeb = x.FechaEdicionWeb,
        //                                UsuarioEdicionWeb = x.UsuarioEdicionWeb.Apellido,
        //                                FechaEdicionVT = x.FechaEdicionVT
        //                            });

        //    return Json(stocks.ToArray(), JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetAllStock()
        {
            var operadorID = GetUserOperadorID();
            var stocks = db.Stocks.Where(x => (operadorID == Guid.Empty || x.ArticuloAsignacion.Locacion.OperadorID == operadorID) && x.ArticuloAsignacion.ControlStock==true).ToList().Select(x => StockViewModel.From(x));

            return Json(stocks, JsonRequestBehavior.AllowGet);
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
            var stocks = db.Stocks.Where(x => (operadorID == Guid.Empty
                                            || x.ArticuloAsignacion.Locacion.OperadorID == operadorID) && x.ArticuloAsignacion.ControlStock).ToList()
                                            .Select(x => StockViewModel.From(x)).Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Stocks");
            int amountOfColumns = 0;
            IRow headerRow = sheet.CreateRow(0);

            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Artículo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Zona");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Máquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Cantidad");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Aviso");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Usuario Edición Web");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Edición Web");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Edición VT");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var stock in stocks.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(stock.OperadorNombre);
                row.CreateCell(colIdx++).SetCellValue(stock.Locacion);
                row.CreateCell(colIdx++).SetCellValue(stock.Articulo);
                row.CreateCell(colIdx++).SetCellValue(stock.Zona);
                row.CreateCell(colIdx++).SetCellValue(stock.Maquina);
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(stock.Cantidad));
                row.CreateCell(colIdx++).SetCellValue(stock.FechaAviso.HasValue ? stock.FechaAviso.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(stock.UsuarioEdicionWeb);
                row.CreateCell(colIdx++).SetCellValue(stock.FechaEdicionWeb.HasValue ? stock.FechaEdicionWeb.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(stock.FechaEdicionVT.HasValue ? stock.FechaEdicionVT.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty);

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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Stock " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        // GET: Stocks/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);

            if (stock == null)
            {
                return HttpNotFound();
            }

            StockViewModel viewModel = StockViewModel.From(stock);
            return View(viewModel);
        }

        // GET: Stocks/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock entity = db.Stocks.Find(id);

            if (entity == null)
            {
                return HttpNotFound();
            }
            var viewModel = StockViewModel.From(entity);
            var operadorID = GetUserOperadorID() == Guid.Empty ? entity.ArticuloAsignacion.Locacion.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;


            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => (x.Locacion.OperadorID == operadorID) && x.MaquinaID != null && x.ControlStock==true);
            var locaciones = articulosAsignaciones.Select(x => x.Locacion).Distinct();
            var articulos = articulosAsignaciones.Where(x => x.LocacionID == viewModel.LocacionID).Select(x => x.Articulo).Distinct();
            var zonas = articulosAsignaciones.Where(x => x.ArticuloID == viewModel.ArticuloID && x.LocacionID == viewModel.LocacionID)
            .Select(x => x.Locacion.NombreZona1 == viewModel.Zona ? x.Locacion.NombreZona1 : x.Locacion.NombreZona2 == viewModel.Zona ? x.Locacion.NombreZona2 :
               x.Locacion.NombreZona3 == viewModel.Zona ? x.Locacion.NombreZona3 : x.Locacion.NombreZona4 == viewModel.Zona ? x.Locacion.NombreZona4 :
               x.Locacion.NombreZona5 == viewModel.Zona ? x.Locacion.NombreZona5 : string.Empty

                ).Distinct();
            var maquinas = articulosAsignaciones.Where(x =>
            (x.Locacion.NombreZona1 == viewModel.Zona ? x.Locacion.NombreZona1 : x.Locacion.NombreZona2 == viewModel.Zona ? x.Locacion.NombreZona2 :
                x.Locacion.NombreZona3 == viewModel.Zona ? x.Locacion.NombreZona3 : x.Locacion.NombreZona4 == viewModel.Zona ? x.Locacion.NombreZona4 :
                x.Locacion.NombreZona5 == viewModel.Zona ? x.Locacion.NombreZona5 : string.Empty)
            == viewModel.Zona && x.ArticuloID == viewModel.ArticuloID
                           && x.LocacionID == viewModel.LocacionID && x.MaquinaID != null)
                                                .Select(x => x.Maquina).Distinct();

            viewModel.LocacionList = new SelectList(locaciones, "LocacionID", "Nombre", viewModel.LocacionID);
            viewModel.ArticuloList = new SelectList(articulos, "ArticuloID", "Nombre", viewModel.ArticuloID);
            viewModel.ZonaList = new SelectList(zonas, viewModel.Zona);
            //viewModel.MaquinaList = new SelectList(maquinas, "MaquinaID", "NombreAlias", viewModel.MaquinaID);
            viewModel.MaquinaList = new SelectList(maquinas.ToList().Select(y => new
            {
                MaquinaID = y.MaquinaID,
                texto = y.NombreAlias != null ? y.MarcaModelo.MarcaModeloNombre + " - " + y.NumeroSerie + '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
            }), "MaquinaID", "texto");

            return View(viewModel);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StockViewModel stockViewModel)
        {
            if (stockViewModel.Capacidad.HasValue && stockViewModel.Cantidad > stockViewModel.Capacidad.Value)
            {
                ModelState.AddModelError(string.Empty, "La cantidad de stock es mayor a la capacidad del mismo");
            }

            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();
                var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

                var command = new CreateStockCommand();
                command.Configure(stockViewModel.StockID, stockViewModel.ArticuloAsignacionID, stockViewModel.Cantidad, db, userId).Execute();

                return RedirectToAction("Index");
            }

            Guid opid = db.Stocks.Find(stockViewModel.StockID).ArticuloAsignacion.Locacion.OperadorID; 
            var operadorID = GetUserOperadorID() == Guid.Empty ? opid : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;

            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => (x.Locacion.OperadorID == operadorID) && x.MaquinaID != null && x.ControlStock==true);
            var locaciones = articulosAsignaciones.Select(x => x.Locacion).Distinct();
            var articulos = articulosAsignaciones.Where(x => x.LocacionID == stockViewModel.LocacionID).Select(x => x.Articulo).Distinct();

            stockViewModel.LocacionList = new SelectList(locaciones, "LocacionID", "Nombre", stockViewModel.LocacionID);
            stockViewModel.ArticuloList = new SelectList(articulos, "ArticuloID", "Nombre", stockViewModel.ArticuloID);

            if (stockViewModel.Zona != null)
            {
                var zonas = articulosAsignaciones.Where(x => x.ArticuloID == stockViewModel.ArticuloID && x.LocacionID == stockViewModel.LocacionID)
               .Select(x => x.Locacion.NombreZona1 == stockViewModel.Zona ? x.Locacion.NombreZona1 : x.Locacion.NombreZona2 == stockViewModel.Zona ? x.Locacion.NombreZona2 :
                  x.Locacion.NombreZona3 == stockViewModel.Zona ? x.Locacion.NombreZona3 : x.Locacion.NombreZona4 == stockViewModel.Zona ? x.Locacion.NombreZona4 :
                  x.Locacion.NombreZona5 == stockViewModel.Zona ? x.Locacion.NombreZona5 : string.Empty
                   ).Distinct();
                stockViewModel.ZonaList = new SelectList(zonas, stockViewModel.Zona);
            }
            else
            {
                var zonas = articulosAsignaciones.Where(x => x.ArticuloID == stockViewModel.ArticuloID && x.LocacionID == stockViewModel.LocacionID).Select(x => x.Locacion).ToList();
                List<SelectListItem> lista = new List<SelectListItem>();
                foreach (var item in zonas)
                {
                    if (item.NombreZona1 != null) lista.Add(new SelectListItem { Text = item.NombreZona1, Value = item.NombreZona1 });
                    if (item.NombreZona2 != null) lista.Add(new SelectListItem { Text = item.NombreZona2, Value = item.NombreZona2 });
                    if (item.NombreZona3 != null) lista.Add(new SelectListItem { Text = item.NombreZona3, Value = item.NombreZona3 });
                    if (item.NombreZona4 != null) lista.Add(new SelectListItem { Text = item.NombreZona4, Value = item.NombreZona4 });
                    if (item.NombreZona5 != null) lista.Add(new SelectListItem { Text = item.NombreZona5, Value = item.NombreZona5 });
                }

                stockViewModel.ZonaList = new SelectList(lista.Select(x => x.Text).Distinct(), stockViewModel.Zona);
            }
            if (stockViewModel.MaquinaID != null)
            {
                var maquinas = articulosAsignaciones.Where(x =>
                (x.Locacion.NombreZona1 == stockViewModel.Zona ? x.Locacion.NombreZona1 : x.Locacion.NombreZona2 == stockViewModel.Zona ? x.Locacion.NombreZona2 :
                    x.Locacion.NombreZona3 == stockViewModel.Zona ? x.Locacion.NombreZona3 : x.Locacion.NombreZona4 == stockViewModel.Zona ? x.Locacion.NombreZona4 :
                    x.Locacion.NombreZona5 == stockViewModel.Zona ? x.Locacion.NombreZona5 : string.Empty)
                == stockViewModel.Zona && x.ArticuloID == stockViewModel.ArticuloID
                               && x.LocacionID == stockViewModel.LocacionID && x.MaquinaID != null)
                                                    .Select(x => x.Maquina).Distinct();

                stockViewModel.MaquinaList = new SelectList(maquinas.ToList().Select(y => new
                {
                    MaquinaID = y.MaquinaID,
                    texto = y.NombreAlias != null ? y.MarcaModelo.MarcaModeloNombre + " - " + y.NumeroSerie + '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
                }), "MaquinaID", "texto", stockViewModel.MaquinaID);
            }
            else
            {
                var maquinas = articulosAsignaciones.Where(x => x.ArticuloID == stockViewModel.ArticuloID
                           && x.LocacionID == stockViewModel.LocacionID && x.MaquinaID != null).Select(x => x.Maquina).Distinct();
                stockViewModel.MaquinaList = new SelectList(maquinas.ToList().Select(y => new
                {
                    MaquinaID = y.MaquinaID,
                    texto = y.NombreAlias != null ? y.MarcaModelo.MarcaModeloNombre + " - " + y.NumeroSerie + '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
                }), "MaquinaID", "texto", stockViewModel.MaquinaID);

            }

            return View(stockViewModel);
        }

        // GET: Stocks/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);

            if (stock == null)
            {
                return HttpNotFound();
            }

            StockViewModel viewModel = StockViewModel.From(stock);
            return View(viewModel);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Stock stock = db.Stocks.Find(id);

            var stockHistorico = db.StocksHistoricos.Where(x => x.StockID == stock.StockID);
            db.StocksHistoricos.RemoveRange(stockHistorico);
            db.Stocks.Remove(stock);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetLocacionesAsignacion(string operadorID)
        {
            Guid operadorGuid = new Guid(operadorID);
            List<SelectListItem> locaciones = new List<SelectListItem>();
            var lista = db.ArticulosAsignaciones.Where(x => (operadorGuid == Guid.Empty || x.Locacion.OperadorID == operadorGuid) && x.MaquinaID != null && x.ControlStock==true).
                        Select(x => x.Locacion).Distinct();

            foreach (var item in lista)
            {
                locaciones.Add(new SelectListItem { Text = item.Nombre, Value = item.LocacionID.ToString() });
            }

            return Json(new SelectList(locaciones, "Value", "Text"));
        }

        public JsonResult GetArticulosByLocacion(string locacionId)
        {
            Guid locacionGuid = new Guid(locacionId);
            List<SelectListItem> articulos = new List<SelectListItem>();
            var lista = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.MaquinaID != null && x.ControlStock==true).
                        Select(x => x.Articulo).Distinct();

            foreach (var item in lista)
            {
                articulos.Add(new SelectListItem { Text = item.Nombre, Value = item.ArticuloID.ToString() });
            }

            return Json(new SelectList(articulos, "Value", "Text"));
        }

        public JsonResult GetMaquinasByLocacionAndArticuloAndZona(string articuloID, string locacionID, string ZonaNro)
        {
            List<SelectListItem> maquinas = new List<SelectListItem>();
            Guid articuloGuid = new Guid(articuloID);
            Guid locacionGuid = new Guid(locacionID);

            var lista = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID == articuloGuid
                        &&
                                                (x.NroZona.HasValue ? (x.NroZona.Value == 1 ? x.Locacion.NombreZona1 : x.NroZona.Value == 2 ? x.Locacion.NombreZona2 :
                                                                            x.NroZona.Value == 3 ? x.Locacion.NombreZona3 : x.NroZona.Value == 4 ? x.Locacion.NombreZona4 :
                                                                            x.NroZona.Value == 5 ? x.Locacion.NombreZona5 : string.Empty) : string.Empty)
                                                == ZonaNro
                        && x.MaquinaID != null && x.ControlStock==true).Select(x => new { nombreMaquina = x.Maquina.NombreAlias, maquinaID = x.MaquinaID }).Distinct();

            foreach (var item in lista)
            {
                maquinas.Add(new SelectListItem { Text = item.nombreMaquina, Value = item.maquinaID.ToString() });
            }

            return Json(new SelectList(maquinas, "Value", "Text"));
        }

        public JsonResult GetZonasByLocacionAndArticulo(string articuloID, string locacionID)
        {
            Guid articuloGuid = new Guid(articuloID);
            Guid locacionGuid = new Guid(locacionID);
            List<SelectListItem> zonas = new List<SelectListItem>();

            var lista = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID == articuloGuid && x.MaquinaID != null && x.ControlStock==true).
                        Select(x => new
                        {
                            Text = x.NroZona.HasValue ? (x.NroZona.Value == 1 ? x.Locacion.NombreZona1 : x.NroZona.Value == 2 ? x.Locacion.NombreZona2 :
                                            x.NroZona.Value == 3 ? x.Locacion.NombreZona3 : x.NroZona.Value == 4 ? x.Locacion.NombreZona4 :
                                            x.NroZona.Value == 5 ? x.Locacion.NombreZona5 : string.Empty) : string.Empty
                        }
                        ).Distinct();

            foreach (var item in lista)
            {
                zonas.Add(new SelectListItem { Text = item.Text, Value = item.Text });
            }

            return Json(new SelectList(zonas, "Value", "Text"));
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

        //public string GetNombreZonaByNroZona(int nroZona)
        //{
        //    string nombreZona = 
        //    return 
        //}

        public JsonResult GetArticuloAsignacionID(string articuloID, string locacionID, string ZonaNro, string maquinaID)
        {
            Guid articuloGuid = new Guid(articuloID);
            Guid locacionGuid = new Guid(locacionID);
            Guid maquinaGuid = new Guid(maquinaID);
            //  int zonaNro = Convert.ToInt32(ZonaNro);
            List<SelectListItem> articulosAsignaciones = new List<SelectListItem>();

            var articuloAsignacion = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID == articuloGuid
                        &&
                        (x.NroZona.HasValue ? (x.NroZona.Value == 1 ? x.Locacion.NombreZona1 : x.NroZona.Value == 2 ? x.Locacion.NombreZona2 :
                                    x.NroZona.Value == 3 ? x.Locacion.NombreZona3 : x.NroZona.Value == 4 ? x.Locacion.NombreZona4 :
                                    x.NroZona.Value == 5 ? x.Locacion.NombreZona5 : string.Empty) : string.Empty)
                                    == ZonaNro
            && x.MaquinaID == maquinaGuid && x.ControlStock==true).FirstOrDefault();

            //var articuloAsignacion = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID == articuloGuid && x.NroZona == zonaNro
            //                                    && x.MaquinaID == maquinaGuid).FirstOrDefault();

            Stock stock = articuloAsignacion.Stocks.FirstOrDefault();

            return Json(new
            {
                ArticuloAsignacionID = articuloAsignacion.Id,
                StockID = (stock != null) ? stock.StockID : Guid.Empty,
                Cantidad = (stock != null) ? stock.Cantidad : 0,
                Capacidad = articuloAsignacion.Capacidad
            }, JsonRequestBehavior.AllowGet);
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
