using BugsMVC.DAL;
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
using System.Net;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using log4net.Filter;
using Newtonsoft.Json.Linq;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class MercadoPagoController : BaseController
    {
        private static readonly log4net.ILog Log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BugsContext db = new BugsContext();

        public ActionResult Index(bool clsSession = true)
        {
            if (clsSession)
            {
                TempData.Clear();
                Session[JQGridFilterRule.Keys.MercadoPago.FILTERS] = null;
                Session[JQGridFilterRule.Keys.MercadoPago.SIDX] = null;
            }

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


        [AuthorizeUser(accion = "ClearSession", controlador = "Usuario")]
        public ActionResult ClearSession()
        {
            Session[JQGridFilterRule.Keys.MercadoPago.FILTERS] = null;
            Session[JQGridFilterRule.Keys.MercadoPago.SORD] = null;
            Session[JQGridFilterRule.Keys.MercadoPago.SIDX] = null;
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Delete(long id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MercadoPagoTable entity = db.MercadoPago.Find(id);

            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        // POST: Operador/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();

            MercadoPagoTable entity = db.MercadoPago.Find(id);
            db.MercadoPago.Remove(entity);
            db.SaveChanges();

            return RedirectToAction("Index");
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

        public JsonResult GetAllMercadoPagos(string sidx, string sord, int rows, int page = 1, int pageSize = 20, string filters="")
        {
            var operadorID = GetUserOperadorID();
            var sessionFilters = Session[JQGridFilterRule.Keys.MercadoPago.FILTERS] as List<Filter>;
            var sessionSort = Session[JQGridFilterRule.Keys.MercadoPago.SIDX] as Dictionary<string, string>;
            bool hasFilter = false;
            bool hasSession = false;
            bool hasSortSession = false;

            if (sessionFilters != null && sessionFilters.Any())
                hasSession = true;

            if (!string.IsNullOrEmpty(filters))
                hasFilter = true;

            if (sessionSort != null && sessionSort.Any())
                hasSortSession = true;

            IQueryable<MercadoPagoTable> query = db.MercadoPago
            .Where(x => operadorID == Guid.Empty || x.Maquina.OperadorID == operadorID)
            .OrderByDescending(x => x.Fecha);

            //var totalRecords = query.Count();
            //var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            List<Filter> parsedFilters = new List<Filter>();

            if (hasFilter)
            {//Unicamente filters viene vacio en la primer carga.

                JObject data = JsonConvert.DeserializeObject<JObject>(filters);
                var itemFilters = data.Last.Values().Select(x => x.ToObject<Filter>()).ToList();
                parsedFilters = itemFilters;
                Session[JQGridFilterRule.Keys.MercadoPago.FILTERS] = parsedFilters;
                TempData.Clear();

            }
            else if (hasSession)
            {
                //Si existia session.
                parsedFilters = sessionFilters;
                Session[JQGridFilterRule.Keys.MercadoPago.FILTERS] = null;
            }

            string EstadoTransmision = "";
            string EstadoFinanciero = "";
            string NroSerieTerminal = "";
            string MaquinaDescripcion = "";
            string NotaService = "";
            string Locacion = "";

            foreach (var item in parsedFilters)
            {

                TempData[item.field] = item.data;

                switch (item.field.ToString())
                {
                    case "MercadoPagoEstadoTransmisionDescripcion":
                        EstadoTransmision = item.data;
                        break;
                    case "MercadoPagoEstadoFinancieroDescripcion":
                        EstadoFinanciero = item.data;
                        break;
                    case "NroSerieTerminal":
                        NroSerieTerminal = item.data;
                        break;
                    case "MaquinaDescripcion":
                        MaquinaDescripcion = item.data;
                        break;
                    case "NotaService":
                        NotaService = item.data;
                        break;
                    case "Locacion":
                        Locacion = item.data;
                        break;
                }

                if (item.field == "Comprobante"  ||
                    item.field == "Entidad" || item.field == "Descripcion")
                {
                    //Si es texto.
                    query = query.Where(string.Format("{0}.ToUpper().Contains(@0)", item.field), item.data.ToString());
                }
                else if (item.field == "Monto" || item.field == "DescuentoAplicado")
                {
                    //Si es decimal
                    query = query.Where(string.Format("{0} = @0", item.field), Convert.ToDecimal(item.data));
                }
                else if (item.field == "Fecha" )
                {
                    //Si es fecha
                    DateTime fech = Convert.ToDateTime(item.data);
                    query = query.Where(o => o.Fecha.Year == fech.Year && o.Fecha.Month == fech.Month && o.Fecha.Day == fech.Day);
                }

            }

            var mercadoPagoList = query
            .ToList()
            .Select(x => MercadoPagoViewModel.From(
                x,
                (x.Maquina != null) ? x.Maquina.Terminal.NumeroSerie.ToString() : " Terminal NO Registrada",
                (x.Maquina != null && x.Maquina.Locacion != null) ? x.Maquina.Locacion.Nombre.ToString() : "Locación NO Registrada."
            ))
            .Where(vm => string.IsNullOrEmpty(NroSerieTerminal) || vm.NroSerieTerminal == NroSerieTerminal)
            .Where(vm => string.IsNullOrEmpty(MaquinaDescripcion) || vm.MaquinaDescripcion == MaquinaDescripcion)
            .Where(vm => string.IsNullOrEmpty(Locacion) || vm.Locacion.ToLower() == Locacion.ToLower())
 
              .Where(vm => vm.NotaService != null &&
                     vm.NotaService.IndexOf(NotaService, StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(vm =>
                     vm.MercadoPagoEstadoFinancieroDescripcion != null &&
                     vm.MercadoPagoEstadoFinancieroDescripcion.IndexOf(EstadoFinanciero, StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(vm =>
              vm.MercadoPagoEstadoTransmisionDescripcion != null &&
              vm.MercadoPagoEstadoTransmisionDescripcion.IndexOf(EstadoTransmision, StringComparison.OrdinalIgnoreCase) >= 0);

            //ordenamiento

            if (sidx != "" && sidx != null || hasSortSession)
            {

                if (sord!="")
                {
                    Dictionary<string, string> sorted = new Dictionary<string, string>();
                    sorted.Add(sidx, sord);
                    Session[JQGridFilterRule.Keys.MercadoPago.SIDX] = sorted;
                }
                //else if (sidx == "" || sidx == null)
                //{
                //    sidx = sessionSort.First().Key;
                //    sord = sessionSort.First().Value;
                //}


                switch (sidx)
                {
                    case "OperadorNombre":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.OperadorNombre);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.OperadorNombre);
                        break;

                    case "Comprobante":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.Comprobante);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.Comprobante);
                        break;


                    case "Monto":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.Monto);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.Monto);
                        break;

                    case "Fecha":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.Fecha);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.Fecha);
                        break;

                    case "MercadoPagoEstadoTransmisionDescripcion":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.MercadoPagoEstadoTransmisionDescripcion);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.MercadoPagoEstadoTransmisionDescripcion);
                        break;

                    case "MercadoPagoEstadoFinancieroDescripcion":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.MercadoPagoEstadoFinancieroDescripcion);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.MercadoPagoEstadoFinancieroDescripcion);
                        break;

                    case "NroSerieTerminal":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.NroSerieTerminal);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.NroSerieTerminal);
                        break;

                    case "MaquinaDescripcion":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.MaquinaDescripcion);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.MaquinaDescripcion);
                        break;

                    case "NotaService":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.NotaService);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.NotaService);
                        break;

                    case "Locacion":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.Locacion);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.Locacion);
                        break;

                    case "Entidad":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.Entidad);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.Entidad);
                        break;

                    case "Descripcion":
                        if (sord == "asc")
                            mercadoPagoList = mercadoPagoList.OrderBy(o => o.Descripcion);
                        else
                            mercadoPagoList = mercadoPagoList.OrderByDescending(o => o.Descripcion);
                        break;

                }

            }
            else
                mercadoPagoList = mercadoPagoList.OrderByDescending(s => s.Fecha);


            int totalRecords = mercadoPagoList.Count();
            int totalPages;

            mercadoPagoList = mercadoPagoList.Skip((page - 1) * pageSize)
            .Take(pageSize);

            TempData.Keep();

            int pageIndex = Convert.ToInt32(page) - 1;
            pageSize = rows;

            totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            var result = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = mercadoPagoList.AsQueryable(),

            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [Audit]
        public ActionResult ExportData(string jqGridPostData)
        {
            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");
            JQGridPostData postData = JsonConvert.DeserializeObject<JQGridPostData>(fixedPostData);
            var sessionSort = Session[JQGridFilterRule.Keys.MercadoPago.SIDX] as Dictionary<string, string>;

            bool hasSortSession = false;

            if (sessionSort != null)
                hasSortSession = true;

            string filters = "true";
            Dictionary<string, DateTime> dateFilters = new Dictionary<string, DateTime>();

            if (postData.filters != null)
            {
                for (int i = 0; i < postData.filters.rules.Count; i++)
                {
                    string col = postData.filters.rules[i].field;
                    string data = postData.filters.rules[i].data.ToLower();
 
                    if (col == "Fecha")
                    {
                        dateFilters.Add(col, Convert.ToDateTime(data));
                    }
                    else
                    {
                        if (filters != String.Empty)
                            filters += " && ";

                        if (col == "Monto")
                            filters += " " + col + "=" + data;
                        else
                            filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";

                    }

                }
            }

            var operadorID = GetUserOperadorID();
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");

            var mercadoPagos = db.MercadoPago.Where(x => operadorID == Guid.Empty || x.Maquina.OperadorID == operadorID)
                            .Select(x => new
                            {
                                Operador = x.Maquina.Operador.Nombre,
                                Comprobante = x.Comprobante,
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
                            });
                            
            if (filters.Replace(" ", "").Replace("&&", "") == "")
            {

            }
            else
            {
                mercadoPagos = mercadoPagos.Where(filters);
            }

            if (dateFilters.Any())
            {
                foreach (var item in dateFilters)
                {

                    if (item.Key == "Fecha")
                        mercadoPagos = mercadoPagos.Where(o => o.Fecha.Year == item.Value.Year && o.Fecha.Month == item.Value.Month && o.Fecha.Day == item.Value.Day);
                 }
            }

            string sord = "";
            string key = "";

            if (sessionSort != null)
            {

                    key = sessionSort.First().Key;
                    sord = sessionSort.First().Value;

                    switch (key)
                    {
                        case "OperadorNombre":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Operador);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Operador);
                            break;
                        case "Comprobante":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Comprobante);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Comprobante);
                            break;

                        case "Monto":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Monto);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Monto);
                            break;

                        case "Fecha":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Fecha);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Fecha);
                            break;

                        case "MercadoPagoEstadoTransmisionDescripcion":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.EstadoTransmision);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.EstadoTransmision);
                            break;

                        case "MercadoPagoEstadoFinancieroDescripcion":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.EstadoFinanciero);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.EstadoFinanciero);
                            break;

                        case "NroSerieTerminal":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.NroSerieTerminal);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.NroSerieTerminal);
                            break;

                        case "MaquinaDescripcion":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Maquina);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Maquina);
                            break;

                        case "NotaService":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.IdCajaMP);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.IdCajaMP);
                            break;

                        case "Locacion":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Locacion);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Locacion);
                            break;

                        case "Entidad":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Entidad);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Entidad);
                            break;

                        case "Descripcion":
                            if (sord == "asc")
                                mercadoPagos = mercadoPagos.OrderBy(o => o.Descripcion);
                            else
                                mercadoPagos = mercadoPagos.OrderByDescending(o => o.Descripcion);
                            break;

                    }
                
            }
            else
                mercadoPagos = mercadoPagos.OrderByDescending(s => s.Fecha);

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
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Estado Transmisión");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Estado Financiero");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Id Caja MP");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locacion");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("N° Serie Maquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("N° Serie Terminal");
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
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(mercadoPago.Monto));
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Fecha.ToString("dd/MM/yyyy HH:mm"));
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.EstadoTransmision);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.EstadoFinanciero);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.IdCajaMP);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Locacion);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.Maquina);
                row.CreateCell(colIdx++).SetCellValue(mercadoPago.NroSerieTerminal);
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


        private class Filter
        {
            public string field { get; set; }
            public string op { get; set; }
            public string data { get; set; }
        }

    }
}