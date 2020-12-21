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

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class StockHistoricoController : BaseController
    {
        private BugsContext db = new BugsContext();
        Guid tipoMovimientoReposicionID = Guid.Parse(Constantes.Reposicion);
        Guid tipoMovimientoNuevoID = Guid.Parse(Constantes.Nuevo);
        // GET: StockHistorico
        [AuthorizeUser(accion = "Index", controlador = "StockHistorico")]
        public ActionResult Index()
        {
            var operadorID = GetUserOperadorID();
            var view = db.StocksHistoricos.Where(x => operadorID == Guid.Empty || operadorID== x.Stock.ArticuloAsignacion.Locacion.OperadorID);
             return View();
        }

        public JsonResult GetAllStockHistorico()
        {
            var operadorID = GetUserOperadorID();
            

            var stockHistoricos = db.StocksHistoricos.Where(x => (operadorID == Guid.Empty || 
                operadorID == x.Stock.ArticuloAsignacion.Locacion.OperadorID) &&
                (x.TipoDeMovimientoID == tipoMovimientoReposicionID || x.TipoDeMovimientoID == tipoMovimientoNuevoID));


            return Json(stockHistoricos.OrderByDescending(x=> x.Fecha).ToList().Select(x => new
            {
                StockHistoricoID = x.StockHistoricoID,
                StockID = x.StockID.HasValue ? x.StockID.Value.ToString() : String.Empty,
                OperadorID = x.Stock.ArticuloAsignacion.Locacion.Operador.Nombre,

                Locacion = x.Stock.ArticuloAsignacion.Locacion.Nombre,
                Articulo = x.Stock.ArticuloAsignacion.Articulo.Nombre,
                Zona = x.Stock.ArticuloAsignacion.NroZona.HasValue ? (x.Stock.ArticuloAsignacion.NroZona.Value == 1 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona1 : x.Stock.ArticuloAsignacion.NroZona.Value == 2 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona2 :
                            x.Stock.ArticuloAsignacion.NroZona.Value == 3 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona3 : x.Stock.ArticuloAsignacion.NroZona.Value == 4 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona4 :
                            x.Stock.ArticuloAsignacion.NroZona.Value == 5 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona5 : string.Empty) : string.Empty,

            Maquina = x.Stock.ArticuloAsignacion.Maquina.NombreAlias != null ? x.Stock.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + " - " + x.Stock.ArticuloAsignacion.Maquina.NumeroSerie + '(' + x.Stock.ArticuloAsignacion.Maquina.NombreAlias + ')' : x.Stock.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + " - " + x.Stock.ArticuloAsignacion.Maquina.NumeroSerie,
                TipoDeMovimientoID = x.TipoDeMovimiento.Nombre,
                UsuarioID = x.UsuarioID.HasValue ? x.Usuario.ApplicationUsers.FirstOrDefault().Email : String.Empty,
                Fecha = x.Fecha,
                FechaAviso = x.FechaAviso,
                Cantidad = x.Cantidad
            }).ToArray(), JsonRequestBehavior.AllowGet);
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

            IQueryable<StockHistorico> stockHistoricos = db.StocksHistoricos.Where(x => (operadorID == Guid.Empty ||
                operadorID == x.Stock.ArticuloAsignacion.Locacion.OperadorID) &&
                (x.TipoDeMovimientoID == tipoMovimientoReposicionID || x.TipoDeMovimientoID == tipoMovimientoNuevoID));

            var stocksHistoricos = stockHistoricos.OrderByDescending(x => x.Fecha).Select(x => new
                {
                    Operador = x.Stock.ArticuloAsignacion.Locacion.Operador.Nombre,
                    Locacion = x.Stock.ArticuloAsignacion.Locacion.Nombre,
                    Articulo = x.Stock.ArticuloAsignacion.Articulo.Nombre,
                Zona = x.Stock.ArticuloAsignacion.NroZona.HasValue ? (x.Stock.ArticuloAsignacion.NroZona.Value == 1 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona1 : x.Stock.ArticuloAsignacion.NroZona.Value == 2 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona2 :
                            x.Stock.ArticuloAsignacion.NroZona.Value == 3 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona3 : x.Stock.ArticuloAsignacion.NroZona.Value == 4 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona4 :
                            x.Stock.ArticuloAsignacion.NroZona.Value == 5 ? x.Stock.ArticuloAsignacion.Locacion.NombreZona5 : string.Empty) : string.Empty,
                Maquina = x.Stock.ArticuloAsignacion.Maquina.NombreAlias != null ? x.Stock.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + " - " + x.Stock.ArticuloAsignacion.Maquina.NumeroSerie + "(" + x.Stock.ArticuloAsignacion.Maquina.NombreAlias + ")" : x.Stock.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + "-" + x.Stock.ArticuloAsignacion.Maquina.NumeroSerie,
                TipoDeMovimientoID = x.TipoDeMovimiento.Nombre,
                    Usuario = x.UsuarioID.HasValue ? x.Usuario.ApplicationUsers.FirstOrDefault().Email : String.Empty,
                    Fecha = x.Fecha,
                    FechaAviso = x.FechaAviso,
                    Cantidad = x.Cantidad
                }).Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Stocks");
            int amountOfColumns = 0;
            IRow headerRow = sheet.CreateRow(0);

            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Aviso");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Artículo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Zona");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Máquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Tipo de Movimiento");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Cantidad");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Usuario");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var item in stocksHistoricos.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(item.Operador);
                row.CreateCell(colIdx++).SetCellValue(item.Fecha.ToString("dd/MM/yyyy HH:mm:ss"));
                row.CreateCell(colIdx++).SetCellValue(item.FechaAviso.HasValue? item.FechaAviso.Value.ToString("dd/MM/yyyy HH:mm:ss"):string.Empty);
                row.CreateCell(colIdx++).SetCellValue(item.Locacion);
                row.CreateCell(colIdx++).SetCellValue(item.Articulo);
                row.CreateCell(colIdx++).SetCellValue(item.Zona);
                row.CreateCell(colIdx++).SetCellValue(item.Maquina);
                row.CreateCell(colIdx++).SetCellValue(item.TipoDeMovimientoID);
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(item.Cantidad));
                row.CreateCell(colIdx++).SetCellValue(item.Usuario);

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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Reposiciones " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
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
    }
}