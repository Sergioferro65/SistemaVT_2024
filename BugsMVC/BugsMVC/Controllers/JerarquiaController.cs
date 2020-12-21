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
using Microsoft.AspNet.Identity.Owin;
using BugsMVC.Security;
using System.Globalization;
using BugsMVC.Models.ViewModels;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class JerarquiaController : BaseController
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

        // GET: Jerarquia
        public ActionResult Index()
        {
            //var jerarquias = db.Jerarquias.Include(j => j.Locacion);
            //return View(jerarquias.ToList());
            return View(new JerarquiaViewModel());
        }

        // GET: Jerarquia/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jerarquia jerarquia = db.Jerarquias.Find(id);
            if (jerarquia == null)
            {
                return HttpNotFound();
            }

            JerarquiaViewModel viewModel = JerarquiaViewModel.From(jerarquia);

            //var operadorID = GetUserOperadorID() == Guid.Empty ? jerarquia.Locacion.OperadorID : GetUserOperadorID();
            //ViewBag.OperadorID = operadorID;

            //ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
            //    "LocacionID", "Nombre", jerarquia.LocacionID);

            //viewModel.NombreZonas = GetListZonas(jerarquia.LocacionID);

            return View(viewModel);

            //var zonas = GetListZonas(Guid.Empty);

            //return View(jerarquia);
        }

        // GET: Jerarquia/Create
        public ActionResult Create()
        {
            Guid operadorID = GetUserOperadorID();
            if (operadorID == Guid.Empty)
                ModelState.AddModelError(String.Empty, "Por favor, seleccione un Operador");

            JerarquiaViewModel viewModel = new JerarquiaViewModel();

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");

            viewModel.NombreZonas = GetListZonas(Guid.Empty);

            return View(viewModel);
        }

        // POST: Jerarquia/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JerarquiaViewModel viewModel)
        {
            var operadorID = GetUserOperadorID();
            var existe = db.Jerarquias.Any(x => x.Locacion.LocacionID == viewModel.LocacionID && x.Nombre == viewModel.Nombre && x.JerarquiaID != viewModel.JerarquiaID);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre ya existe.");
            }

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError("OperadorID", "Campo Obligatorio.");
            }

            if(viewModel.PeriodoRecargaZona1 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if(viewModel.MontoRecorteZona1 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona1", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona1 <= 0)
                {
                    ModelState.AddModelError("RecargaZona1", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona2 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona2 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona2", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona2 <= 0)
                {
                    ModelState.AddModelError("RecargaZona2", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona3 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona3 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona3", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona3 <= 0)
                {
                    ModelState.AddModelError("RecargaZona3", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona4 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona4 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona4", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona4 <= 0)
                {
                    ModelState.AddModelError("RecargaZona4", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona5 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona5 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona5", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona5 <= 0)
                {
                    ModelState.AddModelError("RecargaZona5", "Debe ser mayor a 0.");
                }
            }

            ServersideFieldValidation(viewModel, ModelState);
            ModelState.Remove("JerarquiaID");

            if (ModelState.IsValid)
            {
                viewModel.JerarquiaID = Guid.NewGuid();
                Jerarquia entity = new Jerarquia();
                viewModel.ToEntity(entity);
                db.Jerarquias.Add(entity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "LocacionID", "Nombre", viewModel.LocacionID);

            viewModel.NombreZonas = GetListZonas(viewModel.LocacionID);
            return View(viewModel);
        }

        // GET: Jerarquia/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jerarquia jerarquia = db.Jerarquias.Find(id);
            if (jerarquia == null)
            {
                return HttpNotFound();
            }

            JerarquiaViewModel viewModel = JerarquiaViewModel.From(jerarquia);

            var operadorID = GetUserOperadorID() == Guid.Empty ? jerarquia.Locacion.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;

            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "LocacionID", "Nombre", jerarquia.LocacionID);

            viewModel.NombreZonas = GetListZonas(jerarquia.LocacionID);

            return View(viewModel);
        }

        // POST: Jerarquia/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JerarquiaViewModel viewModel)
        {
            var existe = db.Jerarquias.Any(x => x.Locacion.LocacionID == viewModel.LocacionID && x.Nombre == viewModel.Nombre && x.JerarquiaID != viewModel.JerarquiaID);

            Jerarquia jerarquia = db.Jerarquias.Find(viewModel.JerarquiaID);
            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre ya existe.");
            }

            //var operadorID = GetUserOperadorID();
            //if (operadorID == Guid.Empty)
            //{
            //    ModelState.AddModelError("OperadorID", "Campo Obligatorio.");
            //}

            if (viewModel.PeriodoRecargaZona1 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona1 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona1", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona1 <= 0)
                {
                    ModelState.AddModelError("RecargaZona1", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona2 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona2 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona2", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona2 <= 0)
                {
                    ModelState.AddModelError("RecargaZona2", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona3 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona3 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona3", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona3 <= 0)
                {
                    ModelState.AddModelError("RecargaZona3", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona4 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona4 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona4", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona4 <= 0)
                {
                    ModelState.AddModelError("RecargaZona4", "Debe ser mayor a 0.");
                }
            }
            if (viewModel.PeriodoRecargaZona5 != JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                if (viewModel.MontoRecorteZona5 <= 0)
                {
                    ModelState.AddModelError("MontoRecorteZona5", "Debe ser mayor a 0.");
                }
                if (viewModel.RecargaZona5 <= 0)
                {
                    ModelState.AddModelError("RecargaZona5", "Debe ser mayor a 0.");
                }
            }

            ServersideFieldValidation(viewModel, ModelState);
            var operadorID = GetUserOperadorID() == Guid.Empty ? jerarquia.Locacion.OperadorID : GetUserOperadorID();

            if (ModelState.IsValid)
            {
                //Jerarquia entity = new Jerarquia();
                viewModel.ToEntity(jerarquia);

                db.Entry(jerarquia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre),
                "LocacionID", "Nombre", viewModel.LocacionID);

            viewModel.NombreZonas = GetListZonas(viewModel.LocacionID);

            return View(viewModel);
        }

        // GET: Jerarquia/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jerarquia jerarquia = db.Jerarquias.Find(id);
            if (jerarquia == null)
            {
                return HttpNotFound();
            }
            return View(jerarquia);
        }

        // POST: Jerarquia/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();

            Jerarquia jerarquia = db.Jerarquias.Find(id);

            var usuarios = db.Usuarios.Where(x => x.JerarquiaID == jerarquia.JerarquiaID);
            var usuariosID = usuarios.Select(x => x.UsuarioID);
            var transacciones = db.Transacciones.Where(x => x.JerarquiaID == jerarquia.JerarquiaID);

            foreach (Usuario usuario in usuarios.ToList())
            {
                db.StocksHistoricos.RemoveRange(usuario.StocksHistoricos);
                db.Transacciones.RemoveRange(usuario.Transacciones);
            }


            foreach (var usuarioID in usuariosID.ToList())
            {
                var appUser = db.Users.SingleOrDefault(x => x.UsuarioID == usuarioID);

                if (appUser != null)
                {
                    foreach (var role in UserManager.GetRoles(appUser.Id))
                    {
                        UserManager.RemoveFromRoles(appUser.Id, role);
                    }

                    UserManager.Delete(appUser);
                }
            }

            db.Transacciones.RemoveRange(transacciones);
            db.Usuarios.RemoveRange(usuarios);
            db.Jerarquias.Remove(jerarquia);

            db.SaveChanges();

            return RedirectToAction("Index");
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

            var jerarquias = db.Jerarquias.Where(x => operadorID == Guid.Empty || x.Locacion.OperadorID == operadorID)
                .Select(x => new
                {
                    Nombre = x.Nombre,
                    OperadorNombre = x.Locacion.Operador.Nombre,
                    Locacion = (x.Locacion != null) ? x.Locacion.Nombre : "Locación NO Registrada.",
                    NombreZona1 = x.Locacion.NombreZona1,
                    //MontoMaximoEfectivo = x.MontoMaximoEfectivo,
                    RecargaZona1 = x.RecargaZona1,
                    DescuentoPorcentualZona1 = x.DescuentoPorcentualZona1,
                    MontoRecorteZona1 = x.MontoRecorteZona1,
                    NombreZona2 = x.Locacion.NombreZona2,
                    RecargaZona2 = x.RecargaZona2,
                    DescuentoPorcentualZona2 = x.DescuentoPorcentualZona2,
                    MontoRecorteZona2 = x.MontoRecorteZona2,
                    NombreZona3 = x.Locacion.NombreZona3,
                    RecargaZona3 = x.RecargaZona3,
                    DescuentoPorcentualZona3 = x.DescuentoPorcentualZona3,
                    MontoRecorteZona3 = x.MontoRecorteZona3,
                    NombreZona4 = x.Locacion.NombreZona4,
                    RecargaZona4 = x.RecargaZona4,
                    DescuentoPorcentualZona4 = x.DescuentoPorcentualZona4,
                    MontoRecorteZona4 = x.MontoRecorteZona4,
                    NombreZona5 = x.Locacion.NombreZona5,
                    RecargaZona5 = x.RecargaZona5,
                    DescuentoPorcentualZona5 = x.DescuentoPorcentualZona5,
                    MontoRecorteZona5 = x.MontoRecorteZona5,
                    PeriodoRecargaZona1 = (Jerarquia.PeriodosRecarga)x.PeriodoRecargaZona1,
                    PeriodoRecargaZona2 = (Jerarquia.PeriodosRecarga)x.PeriodoRecargaZona2,
                    PeriodoRecargaZona3 = (Jerarquia.PeriodosRecarga)x.PeriodoRecargaZona3,
                    PeriodoRecargaZona4 = (Jerarquia.PeriodosRecarga)x.PeriodoRecargaZona4,
                    PeriodoRecargaZona5 = (Jerarquia.PeriodosRecarga)x.PeriodoRecargaZona5,

                })
                .Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            ISheet sheet = workbook.CreateSheet("Jerarquía");

            int amountOfColumns = 0;
            IRow headerRow = sheet.CreateRow(0);
            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locación");
            //headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Máximo Efectivo");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 1");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Período Recarga Zona 1");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recarga Zona 1");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recorte Zona 1");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descuento Porcentual Zona 1");


            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 2");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Período Recarga Zona 2");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recarga Zona 2");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recorte Zona 2");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descuento Porcentual Zona 2");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 3");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Período Recarga Zona 3");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recarga Zona 3");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recorte Zona 3");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descuento Porcentual Zona 3");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 4");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Período Recarga Zona 4");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recarga Zona 4");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recorte Zona 4");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descuento Porcentual Zona 4");

            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona 5");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Período Recarga Zona 5");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recarga Zona 5");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Monto Recorte Zona 5");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Descuento Porcentual Zona 5");
            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }

            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var jerarquia in jerarquias.ToList())
            {
                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;
                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.OperadorNombre);

                row.CreateCell(colIdx++).SetCellValue(jerarquia.Nombre);
                row.CreateCell(colIdx++).SetCellValue(jerarquia.Locacion);
                //row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.MontoMaximoEfectivo));
                
                if (jerarquia.NombreZona1 != null)
                {
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.NombreZona1);
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.PeriodoRecargaZona1.ToString());
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.RecargaZona1));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.MontoRecorteZona1));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.DescuentoPorcentualZona1).ToString() + "%");
                }
                else
                {
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                }

                if (jerarquia.NombreZona2 != null)
                {
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.NombreZona2);
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.PeriodoRecargaZona2.ToString());
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.RecargaZona2));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.MontoRecorteZona2));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.DescuentoPorcentualZona2).ToString() + "%");
                }
                else
                {
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                }

                if (jerarquia.NombreZona3 != null)
                {
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.NombreZona3);
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.PeriodoRecargaZona3.ToString());
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.RecargaZona3));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.MontoRecorteZona3));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.DescuentoPorcentualZona3).ToString() + "%");
                }
                else
                {
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                }

                if (jerarquia.NombreZona4 != null)
                {
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.NombreZona4);
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.PeriodoRecargaZona4.ToString());
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.RecargaZona4));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.MontoRecorteZona4));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.DescuentoPorcentualZona4).ToString() + "%");
                }
                else
                {
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                }

                if (jerarquia.NombreZona5 != null)
                {
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.NombreZona5);
                    row.CreateCell(colIdx++).SetCellValue(jerarquia.PeriodoRecargaZona5.ToString());
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.RecargaZona5));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.MontoRecorteZona5));
                    row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(jerarquia.DescuentoPorcentualZona5).ToString() + "%");
                }
                else
                {
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                    row.CreateCell(colIdx++).SetCellType(CellType.Blank);
                }                

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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Jerarquias " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        private void ServersideFieldValidation(JerarquiaViewModel jerarquia, ModelStateDictionary modelState)
        {
            if (!jerarquia.Zona1Activa || jerarquia.PeriodoRecargaZona1 == JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                ModelState.Remove("RecargaZona1");
                ModelState.Remove("DescuentoPorcentualZona1");
                ModelState.Remove("MontoRecorteZona1");
                ModelState.Remove("PeriodoRecargaZona1");
            }
            else
            {
                if (jerarquia.MontoRecorteZona1 == null)
                {
                    ModelState.AddModelError("MontoRecorteZona1", "Campo Obligatorio");
                }
                if (jerarquia.PeriodoRecargaZona1 == null)
                {
                    ModelState.AddModelError("PeriodoRecargaZona1", "Campo Obligatorio");
                }
            }

            if (!jerarquia.Zona2Activa || jerarquia.PeriodoRecargaZona2 == JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                ModelState.Remove("RecargaZona2");
                ModelState.Remove("DescuentoPorcentualZona2");
                ModelState.Remove("MontoRecorteZona2");
                ModelState.Remove("PeriodoRecargaZona2");
            }
            else
            {
                if (jerarquia.MontoRecorteZona2 == null)
                {
                    ModelState.AddModelError("MontoRecorteZona2", "Campo Obligatorio");
                }
                if (jerarquia.PeriodoRecargaZona2 == null)
                {
                    ModelState.AddModelError("PeriodoRecargaZona2", "Campo Obligatorio");
                }
            }

            if (!jerarquia.Zona3Activa || jerarquia.PeriodoRecargaZona3 == JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                ModelState.Remove("RecargaZona3");
                ModelState.Remove("DescuentoPorcentualZona3");
                ModelState.Remove("MontoRecorteZona3");
                ModelState.Remove("PeriodoRecargaZona3");
            }
            else
            {
                if (jerarquia.MontoRecorteZona3 == null)
                {
                    ModelState.AddModelError("MontoRecorteZona3", "Campo Obligatorio");
                }
                if (jerarquia.PeriodoRecargaZona3 == null)
                {
                    ModelState.AddModelError("PeriodoRecargaZona3", "Campo Obligatorio");
                }
            }

            if (!jerarquia.Zona4Activa || jerarquia.PeriodoRecargaZona4 == JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                ModelState.Remove("RecargaZona4");
                ModelState.Remove("DescuentoPorcentualZona4");
                ModelState.Remove("MontoRecorteZona4");
                ModelState.Remove("PeriodoRecargaZona4");
            }
            else
            {
                if (jerarquia.MontoRecorteZona4 == null)
                {
                    ModelState.AddModelError("MontoRecorteZona4", "Campo Obligatorio");
                }
                if (jerarquia.PeriodoRecargaZona4 == null)
                {
                    ModelState.AddModelError("PeriodoRecargaZona4", "Campo Obligatorio");
                }
            }

            if (!jerarquia.Zona5Activa || jerarquia.PeriodoRecargaZona5 == JerarquiaViewModel.PeriodosRecarga.Ninguno)
            {
                ModelState.Remove("RecargaZona5");
                ModelState.Remove("DescuentoPorcentualZona5");
                ModelState.Remove("MontoRecorteZona5");
                ModelState.Remove("PeriodoRecargaZona5");
            }
            else
            {
                if (jerarquia.MontoRecorteZona5 == null)
                {
                    ModelState.AddModelError("MontoRecorteZona5", "Campo Obligatorio");
                }
                if (jerarquia.PeriodoRecargaZona5 == null)
                {
                    ModelState.AddModelError("PeriodoRecargaZona5", "Campo Obligatorio");
                }
            }
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

        public JsonResult GetZonasByLocacion(string locacionID)
        {
            Guid locacionGuid = new Guid(locacionID);

            return Json(GetListZonas(locacionGuid));
        }

        public List<string> GetListZonas(Guid locacionID)
        {
            List<string> list = new List<string>();

            if (locacionID != Guid.Empty)
            {
                Locacion locacion = db.Locaciones.Where(x => x.LocacionID == locacionID).First();

                list.Add(locacion.NombreZona1 ?? "");
                list.Add(locacion.NombreZona2 ?? "");
                list.Add(locacion.NombreZona3 ?? "");
                list.Add(locacion.NombreZona4 ?? "");
                list.Add(locacion.NombreZona5 ?? "");
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    list.Add(string.Empty);
                }
            }

            return list;
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

        public JsonResult GetAllJerarquias()
        {
            var operadorID = GetUserOperadorID();

            //var jerarquias = db.Jerarquias.Where(x => operadorID == Guid.Empty || x.Locacion.OperadorID == operadorID)
            //    .Select(x => new
            //    {
            //        JerarquiaID = x.JerarquiaID,
            //        OperadorNombre = x.Locacion.Operador.Nombre,
            //        Locacion = x.Locacion.Nombre,
            //        NombreZona1 = x.Locacion.NombreZona1,
            //        NombreZona2 = x.Locacion.NombreZona2,
            //        NombreZona3 = x.Locacion.NombreZona3,
            //        NombreZona4 = x.Locacion.NombreZona4,
            //        NombreZona5 = x.Locacion.NombreZona5,
            //        DescuentoPorcentualZona1 = x.DescuentoPorcentualZona1,
            //        DescuentoPorcentualZona2 = x.DescuentoPorcentualZona2,
            //        DescuentoPorcentualZona3 = x.DescuentoPorcentualZona3,
            //        DescuentoPorcentualZona4 = x.DescuentoPorcentualZona4,
            //        DescuentoPorcentualZona5 = x.DescuentoPorcentualZona5,
            //        MontoRecorteZona1 = x.MontoRecorteZona1,
            //        MontoRecorteZona2 = x.MontoRecorteZona2,
            //        MontoRecorteZona3 = x.MontoRecorteZona3,
            //        MontoRecorteZona4 = x.MontoRecorteZona4,
            //        MontoRecorteZona5 = x.MontoRecorteZona5,
            //        Nombre = x.Nombre,
            //        RecargaZona1 = x.RecargaZona1,
            //        RecargaZona2 = x.RecargaZona2,
            //        RecargaZona3 = x.RecargaZona3,
            //        RecargaZona4 = x.RecargaZona4,
            //        RecargaZona5 = x.RecargaZona5,
            //        PeriodoRecargaZona1 = x.PeriodoRecargaZona1,
            //        PeriodoRecargaZona2 = x.PeriodoRecargaZona2,
            //        PeriodoRecargaZona3 = x.PeriodoRecargaZona3,
            //        PeriodoRecargaZona4 = x.PeriodoRecargaZona4,
            //        PeriodoRecargaZona5 = x.PeriodoRecargaZona5
            //    });
            var jerarquias = db.Jerarquias.Where(x => operadorID == Guid.Empty || x.Locacion.OperadorID == operadorID).ToList()
                .Select(x => JerarquiaViewModel.From(x));


            return Json(jerarquias.ToArray(), JsonRequestBehavior.AllowGet);

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
