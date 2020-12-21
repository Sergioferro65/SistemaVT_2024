using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugsMVC.DAL;
using BugsMVC.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using System.Linq.Dynamic;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using BugsMVC.Security;
using BugsMVC.Models.ViewModels;
using BugsMVC.Handlers;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class ConsumidorController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: Transaccion
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MiCuenta()
        {
            Guid operadorID = GetUserOperadorID();
            Guid userID = ViewHelper.GetCurrentUsuarioId();
            Usuario usuario = db.Usuarios.Find(userID);
            var transacciones = db.Transacciones.Where(x => x.Usuario.UsuarioID == userID).ToList();
            ConsumidorViewModel viewModel = ConsumidorViewModel.From(usuario,transacciones).WithLocacion(usuario.Locacion);

            return View(viewModel);
        }


        public JsonResult GetAllConsumos()
        {
            Guid operadorID = GetUserOperadorID();
            Guid userID = ViewHelper.GetCurrentUsuarioId();
            var transacciones = db.Transacciones.Where(x => x.Usuario.UsuarioID == userID && x.FechaTransaccion.HasValue && x.FechaTransaccion.Value.Month == DateTime.Now.Month && x.FechaTransaccion.Value.Year == DateTime.Now.Year).ToList()
                                                    .Select(x => new
                                                    {
                                                        TransaccionID = x.TransaccionID,
                                                        FechaTransaccion = x.FechaTransaccion,
                                                        TransaccionTexto = x.TransaccionTexto != null ? x.TransaccionTexto.TextoTransaccion : string.Empty,
                                                        ValorVenta = x.ValorVenta,
                                                        ValorRecarga = x.ValorRecarga,
                                                        nroSerie = (x.Maquina != null) ? x.Maquina.NumeroSerie : string.Empty,
                                                        //Articulo = (x.Articulo != null) ? x.Articulo.Nombre : "Artículo " + x.ValorVenta.ToString(),
                                                        //Locacion = (x.Locacion != null) ? x.Locacion.Nombre : "Locación NO Registrada.",
                                                        //Roles = String.Join(",", (from Rol in x.Usuario.ApplicationUsers.FirstOrDefault().Roles
                                                        //                          join j in db.Roles
                                                        //                            on Rol.RoleId equals j.Id
                                                        //                        select j.Name).Select(y => y))
                                                    });

            return Json(transacciones.ToArray(), JsonRequestBehavior.AllowGet);
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

                    if (i > 0)
                    {
                        filters += " && ";
                    }
                    else
                    {
                        filters = string.Empty;
                    }

                    if (col == "FechaTransaccion" || col == "FechaAltaBase")
                    {
                        //filters += " " + col + "(\"" + data + "\") ";
                    }
                    else
                    {
                        filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";
                    }
                }
            }
            
           // Guid operadorID = GetUserOperadorID();

            Guid userID = ViewHelper.GetCurrentUsuarioId();
            //var transacciones = db.Transacciones.Where(x => x.Usuario.UsuarioID == userID)
            //    .Select(x => new
            //    {
            //        TransaccionID = x.TransaccionID,
            //        FechaTransaccion = x.FechaTransaccion,
            //        ValorVenta = x.ValorVenta,
            //        Articulo = (x.Articulo != null) ? x.Articulo.Nombre : "Artículo " + x.ValorVenta.ToString(),
            //        Locacion = (x.Locacion != null) ? x.Locacion.Nombre : "Locación NO Registrada."
            //    })
            //.Where(filters);

            var transacciones = db.Transacciones.Where(x => x.Usuario.UsuarioID == userID)
            .Select(x => new
            {
                TransaccionID = x.TransaccionID,
                FechaTransaccion = x.FechaTransaccion,
                TransaccionTexto = x.TransaccionTexto != null ? x.TransaccionTexto.TextoTransaccion : " - ",
                ValorVenta = x.ValorVenta,
                ValorRecarga = x.ValorRecarga,
                nroSerie = (x.Maquina != null) ? x.Maquina.NumeroSerie : "Nro de máquina (serie) NO registrada.",
            })
        .Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            ISheet sheet = workbook.CreateSheet("Listado de movimientos del mes");
            int amountOfColumns = 0;
            IRow headerRow = sheet.CreateRow(0);

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Transacción");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Transacción Texto");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Valor Venta");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Valor Recarga");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nro Serie Máquina");
            


            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var transaccion in transacciones.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                row.CreateCell(colIdx++).SetCellValue(transaccion.FechaTransaccion.HasValue ? transaccion.FechaTransaccion.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(transaccion.TransaccionTexto);
                row.CreateCell(colIdx++, CellType.Numeric).SetCellValue(Convert.ToDouble(transaccion.ValorVenta));
                row.CreateCell(colIdx++, CellType.Numeric).SetCellValue(Convert.ToDouble(transaccion.ValorRecarga));
                row.CreateCell(colIdx++).SetCellValue(transaccion.nroSerie);

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
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 2 * 256);
            }

            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Consumos " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
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
