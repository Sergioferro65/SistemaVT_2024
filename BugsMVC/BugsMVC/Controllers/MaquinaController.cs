using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BugsMVC.DAL;
using BugsMVC.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using BugsMVC.Handlers;
using System.Linq.Dynamic;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using BugsMVC.Security;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class MaquinaController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: Maquina
        public ActionResult Index()
        {            
            var maquinas = db.Maquinas.Include(x => x.Operador).ToList();
            return View(maquinas);
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

        public JsonResult GetAllMaquinas()
        {
            var operadorID = GetUserOperadorID();

            var maquinas = db.Maquinas.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).ToList()
                .Select(x => new
                {
                MaquinaID = x.MaquinaID,
                OperadorNombre = x.Operador.Nombre,
                Zona = x.Zona.HasValue ? x.Zona.Value.ToString() : string.Empty,
                NombreZona1 = x.Locacion == null ? string.Empty : x.Locacion.NombreZona1,
                NombreZona2 = x.Locacion == null ? string.Empty : x.Locacion.NombreZona2,
                NombreZona3 = x.Locacion == null ? string.Empty : x.Locacion.NombreZona3,
                NombreZona4 = x.Locacion == null ? string.Empty : x.Locacion.NombreZona4,
                NombreZona5 = x.Locacion == null ? string.Empty : x.Locacion.NombreZona5,
                ZonaText = x.Zona.HasValue ? (x.Locacion != null ? 
                                ((x.Locacion.NombreZona1 != null)? x.Locacion.NombreZona1 : 
                                (x.Locacion.NombreZona2 != null) ? x.Locacion.NombreZona2 : 
                                (x.Locacion.NombreZona3 != null) ? x.Locacion.NombreZona3 : 
                                (x.Locacion.NombreZona4 != null) ? x.Locacion.NombreZona4 : 
                                (x.Locacion.NombreZona5 != null) ? x.Locacion.NombreZona5 : string.Empty) : string.Empty) : string.Empty,
                Operador = x.Operador.Nombre,
                NumeroSerie = x.NumeroSerie,
                NombreAlias = x.NombreAlias,
                Ubicacion = x.Ubicacion,
                Estado = x.Estado,
                EstadoConexion = x.EstadoConexion,
                Mensaje = x.Mensaje,
                NotasService = x.NotasService,
                ContadorVentasParcial = x.ContadorVentasParcial,
                MontoVentasParcial = x.MontoVentasParcial,
                ContadorVentasHistorico = x.ContadorVentasHistorico,
                MontoVentasHistorico = x.MontoVentasHistorico,
                FechaUltimoService = x.FechaUltimoService,
                FechaUltimaRecaudacion = x.FechaUltimaRecaudacion,
                FechaUltimaReposicion = x.FechaUltimaReposicion,
                EfectivoRecaudado = x.TotalRecaudado,
                SoloVentaEfectivo = x.SoloVentaEfectivo,
                ValorVenta = x.ValorVenta,
                Decimales = x.Decimales,
                FactorEscala = x.FactorEscala,
                TiempoSesion = x.TiempoSesion,
                CreditoMaximoCash = x.CreditoMaximoCash,
                ValorChannelA = x.ValorChannelA,
                ValorChannelB = x.ValorChannelB,
                ValorChannelC = x.ValorChannelC,
                ValorChannelD = x.ValorChannelD,
                ValorChannelE = x.ValorChannelE,
                ValorChannelF = x.ValorChannelF,
                ValorBillete1 = x.ValorBillete1,
                ValorBillete2 = x.ValorBillete2,
                ValorBillete3 = x.ValorBillete3,
                ValorBillete4 = x.ValorBillete4,
                ValorBillete5 = x.ValorBillete5,
                ValorBillete6 = x.ValorBillete6,
                AlarmaActiva = x.AlarmaActiva,
                FechaAviso = x.FechaAviso,
                FechaEstado = x.FechaEstado,
                Locacion = x.Locacion  == null ? "Sin asignación" : x.Locacion.Nombre,
                MarcaModelo = x.MarcaModelo.MarcaModeloNombre,
                Terminal = x.Terminal == null ? "Sin asignación" : x.Terminal.NumeroSerie.ToString(),
                TerminalID = x.Terminal == null ? null : x.Terminal.TerminalID.ToString(),
                TipoProducto = x.TipoProducto.Nombre,
                DescuentoPorcentual = x.DescuentoPorcentual ?? 0,
                PrecioVentaValorDescuento = x.PrecioVentaValorDescuento
                });

            return Json(maquinas.ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaquinas(string LocacionID)
        {
            Guid locacionID = new Guid(LocacionID);
            List<SelectListItem> maquinas = new List<SelectListItem>();

            var lista = db.Maquinas.Where(x => x.LocacionID == locacionID);

            foreach (var item in lista)
            {
                maquinas.Add(new SelectListItem { Text = item.NombreAlias, Value = item.MaquinaID.ToString() });
            }

            return Json(new SelectList(maquinas, "Value", "Text"));
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

            var maquinas = db.Maquinas.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).ToList()
                            .Select(x => new
                            {
                                MaquinaId = x.MaquinaID,
                                NumeroSerie = x.NumeroSerie,
                                OperadorNombre = x.Operador.Nombre,
                                MarcaModelo = x.MarcaModelo.MarcaModeloNombre,
                                Terminal = x.Terminal == null ? "Sin asignación" : x.Terminal.NumeroSerie.ToString(),
                                AlarmaActiva = x.AlarmaActiva.Value ? "Si" : "No",
                                FechaAviso = x.FechaAviso,
                                FechaEstado = x.FechaEstado,
                                Locacion = x.Locacion == null ? "Sin asignación" : x.Locacion.Nombre,
                                Zona = x.Zona.HasValue?  (x.Zona.Value == 1 ? (x.Locacion == null ? string.Empty : x.Locacion.NombreZona1)
                                : x.Zona.Value == 2 ? (x.Locacion == null ? string.Empty : x.Locacion.NombreZona2) :
                                        x.Zona.Value == 3 ? (x.Locacion == null ? string.Empty : x.Locacion.NombreZona3) : 
                                        x.Zona.Value == 4 ? (x.Locacion == null ? string.Empty : x.Locacion.NombreZona4) :
                                        x.Zona.Value == 5 ? (x.Locacion == null ? string.Empty : x.Locacion.NombreZona5) : string.Empty): string.Empty,
                                NombreAlias = x.NombreAlias,
                                Ubicacion = x.Ubicacion,
                                Estado = x.Estado,
                                EstadoConexion = x.EstadoConexion,
                                ContadorVentasParcial = x.ContadorVentasParcial,
                                MontoVentasParcial = x.MontoVentasParcial,
                                ContadorVentasHistorico = x.ContadorVentasHistorico,
                                MontoVentasHistorico = x.MontoVentasHistorico,
                                EfectivoRecaudado = x.TotalRecaudado,
                                //ValorVenta = x.ValorVenta,
                                DescuentoPorcentual = x.DescuentoPorcentual.HasValue ? x.DescuentoPorcentual.Value : 0,
                                PrecioVentaValorDescuento = x.PrecioVentaValorDescuento,
                                TipoProducto = x.TipoProducto.Nombre
                            })
                            .Where(filters);

            //Create new Excel workbook
            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");
            short percentageFormat = workbook.CreateDataFormat().GetFormat("#,0.00%");

            // Create new Excel sheet
            ISheet sheet = workbook.CreateSheet("Máquinas");

            int amountOfColumns = 0;
            int percentajeColumn = 0;
            int ventaDescuentoColumn = 0;

            // Create a header row
            IRow headerRow = sheet.CreateRow(0);

            // Set the column names in the header row
            if (esSuperAdmin)
            { 
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Máquina Id");
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            }
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Marca Modelo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("N° Serie Máquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Tipo Producto");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Estado");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Estado Conexión");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Alarma Activa");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Estado");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Aviso");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Zona");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Ubicación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Alias");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("N° Serie Terminal");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Contador Ventas Parcial");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Ventas Parcial");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Contador Ventas Historico");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Ventas Historico");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Efectivo Recaudado");
            //headerRow.CreateCell(amountOfColumns++).SetCellValue("Valor Venta");
            percentajeColumn = amountOfColumns;
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descuento Porcentual");
            ventaDescuentoColumn = amountOfColumns;
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Precio Venta/Valor Descuento");

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
            XSSFCellStyle percentageCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            percentageCellStyle.VerticalAlignment = VerticalAlignment.Top;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            defaultCellStyle.VerticalAlignment = VerticalAlignment.Top;

            XSSFCellStyle leftAlignCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            leftAlignCellStyle.WrapText = true;
            leftAlignCellStyle.Alignment = HorizontalAlignment.Left;
            leftAlignCellStyle.VerticalAlignment = VerticalAlignment.Top;

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);
            dateCellStyle.VerticalAlignment = VerticalAlignment.Top;

            // Populate the sheet with values from the grid data
            foreach (var maquina in maquinas.ToList())
            {
                // Create a new row
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                // Set values for the cells
                if (esSuperAdmin)
                {
                    row.CreateCell(colIdx++).SetCellValue(maquina.MaquinaId.ToString());
                    row.CreateCell(colIdx++).SetCellValue(maquina.OperadorNombre);
                }
                row.CreateCell(colIdx++).SetCellValue(maquina.MarcaModelo);
                row.CreateCell(colIdx++).SetCellValue(maquina.NumeroSerie);
                row.CreateCell(colIdx++).SetCellValue(maquina.TipoProducto);
                row.CreateCell(colIdx++).SetCellValue(maquina.Estado);
                row.CreateCell(colIdx++).SetCellValue(maquina.EstadoConexion);
                row.CreateCell(colIdx++).SetCellValue(maquina.AlarmaActiva);
                row.CreateCell(colIdx++).SetCellValue(maquina.FechaEstado.HasValue ? maquina.FechaEstado.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(maquina.FechaAviso.HasValue ? maquina.FechaAviso.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(maquina.Locacion);
                row.CreateCell(colIdx++).SetCellValue(maquina.Zona);
                row.CreateCell(colIdx++).SetCellValue(maquina.Ubicacion);
                row.CreateCell(colIdx++).SetCellValue(maquina.NombreAlias);
                row.CreateCell(colIdx++).SetCellValue(maquina.Terminal);
                row.CreateCell(colIdx++).SetCellValue(maquina.ContadorVentasParcial.ToString());
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(maquina.MontoVentasParcial));
                row.CreateCell(colIdx++).SetCellValue(maquina.ContadorVentasHistorico.ToString());
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(maquina.MontoVentasHistorico));
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(maquina.EfectivoRecaudado));
                //row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(maquina.ValorVenta));
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(maquina.DescuentoPorcentual/100));
                row.CreateCell(colIdx++).SetCellValue(maquina.PrecioVentaValorDescuento);

                for (int j = 0; j < colIdx; j++)
                {
                    row.Cells[j].CellStyle = defaultCellStyle;
                    if (row.Cells[j].CellType == CellType.Numeric)
                    {
                        row.Cells[j].CellStyle.DataFormat = doubleFormat;
                    }
                }
                row.Cells[percentajeColumn].CellStyle = percentageCellStyle;
                row.Cells[percentajeColumn].CellStyle.DataFormat = percentageFormat;
                row.Cells[ventaDescuentoColumn].CellStyle = leftAlignCellStyle;
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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Máquinas " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        // GET: Maquina/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Maquina maquina = db.Maquinas.Find(id);
            if (maquina == null)
            {
                return HttpNotFound();
            }
            return View(maquina);
        }

        // GET: Maquina/Create
        public ActionResult Create()
        {
            ViewBag.Zona = new SelectList(string.Empty, "ZonaID", "Nombre");

            var operadorID = GetUserOperadorID();

            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");
            ViewBag.MarcaModeloID = new SelectList(db.MarcasModelos.OrderBy(x => x.MarcaModeloNombre), "MarcaModeloID", "MarcaModeloNombre");
            ViewBag.TerminalID = new SelectList(db.Terminales.Where(x => x.OperadorID == operadorID && !x.Maquinas.Any(y => y.TerminalID == x.TerminalID)), "TerminalID", "NumeroSerie");
            ViewBag.TipoProductoID = new SelectList(db.TipoProductos.OrderBy(x => x.TipoProductoID), "TipoProductoID", "Nombre");

            Maquina maquina = new Maquina();
            maquina.Estado = "Creada";

            return View(maquina);
        }

        // POST: Maquina/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FechaEstado,MaquinaID,Zona,NumeroSerie,NombreAlias,Ubicacion,Estado,Mensaje,NotasService,ContadorVentasParcial,MontoVentasParcial,ContadorVentasHistorico,MontoVentasHistorico,FechaUltimoService,FechaUltimaRecaudacion,FechaUltimaReposicion,EfectivoRecaudado,SoloVentaEfectivo,ValorVenta,Decimales,FactorEscala,TiempoSesion,CreditoMaximoCash,ValorChannelA,ValorChannelB,ValorChannelC,ValorChannelD,ValorChannelE,ValorChannelF,ValorBillete1,ValorBillete2,ValorBillete3,ValorBillete4,ValorBillete5,ValorBillete6,LocacionID,MarcaModeloID,TerminalID,OperadorID,CheckAsignarMaquina,ShowDetails,AlarmaActiva,FechaAviso,MostrarPanelDescuentos,MostrarDatosOpcionales,FechaUltimoOk,FechaUltimaConexion,EstadoConexion,DescuentoPrecio1,DescuentoPrecio2,DescuentoPrecio3,DescuentoPrecio4,DescuentoPrecio5,DescuentoValor1,DescuentoValor2,DescuentoValor3,DescuentoValor4,DescuentoValor5,DescuentoPorcentual,TipoProductoID")] Maquina maquina)
        {
            var operadorID = GetUserOperadorID();

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }

            if (maquina.CheckAsignarMaquina)
            {
                if (!maquina.LocacionID.HasValue)
                    ModelState.AddModelError("LocacionID", "Por favor seleccione una Locación.");
                if (!maquina.TerminalID.HasValue)
                    ModelState.AddModelError("TerminalID", "Por favor seleccione una Terminal.");
                if (maquina.Zona == 0 || !maquina.Zona.HasValue)
                    ModelState.AddModelError("Zona", "Por favor seleccione una Zona.");
            }
            
            if (db.Maquinas.Any(x => x.MaquinaID != maquina.MaquinaID && x.NumeroSerie == maquina.NumeroSerie && x.OperadorID == maquina.OperadorID))
            {
                ModelState.AddModelError("NumeroSerie", "Ya existe una máquina con el mismo número de serie.");
            }

            if (ModelState.IsValid)
            {
                maquina.MaquinaID = Guid.NewGuid();
                maquina.OperadorID = operadorID;
                maquina.FechaUltimaRecaudacion = DateTime.Now;
                maquina.FechaUltimoService = DateTime.Now;
                if (maquina.CheckAsignarMaquina)
                {
                    maquina.FechaEstado = DateTime.Now;
                }
                db.Maquinas.Add(maquina);                              

                ActualizarMaquinaJonathan(maquina);

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            if (maquina.LocacionID.HasValue && maquina.LocacionID != Guid.Empty)
            {
                List<SelectListItem> zonas = GetZonasList(maquina.LocacionID.Value);

                ViewBag.Zona = new SelectList(zonas, "Value", "Text", Convert.ToString(maquina.Zona));
            }
            else
            {
                ViewBag.Zona = new SelectList(string.Empty, "Value", "Text");
            }

            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "LocacionID", "Nombre", maquina.LocacionID);
            ViewBag.MarcaModeloID = new SelectList(db.MarcasModelos.OrderBy(x => x.MarcaModeloNombre), "MarcaModeloID", "MarcaModeloNombre", maquina.MarcaModeloID);
            ViewBag.TerminalID = new SelectList(db.Terminales.Where(x => x.OperadorID == operadorID && !x.Maquinas.Any(y => y.TerminalID == x.TerminalID)), "TerminalID", "NumeroSerie");
            ViewBag.TipoProductoID = new SelectList(db.TipoProductos.OrderBy(x => x.TipoProductoID), "TipoProductoID", "Nombre", maquina.TipoProductoID);

            if (!ModelState.IsValidField("DescuentoPorcentual"))
            {
                maquina.MostrarPanelDescuentos = true;
            }

            return View(maquina);
        }

        private void ActualizarMaquinaJonathan(Maquina maquina)
        {
            if (maquina.TerminalID != null)
            {
                var terminal = db.Terminales.Find(maquina.TerminalID);
                terminal.MaquinaIDJonathan = maquina.MaquinaID;
                db.Entry(terminal).State = EntityState.Modified;
            }
        }

        public string GetNombreZona(int numeroZona, Locacion locacion)
        {
            string nombreZona = string.Empty;

            switch (numeroZona)
            {
                case 1:
                    nombreZona = locacion.NombreZona1;
                    break;
                case 2:
                    nombreZona = locacion.NombreZona2;
                    break;
                case 3:
                    nombreZona = locacion.NombreZona3;
                    break;
                case 4:
                    nombreZona = locacion.NombreZona4;
                    break;
                case 5:
                    nombreZona = locacion.NombreZona5;
                    break;
                default:
                    break;
            }
            return nombreZona;
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

        // GET: Maquina/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Maquina maquina = db.Maquinas.Find(id);

            if (maquina == null)
            {
                return HttpNotFound();
            }            

            if (maquina.LocacionID.HasValue && maquina.LocacionID != Guid.Empty)
            {
                List<SelectListItem> zonas = GetZonasList(maquina.LocacionID.Value);

                ViewBag.Zona = new SelectList(zonas, "Value", "Text", Convert.ToString(maquina.Zona));
            }
            else
            {
                ViewBag.Zona = new SelectList(string.Empty, "Value", "Text");
            }

            var operadorID = GetUserOperadorID() == Guid.Empty ? maquina.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x=>x.OperadorID==operadorID).OrderBy(x => x.Nombre),
                "LocacionID", "Nombre", maquina.LocacionID);
            ViewBag.MarcaModeloID = new SelectList(db.MarcasModelos.OrderBy(x => x.MarcaModeloNombre), "MarcaModeloID", "MarcaModeloNombre", maquina.MarcaModeloID);
            ViewBag.TerminalID = new SelectList(db.Terminales.Where(x => x.OperadorID == operadorID && !x.Maquinas.Any(y =>y.TerminalID != maquina.TerminalID && y.TerminalID == x.TerminalID)), "TerminalID", "NumeroSerie", maquina.TerminalID);
            ViewBag.TipoProductoID = new SelectList(db.TipoProductos.OrderBy(x => x.TipoProductoID), "TipoProductoID", "Nombre", maquina.TipoProductoID);
            maquina.CheckAsignarMaquina = (maquina.LocacionID.HasValue || !string.IsNullOrEmpty(maquina.Ubicacion) || !string.IsNullOrEmpty(maquina.NombreAlias) || maquina.TerminalID.HasValue);

            return View(maquina);
        }

        // POST: Maquina/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FechaAviso,FechaEstado,MaquinaID,Zona,NumeroSerie,NombreAlias,Ubicacion,Estado,Mensaje,NotasService,ContadorVentasParcial,MontoVentasParcial,ContadorVentasHistorico,MontoVentasHistorico,FechaUltimoService,FechaUltimaRecaudacion,FechaUltimaReposicion,TotalRecaudado,SoloVentaEfectivo,ValorVenta,Decimales,FactorEscala,TiempoSesion,CreditoMaximoCash,ValorChannelA,ValorChannelB,ValorChannelC,ValorChannelD,ValorChannelE,ValorChannelF,ValorBillete1,ValorBillete2,ValorBillete3,ValorBillete4,ValorBillete5,ValorBillete6,LocacionID,MarcaModeloID,TerminalID,OperadorID,CheckAsignarMaquina,ShowDetails,AlarmaActiva,MostrarDatosOpcionales,FechaUltimoOk,EstadoConexion,FechaUltimaConexion,DescuentoPrecio1,DescuentoPrecio2,DescuentoPrecio3,DescuentoPrecio4,DescuentoPrecio5,DescuentoValor1,DescuentoValor2,DescuentoValor3,DescuentoValor4,DescuentoValor5,DescuentoPorcentual,TipoProductoID")] Maquina maquina)
        {
            var operadorID = GetUserOperadorID() == Guid.Empty ? maquina.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;

            if (db.Maquinas.Any(x => x.MaquinaID != maquina.MaquinaID && x.NumeroSerie == maquina.NumeroSerie && x.OperadorID == maquina.OperadorID))
            {
                ModelState.AddModelError("NumeroSerie", "Ya existe una máquina con el mismo número de serie.");
            }

            if (maquina.CheckAsignarMaquina)
            {
                if (!maquina.LocacionID.HasValue)
                    ModelState.AddModelError("LocacionID", "Por favor seleccione una Locación.");
                if (!maquina.TerminalID.HasValue)
                    ModelState.AddModelError("TerminalID", "Por favor seleccione una Terminal.");
                if (!maquina.Zona.HasValue || maquina.Zona == 0)
                    ModelState.AddModelError("Zona", "Por favor seleccione una Zona.");
            }

            if (ModelState.IsValid)
            {
                ActualizarMaquinaJonathan(maquina);
                maquina.OperadorID = operadorID;
                if (!maquina.CheckAsignarMaquina)
                {
                    maquina.EstadoConexion = string.Empty;
                }
                else
                {
                    maquina.FechaEstado = DateTime.Now;
                }
                db.Entry(maquina).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            if (maquina.LocacionID.HasValue && maquina.LocacionID != Guid.Empty)
            {
                List<SelectListItem> zonas = GetZonasList(maquina.LocacionID.Value);

                ViewBag.Zona = new SelectList(zonas, "Value", "Text", Convert.ToString(maquina.Zona));
            }
            else
            {
                ViewBag.Zona = new SelectList(string.Empty, "Value", "Text");
            }

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "LocacionID", "Nombre", maquina.LocacionID);
            ViewBag.MarcaModeloID = new SelectList(db.MarcasModelos.OrderBy(x => x.MarcaModeloNombre), "MarcaModeloID", "MarcaModeloNombre", maquina.MarcaModeloID);

            Guid? TerminalIDmaquinaActual = maquina.TerminalID ?? db.Maquinas.Find(maquina.MaquinaID).TerminalID;
            
            ViewBag.TerminalID = new SelectList(db.Terminales.Where(x => x.OperadorID == operadorID && !x.Maquinas.Any(y =>y.TerminalID != TerminalIDmaquinaActual && y.TerminalID == x.TerminalID)), "TerminalID", "NumeroSerie", TerminalIDmaquinaActual);

            if (!ModelState.IsValidField("DescuentoPorcentual"))
            {
                maquina.MostrarPanelDescuentos = true;
            }


            ViewBag.TipoProductoID = new SelectList(db.TipoProductos.OrderBy(x => x.TipoProductoID), "TipoProductoID", "Nombre", maquina.TipoProductoID);

            return View(maquina);
        }

        // GET: Maquina/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Maquina maquina = db.Maquinas.Find(id);
            if (maquina == null)
            {
                return HttpNotFound();
            }
            return View(maquina);
        }

        // POST: Maquina/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Maquina maquina = db.Maquinas.Find(id);
            var transacciones = db.Transacciones.Where(x => x.MaquinaID == maquina.MaquinaID);
            var transaccionesMal = db.TransaccionesMal.Where(x => x.MaquinaID == maquina.MaquinaID);
            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => x.MaquinaID == maquina.MaquinaID);
            var articulosAsignacionesID = articulosAsignaciones.Select(x => x.Id);
            var stocks = db.Stocks.Where(x => articulosAsignacionesID.Contains(x.ArticuloAsignacionID));
            var stocksID = stocks.Select(x => x.StockID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => stocksID.Contains(x.StockID.Value));
            var tableOfflines = db.TablasOfflines.Where(x => x.LocacionID == maquina.LocacionID);

            if (maquina.TerminalID.HasValue && maquina.Terminal != null)
                maquina.Terminal.MaquinaIDJonathan = null;

            db.StocksHistoricos.RemoveRange(stocksHistoricos);
            db.Stocks.RemoveRange(stocks);
            db.Transacciones.RemoveRange(transacciones);
            db.TransaccionesMal.RemoveRange(transaccionesMal);
            db.TablasOfflines.RemoveRange(tableOfflines);
           
            //Se desasigna
            articulosAsignaciones.ToList().ForEach(x => {
                x.MaquinaID = null;
                x.ControlStock = false;
                x.AlarmaActiva = false;
                x.AlarmaBajo = null;
                x.AlarmaMuyBajo = null;
                x.Capacidad = null;
            });

            db.Maquinas.Remove(maquina);
            db.SaveChanges();
            return RedirectToAction("Index");
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
