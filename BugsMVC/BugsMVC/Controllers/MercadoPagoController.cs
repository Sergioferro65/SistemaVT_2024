﻿using BugsMVC.DAL;
using BugsMVC.Handlers;
using BugsMVC.Helpers;
using BugsMVC.Models;
using BugsMVC.Models.ViewModels;
using BugsMVC.Security;
using MercadoPago.Resources;
using MercadoPago.Common;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using BugsMVC.Models;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class MercadoPagoController : BaseController
    {
        private static readonly log4net.ILog Log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BugsContext db = new BugsContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Configure()
        {
            var operadorID = GetUserOperadorID();
            Operador operador = db.Operadores.Where(x => x.OperadorID == operadorID).FirstOrDefault();
            MercadoPagoConfiguracionViewModel viewModel = new MercadoPagoConfiguracionViewModel();

            if (operador != null)
                viewModel = MercadoPagoConfiguracionViewModel.From(operador);

            return View(viewModel);
        }

        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(MercadoPagoConfiguracionViewModel viewModel)
        {
            var operadorID = GetUserOperadorID();
            Operador operador = db.Operadores.Where(x => x.OperadorID == operadorID).FirstOrDefault();

            viewModel.ToEntity(operador);
            db.Entry(operador).State = EntityState.Modified;
            db.SaveChanges();
            //MercadoPagoConfiguracionViewModel viewModel = new MercadoPagoConfiguracionViewModel();

            return RedirectToAction("ConfigureSuccess");
            //return View(viewModel);
        }

        public ActionResult ConfigureSuccess()
        {
            return View();
        }


        public ActionResult DeleteRange()
        {
            MercadoPagoDeleteRangeViewModel viewModel = new MercadoPagoDeleteRangeViewModel();
            return View(viewModel);
        }

        [Audit]
        [HttpPost]
        public ActionResult DeleteRange(MercadoPagoDeleteRangeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.FechaHasta = viewModel.FechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
                Guid operadorID = GetUserOperadorID();

                List<MercadoPagoTable> mercadoPagoTables = db.MercadoPago.Where(x => x.Fecha != null &&
                x.Fecha >= viewModel.FechaDesde &&
                x.Fecha <= viewModel.FechaHasta &&
                (operadorID == Guid.Empty || x.Maquina.OperadorID == operadorID)).ToList();

                db.MercadoPago.RemoveRange(mercadoPagoTables);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }


        public JsonResult DevolverDinero(string Comprobante)
        {

            try
            {

                MercadoPagoTable entity = db.MercadoPago.Where(x => x.Comprobante == Comprobante).First();

                MercadoPago.SDK.CleanConfiguration();
                MercadoPago.SDK.ClientId = entity.Maquina.Operador.ClientId;
                MercadoPago.SDK.ClientSecret = entity.Maquina.Operador.SecretToken;

                Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == entity.MaquinaId).FirstOrDefault();
                Operador operador = db.Operadores.Where(x => x.OperadorID == maquina.OperadorID).FirstOrDefault();

                if (entity != null)
                {
                    long id = 0;
                    long.TryParse(entity.Comprobante, out id);
                    Payment payment = Payment.FindById(id);
                    if (payment.Errors == null)
                    {
                        payment.Refund();

                        if (payment.Status == PaymentStatus.approved)
                        {
                        

                            entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;

                            db.Entry(entity).State = EntityState.Modified;
                            db.SaveChanges();

                            return Json("", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("not approved", JsonRequestBehavior.DenyGet);
                        }
                    }
                    else {
                        return Json("not found", JsonRequestBehavior.DenyGet);
                    }

                }
                else {
                    return Json("Not Found", JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {    
                Log.Error("Hubo un error  devolver Dinero", ex);
                return Json("Not Found", JsonRequestBehavior.DenyGet);
            }

        }

        //public JsonResult GetAllMercadoPagos()
        //{
        //    var operadorID = GetUserOperadorID();
        //    var mercadoPagoViewModel = db.MercadoPago
        //        .Where(x => operadorID == Guid.Empty || x.Maquina.OperadorID == operadorID)
        //        .ToList()
        //        .OrderByDescending(x => x.Fecha)
        //        .Select(x => MercadoPagoViewModel.From(
        //            x,
        //            (x.Maquina != null) ? x.Maquina.Terminal.NumeroSerie.ToString() : " Terminal NO Registrada",
        //            (x.Maquina != null && x.Maquina.Locacion != null) ? x.Maquina.Locacion.Nombre.ToString() : "Locación NO Registrada."
        //        ));

        //    return Json(mercadoPagoViewModel, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetAllMercadoPagos(int rows, int page = 1, int pageSize = 20)
        {
            var operadorID = GetUserOperadorID();

            var query = db.MercadoPago
                .Where(x => operadorID == Guid.Empty || x.Maquina.OperadorID == operadorID)
                .OrderByDescending(x => x.Fecha);

            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);


            var mercadoPagoList = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(x => MercadoPagoViewModel.From(
                    x,
                    (x.Maquina != null) ? x.Maquina.Terminal.NumeroSerie.ToString() : " Terminal NO Registrada",
                    (x.Maquina != null && x.Maquina.Locacion != null) ? x.Maquina.Locacion.Nombre.ToString() : "Locación NO Registrada."
                ));

            TempData.Keep();

            int pageIndex = Convert.ToInt32(page) - 1;
            pageSize = rows;

            //mercadoPagoList = mercadoPagoList.Skip(pageIndex * pageSize).Take(pageSize);

            var result = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = mercadoPagoList
            };


            return Json(result, JsonRequestBehavior.AllowGet);
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

            var operadorID = GetUserOperadorID();
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");

            var mercadoPagos = db.MercadoPago.Where(x => operadorID == Guid.Empty || x.Maquina.OperadorID == operadorID)
                            .Select(x => new
                            {
                                Operador = x.Maquina.Operador.Nombre,
                                Comprobante = x.Comprobante ,
                                Maquina = x.Maquina.NumeroSerie, //x.Maquina.NombreAlias != null ? x.Maquina.MarcaModelo.MarcaModeloNombre + " - " + x.Maquina.NumeroSerie + "(" + x.Maquina.NombreAlias + ")" : x.Maquina.MarcaModelo.MarcaModeloNombre + "-" + x.Maquina.NumeroSerie,
                                EstadoTransmision = x.MercadoPagoEstadoTransmision.Descripcion,
                                EstadoFinanciero = x.MercadoPagoEstadoFinanciero.Descripcion,
                                IdCajaMP = x.Maquina.NotasService,
                                Locacion = (x.Maquina != null && x.Maquina.Locacion != null) ? x.Maquina.Locacion.Nombre.ToString() : "Locación NO Registrada.",
                                NroSerieTerminal = (x.Maquina != null) ? x.Maquina.Terminal.NumeroSerie.ToString() : " Terminal NO Registrada",
                                Monto = x.Monto,
                                Fecha = x.Fecha,
                                Entidad = x.Entidad,
                                x.Descripcion,
                            })
                            .Where(filters);

            //Create new Excel workbook
            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            // Create new Excel sheet
            ISheet sheet = workbook.CreateSheet("Máquinas");

            int amountOfColumns = 0;

            // Create a header row
            IRow headerRow = sheet.CreateRow(0);

            // Set the column names in the header row
            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Comprobante");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("N° Serie Maquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Estado Transmisión");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Estado Financiero");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Id Caja MP");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locacion");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nro Serie Terminal");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Entidad");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descripción");

            // Define a cell style for the header row values
            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            // Apply the cell style to header row cells
            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            // First row for data
            var rowNumber = 1;
            int colIdx;

            // Define a default cell style for values
            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            // Define a cell style for Date values
            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            // Populate the sheet with values from the grid data
            foreach (var mercadoPago in mercadoPagos.ToList())
            {
                // Create a new row
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                // Set values for the cells
                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(mercadoPago.Operador);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Comprobante );
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Maquina);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.EstadoTransmision);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.EstadoFinanciero);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.IdCajaMP);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Locacion);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.NroSerieTerminal);
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(mercadoPago.Monto));
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Fecha.ToString("dd/MM/yyyy HH:mm"));
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Entidad );
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Descripcion);

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

            // About width units for columns: 28 * 256 is equivalent to 200px            
            for (int i = 0; i < amountOfColumns; i++)
            {
                sheet.AutoSizeColumn(i);
                // Add approx 8px to width
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1 * 256);
            }

            // Make adjustments for specific columns
            //sheet.SetColumnWidth(0, 10 * 256);

            // Write the workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Pagos Externos " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
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