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
using System.Linq.Dynamic;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using BugsMVC.Security;
using BugsMVC.Models.ViewModels;
using BugsMVC.Handlers;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class TerminalController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: Terminal
        public ActionResult Index()
        {
            var terminales = db.Terminales.Include(t => t.ModeloTerminal);
            return View(terminales.ToList().Select(x => TerminalViewModel.From(x)));
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

        public JsonResult GetAllTerminales()
        {          
            var operadorID = GetUserOperadorID();

            var terminales = db.Terminales.Where(x => operadorID == Guid.Empty || (x.OperadorID.HasValue ? x.OperadorID.Value : Guid.Empty) == operadorID).ToList()
                .Select(x => new
                {
                    TerminalID = x.TerminalID,
                    OperadorNombre = x.OperadorID.HasValue ? x.Operador.Nombre : string.Empty,
                    FechaAlta = x.FechaAlta.HasValue ? x.FechaAlta.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    Interfaz = x.Interfaz,
                    Maquina = x.Maquinas.FirstOrDefault() != null ? x.Maquinas.FirstOrDefault().NumeroSerie : "",
                    MaquinaAsignada = x.Maquinas.FirstOrDefault() != null ? true : false,
                    ModeloTerminal = x.ModeloTerminal.Modelo,
                    NumeroSerie = x.NumeroSerie,
                    Version = x.Version,
                    Perifericos = x.Perifericos ?? 0,
                    ModuloComunicacion = x.ModuloComunicacion,
                    SimCard = x.SimCard,
                    NivelSenal1 = x.NivelSenal1 ?? 0,
                    NivelSenal2 = x.NivelSenal2 ?? 0,
                    NivelSenal3 = x.NivelSenal3 ?? 0,
                    FechaNivel1 = x.FechaNivel1.HasValue ? x.FechaNivel1.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    FechaNivel2 = x.FechaNivel2.HasValue ? x.FechaNivel2.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    FechaNivel3 = x.FechaNivel3.HasValue ? x.FechaNivel3.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    PerifericoDescripcion = TerminalViewModel.GetPerifericoDescripcion(x.Perifericos ?? 0)
                });

            return Json(terminales.ToArray(), JsonRequestBehavior.AllowGet);
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

            var terminales = db.Terminales.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).ToList()
                                .Select(x => new
                                {
                                    NumeroSerie = x.NumeroSerie,
                                    OperadorNombre = x.OperadorID.HasValue? x.Operador.Nombre:string.Empty,
                                    ModeloTerminal = x.ModeloTerminal.Modelo,
                                    Interfaz = x.Interfaz,
                                    Version = x.Version,
                                    FechaAlta = x.FechaAlta,
                                    MaquinaAsignada = x.Maquinas.FirstOrDefault() != null ? true : false,
                                    Maquina = x.Maquinas.FirstOrDefault() != null ? x.Maquinas.FirstOrDefault().NumeroSerie : "",
                                    Habilitados = TerminalViewModel.GetPerifericoDescripcion(x.Perifericos ?? 0),
                                    ModuloComunicacion = x.ModuloComunicacion,
                                    SimCard = x.SimCard,
                                    NivelSenal1 = x.NivelSenal1.HasValue ? "Nivel Señal 1 = " + x.NivelSenal1.Value + Environment.NewLine : string.Empty,
                                    NivelSenal2 = x.NivelSenal2.HasValue ? "Nivel Señal 2 = " + x.NivelSenal2.Value + Environment.NewLine : string.Empty,
                                    NivelSenal3 = x.NivelSenal3.HasValue ? "Nivel Señal 3 = " + x.NivelSenal3.Value + Environment.NewLine : string.Empty,
                                    FechaNivel1 = x.FechaNivel1.HasValue ? "Fecha Nivel 1 = " + x.FechaNivel1.Value.ToString("dd/MM/yyyy HH:mm") + Environment.NewLine : string.Empty,
                                    FechaNivel2 = x.FechaNivel2.HasValue ? "Fecha Nivel 2 = " + x.FechaNivel2.Value.ToString("dd/MM/yyyy HH:mm") + Environment.NewLine : string.Empty,
                                    FechaNivel3 = x.FechaNivel3.HasValue ? "Fecha Nivel 3 = " + x.FechaNivel3.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                })
                                .Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            ISheet sheet = workbook.CreateSheet("Terminales");
            int amountOfColumns = 0;
            int fechaNivelColumn = 0;
            int habilitadosColumn = 0;
            IRow headerRow = sheet.CreateRow(0);
            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Número de Serie");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Máquina Asignada");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nro. Serie Máquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Modelo Terminal");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Versión");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Interfaz");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Modulo Comunicación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Sim Card");
            fechaNivelColumn = amountOfColumns;
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha/Nivel");
            habilitadosColumn = amountOfColumns;
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Periféricos Habilitados");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Fecha Alta");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);
            
            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;
            
            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            defaultCellStyle.VerticalAlignment = VerticalAlignment.Top;

            XSSFCellStyle leftAlignCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);
            leftAlignCellStyle.WrapText = true;
            leftAlignCellStyle.Alignment = HorizontalAlignment.Left;
            leftAlignCellStyle.VerticalAlignment = VerticalAlignment.Top;

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);
            dateCellStyle.VerticalAlignment = VerticalAlignment.Top;

            foreach (var terminal in terminales.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);
                colIdx = 0;

                string fechaNivelSenal = terminal.NivelSenal1 + terminal.FechaNivel1 + terminal.NivelSenal2 + terminal.FechaNivel2 + terminal.NivelSenal3 + terminal.FechaNivel3;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(terminal.OperadorNombre);

                row.CreateCell(colIdx++).SetCellValue(terminal.NumeroSerie.ToString());
                row.CreateCell(colIdx++).SetCellValue(terminal.MaquinaAsignada ? "SI" : "NO");
                row.CreateCell(colIdx++).SetCellValue(terminal.Maquina);
                row.CreateCell(colIdx++).SetCellValue(terminal.ModeloTerminal);
                row.CreateCell(colIdx++).SetCellValue(terminal.Version.ToString());
                row.CreateCell(colIdx++).SetCellValue(terminal.Interfaz);   
                row.CreateCell(colIdx++).SetCellValue(terminal.ModuloComunicacion);
                row.CreateCell(colIdx++).SetCellValue(terminal.SimCard);
                row.CreateCell(colIdx++).SetCellValue(fechaNivelSenal);
                row.CreateCell(colIdx++).SetCellValue(terminal.Habilitados);
                row.CreateCell(colIdx++).SetCellValue(terminal.FechaAlta.HasValue ? terminal.FechaAlta.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);

                for (int j = 0; j < colIdx; j++)
                {
                    row.Cells[j].CellStyle = defaultCellStyle;
                    if (row.Cells[j].CellType == CellType.Numeric)
                    {
                        row.Cells[j].CellStyle.DataFormat = doubleFormat;
                    }
                }

                row.Cells[fechaNivelColumn].CellStyle = leftAlignCellStyle;
                row.Cells[habilitadosColumn].CellStyle = leftAlignCellStyle;
            }

            HSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);
           
            for (int i = 0; i < amountOfColumns; i++)
            {
                sheet.AutoSizeColumn(i);
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1 * 256);
            }

            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Terminales " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        // GET: Terminal/Details/5
        public ActionResult Details(Guid? id, bool? backToMaquina)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Terminal terminal = db.Terminales.Find(id);
            if (terminal == null)
            {
                return HttpNotFound();
            }
            TerminalViewModel viewModel = TerminalViewModel.From(terminal);

            if (backToMaquina.HasValue)
            {
                viewModel.BackToMaquina = backToMaquina.Value;
            }

            return View(viewModel);
        }

        // GET: Terminal/Create
        public ActionResult Create()
        {
            Guid operadorID = GetUserOperadorID();
            ViewBag.OperadorID = operadorID;
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal, "ModeloTerminalID", "Modelo");
            TerminalViewModel terminal = new TerminalViewModel();
            terminal.OperadorID = operadorID;
            return View(terminal);
        }

        // POST: Terminal/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TerminalID,OperadorID,NumeroSerie,Version,Interfaz,FechaAlta,MaquinaID,ModeloTerminalID,FechaUltimaConexion,FechaFabricacion,FechaEstadoSeteosEscritura,TipoLector_out")] TerminalViewModel terminal)
        {
            Guid operadorID = GetUserOperadorID();
            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor, seleccione un Operador.");
            }

            if (db.Terminales.Any(x => x.NumeroSerie == terminal.NumeroSerie && x.ModeloTerminalID == terminal.ModeloTerminalID))
            {
                ModelState.AddModelError(string.Empty, "Número de serie existente.");
            }

            if (ModelState.IsValid)
            {
                terminal.OperadorID = operadorID;
                terminal.TerminalID = Guid.NewGuid();
                Terminal entity = new Terminal();
                terminal.ToEntity(entity);
                db.Terminales.Add(entity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OperadorID = GetUserOperadorID();
            terminal.OperadorID = operadorID;
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal, "ModeloTerminalID", "Modelo", terminal.ModeloTerminalID);
            return View(terminal);
        }

        // GET: Terminal/Edit/5
        public ActionResult Edit(Guid? id)
        {
            Guid operadorID = GetUserOperadorID();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Terminal terminal = db.Terminales.Find(id);
            if (terminal == null)
            {
                return HttpNotFound();
            }
            TerminalViewModel viewModel = TerminalViewModel.From(terminal);
            ViewBag.OperadorID = operadorID;
            viewModel.OperadorID = operadorID;
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal, "ModeloTerminalID", "Modelo", viewModel.ModeloTerminalID);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TerminalID,OperadorID,NumeroSerie,Interfaz,Version,FechaAlta,MaquinaIDJonathan,ModeloTerminalID,FechaUltimaConexion,FechaFabricacion,FechaEstadoSeteosEscritura,TipoLector_out")] TerminalViewModel terminal)
        {
            var operadorID = GetUserOperadorID() == Guid.Empty ? terminal.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;
            terminal.OperadorID = operadorID;

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor, seleccione un Operador.");
            }

            if (db.Terminales.Any(x=> x.TerminalID != terminal.TerminalID && x.NumeroSerie == terminal.NumeroSerie && x.ModeloTerminalID == terminal.ModeloTerminalID))
            {
                ModelState.AddModelError(string.Empty, "Número de serie existente.");
            }

            if (ModelState.IsValid)
            {
                Terminal entity = new Terminal();
                terminal.ToEntity(entity);
                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal, "ModeloTerminalID", "Modelo", terminal.ModeloTerminalID);
            return View(terminal);
        }

        // GET: Terminal/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Terminal terminal = db.Terminales.Find(id);
            if (terminal == null)
            {
                return HttpNotFound();
            }
            TerminalViewModel viewModel = TerminalViewModel.From(terminal);
            return View(viewModel);
        }

        // POST: Terminal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Terminal terminal = db.Terminales.Find(id);            

            var transacciones = db.Transacciones.Where(x => x.TerminalID == terminal.TerminalID);
            var transaccionesMal = db.TransaccionesMal.Where(x => x.TerminalID == terminal.TerminalID);


            terminal.Maquinas.ToList().ForEach(x => x.DesasignarMaquina());

            db.Transacciones.RemoveRange(transacciones);
            db.TransaccionesMal.RemoveRange(transaccionesMal);
            db.Terminales.Remove(terminal);
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
