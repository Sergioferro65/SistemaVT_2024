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
using BugsMVC.Security;
using BugsMVC.Commands;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class ArticuloAsignacionController : BaseController
    {
        private BugsContext db = new BugsContext();
        
        // GET: ArticuloAsignacion
        public ActionResult Index()
        {
            var articulosAsignaciones = db.ArticulosAsignaciones;
            return View(articulosAsignaciones.ToList());
        }

        // GET: Articulo/Details/5
        [AuthorizeUser(accion = "Detalles", controlador = "ArticuloAsignacion")]
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArticuloAsignacion articuloAsignacion = db.ArticulosAsignaciones.Find(id);
            if (articuloAsignacion == null)
            {
                return HttpNotFound();
            }
            return View(articuloAsignacion);
        }

        // GET: ArticuloAsignacion/Create
        [AuthorizeUser(accion = "Crear", controlador = "ArticuloAsignacion")]
        public ActionResult Create()
        {
            var operadorID = GetUserOperadorID();
            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x =>x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");
            ViewBag.ArticuloID = new SelectList(db.Articulos.Where(x =>x.OperadorID == operadorID).OrderBy(x => x.Nombre), "ArticuloID", "Nombre");
            ViewBag.MaquinaID = new SelectList(string.Empty, "MaquinaID", "NombreAlias");
            ViewBag.NroZona = new SelectList(string.Empty, "ZonaID", "Nombre");
            ViewBag.ShowStockDetails = false;

            ArticuloAsignacion articuloAsignacion = new ArticuloAsignacion();
            return View(articuloAsignacion);
        }

        // POST: ArticuloAsignacion/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ArticuloID,LocacionID,MaquinaID,NroZona,Precio,AlarmaActiva,Capacidad,AlarmaMuyBajo,AlarmaBajo,ControlStock")] ArticuloAsignacion articuloAsignacion)
        {
            var operadorID = GetUserOperadorID();
            var existePrecio = db.ArticulosAsignaciones.Any(x => x.LocacionID == articuloAsignacion.LocacionID
                                                     && (x.NroZona.HasValue? x.NroZona.Value:0)
                                                     == (articuloAsignacion.NroZona.HasValue? articuloAsignacion.NroZona:0)
                                                     && (x.MaquinaID.HasValue?x.MaquinaID.Value:Guid.Empty) 
                                                     == (articuloAsignacion.MaquinaID.HasValue?articuloAsignacion.MaquinaID.Value:Guid.Empty)
                                                     && x.Precio == articuloAsignacion.Precio
                                                     && x.Id != articuloAsignacion.Id
                                                     );

            var existeMaquina = db.ArticulosAsignaciones.Any(x => x.MaquinaID.HasValue && articuloAsignacion.MaquinaID.HasValue
                                         && x.MaquinaID.Value == articuloAsignacion.MaquinaID.Value
                                         && x.ArticuloID == articuloAsignacion.ArticuloID
                                         && x.Id != articuloAsignacion.Id
                                         );

            if (existePrecio)
            {
                ModelState.AddModelError("Precio", "Ya existe un producto con ese precio en el mismo nivel.");
            }

            if (existeMaquina)
            {
                ModelState.AddModelError("Maquina", "Ya existe el articulo para la máquina seleccionada.");
            }

            if (operadorID == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Por favor seleccione un Operador.");
            }

            if (articuloAsignacion.ControlStock && (articuloAsignacion.AlarmaBajo == null ||
                articuloAsignacion.AlarmaMuyBajo == null || articuloAsignacion.Capacidad == null))
            {
                ModelState.AddModelError("ControlStock", "Debe completar los campos de Alarma Bajo,Alarma Muy Bajo y Capacidad");
            }

            if (ModelState.IsValid)
            {
                articuloAsignacion.Id = Guid.NewGuid();
                db.ArticulosAsignaciones.Add(articuloAsignacion);

                if (articuloAsignacion.ControlStock)
                {
                    //Se crea el Stock con cantidad 0 
                    var command = new CreateStockCommand();
                    command.Configure(Guid.Empty, articuloAsignacion.Id, 0, db, User.Identity.GetUserId()).Execute();
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre",articuloAsignacion.LocacionID);
            ViewBag.ArticuloID = new SelectList(db.Articulos.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "ArticuloID", "Nombre", articuloAsignacion.ArticuloID);

            List<SelectListItem> zonas = GetZonasList(articuloAsignacion.LocacionID); 
            ViewBag.NroZona = new SelectList(zonas, "Value", "Text", articuloAsignacion.NroZona.HasValue ? articuloAsignacion.NroZona.Value.ToString()
                                                                                            : "Seleccionar Zona");

            ViewBag.MaquinaID = new SelectList(db.Maquinas.Where(x => x.Zona == articuloAsignacion.NroZona && x.LocacionID == articuloAsignacion.LocacionID).ToList().Select(y => new {
                MaquinaID = y.MaquinaID,
                texto = y.NombreAlias != null ? y.MarcaModelo.MarcaModeloNombre + " - " + y.NumeroSerie + '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
                }),
                    "MaquinaID", "texto", articuloAsignacion.MaquinaID.HasValue ? articuloAsignacion.MaquinaID.Value.ToString() :
                                                                                        "Seleccionar Máquina");
            ViewBag.ShowStockDetails = articuloAsignacion.ControlStock && articuloAsignacion.MaquinaID != Guid.Empty;
            return View(articuloAsignacion);
        }

        // GET: Articulo/Edit/5
        [AuthorizeUser(accion = "Editar", controlador = "ArticuloAsignacion")]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ArticuloAsignacion articuloAsignacion = db.ArticulosAsignaciones.Find(id);

            if (articuloAsignacion == null)
            {
                return HttpNotFound();
            }

            //var operadorID = articuloAsignacion.Articulo.OperadorID;
            var operadorID = (GetUserOperadorID() == Guid.Empty) ? articuloAsignacion.Locacion.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre", articuloAsignacion.LocacionID);
            ViewBag.ArticuloID = new SelectList(db.Articulos.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "ArticuloID", "Nombre", articuloAsignacion.ArticuloID);

            
            List<SelectListItem> zonas = GetZonasList(articuloAsignacion.LocacionID);
            ViewBag.NroZona = new SelectList(zonas, "Value", "Text", articuloAsignacion.NroZona.HasValue ? articuloAsignacion.NroZona.Value.ToString()
                                                                                            : "Seleccionar Zona");
            ViewBag.MaquinaID = new SelectList(db.Maquinas.Where(x => x.Zona == articuloAsignacion.NroZona && x.LocacionID == articuloAsignacion.LocacionID).ToList().Select(y => new {
                MaquinaID = y.MaquinaID,
                texto = y.NombreAlias != null ? y.MarcaModelo.MarcaModeloNombre + " - " + y.NumeroSerie + '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
            }), "MaquinaID", "texto", articuloAsignacion.MaquinaID.HasValue ? articuloAsignacion.MaquinaID.Value.ToString() : "Seleccionar Máquina");
            ViewBag.ShowStockDetails = articuloAsignacion.ControlStock && articuloAsignacion.MaquinaID != Guid.Empty;

            articuloAsignacion.ControlStockInicio = articuloAsignacion.ControlStock;

            return View(articuloAsignacion);
        }

        // POST: Articulo/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ArticuloID,LocacionID,MaquinaID,NroZona,Precio,AlarmaActiva,Capacidad,AlarmaMuyBajo,AlarmaBajo,ControlStock,ControlStockInicio")] ArticuloAsignacion articuloAsignacion)
        {
            var existePrecio = db.ArticulosAsignaciones.Any(x => x.LocacionID == articuloAsignacion.LocacionID
                                                                && (x.NroZona.HasValue ? x.NroZona.Value : 0)
                                                                == (articuloAsignacion.NroZona.HasValue ? articuloAsignacion.NroZona : 0)
                                                                && (x.MaquinaID.HasValue ? x.MaquinaID.Value : Guid.Empty)
                                                                == (articuloAsignacion.MaquinaID.HasValue ? articuloAsignacion.MaquinaID.Value : Guid.Empty)
                                                                && x.Precio == articuloAsignacion.Precio
                                                                && x.Id != articuloAsignacion.Id
                                                                );


            var existeMaquina = db.ArticulosAsignaciones.Any(x => x.MaquinaID.HasValue && articuloAsignacion.MaquinaID.HasValue
                             && x.MaquinaID.Value == articuloAsignacion.MaquinaID.Value
                             && x.ArticuloID == articuloAsignacion.ArticuloID
                             && x.Id != articuloAsignacion.Id
                             );

            if (existePrecio)
            {
                ModelState.AddModelError("Precio", "Ya existe un producto con ese precio en el mismo nivel.");
            }

            if (existeMaquina)
            {
                ModelState.AddModelError("Maquina", "Ya existe el articulo para la máquina seleccionada.");
            }

            if (articuloAsignacion.ControlStock && articuloAsignacion.AlarmaActiva.HasValue && articuloAsignacion.AlarmaActiva.Value && (articuloAsignacion.AlarmaBajo == null ||
               articuloAsignacion.AlarmaMuyBajo == null || articuloAsignacion.Capacidad == null))
            {
                ModelState.AddModelError("AlarmaActiva", "Debe completar los campos de Alarma Bajo,Alarma Muy Bajo y Capacidad");
            }

            if (articuloAsignacion.ControlStock && (articuloAsignacion.AlarmaBajo == null ||
                articuloAsignacion.AlarmaMuyBajo == null || articuloAsignacion.Capacidad == null))
            {
                ModelState.AddModelError("ControlStock", "Debe completar los campos de Alarma Bajo,Alarma Muy Bajo y Capacidad");
            }

            if (ModelState.IsValid)
            {
                //if (!articuloAsignacion.ControlStock || (articuloAsignacion.AlarmaActiva.HasValue && !articuloAsignacion.AlarmaActiva.Value))
                //{
                //    articuloAsignacion.AlarmaBajo = null;
                //    articuloAsignacion.AlarmaMuyBajo = null;
                //    articuloAsignacion.Capacidad = null;
                //    articuloAsignacion.AlarmaActiva = null;
                //}
                //    var a = db.ArticulosAsignaciones.Find(articuloAsignacion.Id);
                //Se crea el Stock con cantidad 0 

                //var command = new CreateStockCommand();
                //StockViewModel stockViewModel = new StockViewModel();

                //if (articuloAsignacion.ControlStock && articuloAsignacion.Stocks == null)
                //{
                //    command.Configure(Guid.Empty, articuloAsignacion.Id, 0, db, User.Identity.GetUserId()).Execute();
                //}

                if (articuloAsignacion.ControlStock != articuloAsignacion.ControlStockInicio)
                {
                    if (articuloAsignacion.ControlStock)
                    {
                        var command = new CreateStockCommand();
                        command.Configure(Guid.Empty, articuloAsignacion.Id, 0, db, User.Identity.GetUserId()).Execute();
                    }
                    else
                    {
                        var stocks = db.Stocks.Where(x => x.ArticuloAsignacionID == articuloAsignacion.Id);
                        var stocksID = stocks.Select(x => x.StockID);
                        var stocksHistoricos = db.StocksHistoricos.Where(x => stocksID.Contains(x.StockID.Value));

                        db.StocksHistoricos.RemoveRange(stocksHistoricos);
                        db.Stocks.RemoveRange(stocks);
                    }
                }

                db.Entry(articuloAsignacion).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var operadorID = (GetUserOperadorID() == Guid.Empty && articuloAsignacion.Locacion != null) ? articuloAsignacion.Locacion.OperadorID : GetUserOperadorID();
            ViewBag.OperadorID = operadorID;
            ViewBag.LocacionID = new SelectList(db.Locaciones.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "LocacionID", "Nombre");
            ViewBag.ArticuloID = new SelectList(db.Articulos.Where(x => x.OperadorID == operadorID).OrderBy(x => x.Nombre), "ArticuloID", "Nombre");

            List<SelectListItem> zonas = GetZonasList(articuloAsignacion.LocacionID);
            ViewBag.NroZona = new SelectList(zonas, "Value", "Text", articuloAsignacion.NroZona.HasValue ? articuloAsignacion.NroZona.Value.ToString()
                                                                                            : "Seleccionar Zona");

            ViewBag.MaquinaID = new SelectList(db.Maquinas.Where(x => x.Zona == articuloAsignacion.NroZona && x.LocacionID == articuloAsignacion.LocacionID).ToList().Select(y => new {
                MaquinaID = y.MaquinaID,
                texto = y.NombreAlias != null ? y.MarcaModelo.MarcaModeloNombre + " - " + y.NumeroSerie + '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
                }), "MaquinaID", "texto", articuloAsignacion.MaquinaID.HasValue ? articuloAsignacion.MaquinaID.Value.ToString() : "Seleccionar Máquina");

            ViewBag.ShowStockDetails = articuloAsignacion.ControlStock && articuloAsignacion.MaquinaID != Guid.Empty;
            return View(articuloAsignacion);
        }

        // GET: Articulo/Delete/5
        [AuthorizeUser(accion = "Eliminar", controlador = "ArticuloAsignacion")]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArticuloAsignacion articuloAsignacion = db.ArticulosAsignaciones.Find(id);
            if (articuloAsignacion == null)
            {
                return HttpNotFound();
            }
            return View(articuloAsignacion);
        }

        // POST: Articulo/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            ArticuloAsignacion articuloAsignacion = db.ArticulosAsignaciones.Find(id);

            var stocks = db.Stocks.Where(x => x.ArticuloAsignacionID == articuloAsignacion.Id);
            var stocksID = stocks.Select(x => x.StockID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => stocksID.Contains(x.StockID.Value));

            db.StocksHistoricos.RemoveRange(stocksHistoricos);
            db.Stocks.RemoveRange(stocks);
            db.ArticulosAsignaciones.Remove(articuloAsignacion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetMaquinas(string nroZona , string locacionID)
        {
            if (nroZona == string.Empty || locacionID == string.Empty)
                return Json(new SelectList(string.Empty, "MaquinaID", "NumeroSerie"));

            int zona = Convert.ToInt32(nroZona);
            Guid locacionGuid = new Guid(locacionID);
            return Json(new SelectList(db.Maquinas.Where(x => x.Zona == zona && x.LocacionID==locacionGuid).ToList().Select( y => new { MaquinaID= y.MaquinaID,
                texto = y.NombreAlias != null? y.MarcaModelo.MarcaModeloNombre+ " - " + y.NumeroSerie+ '(' + y.NombreAlias + ')' : y.MarcaModelo.MarcaModeloNombre + '-' + y.NumeroSerie
                })  , "MaquinaID", "texto"));
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

        public JsonResult GetAllArticulosAsignaciones()
        {
            var operadorID = GetUserOperadorID();
            var articulosAsignados = db.ArticulosAsignaciones.Where(x => operadorID == Guid.Empty || x.Articulo.OperadorID == operadorID)
                .Select(x => new
                {
                    Id = x.Id,
                    OperadorNombre = x.Locacion.Operador.Nombre,
                    ArticuloID = x.ArticuloID,
                    LocacionID = x.LocacionID,
                    MaquinaID = x.MaquinaID,
                    Articulo = x.Articulo.Nombre,
                    Locacion = x.Locacion.Nombre,
                    Maquina = x.Maquina == null ? "Sin asignar" : 
                             (x.Maquina.NombreAlias != null ? x.Maquina.MarcaModelo.MarcaModeloNombre + " - " + x.Maquina.NumeroSerie + "(" + x.Maquina.NombreAlias + ")"
                             : x.Maquina.MarcaModelo.MarcaModeloNombre + " - " + x.Maquina.NumeroSerie
                             ),
                    NroZona =  x.NroZona.HasValue ? (x.NroZona.Value == 1 ? x.Locacion.NombreZona1 : x.NroZona.Value == 2 ? x.Locacion.NombreZona2 :
                                        x.NroZona.Value == 3 ? x.Locacion.NombreZona3 : x.NroZona.Value == 4 ? x.Locacion.NombreZona4 :
                                        x.NroZona.Value == 5 ? x.Locacion.NombreZona5 : string.Empty) : string.Empty,
                    Precio = x.Precio,
                    ControlStock = x.ControlStock,
                    AlarmaActiva = x.AlarmaActiva,
                    AlarmaBajo = x.AlarmaBajo.HasValue ? x.AlarmaBajo.Value.ToString() : String.Empty,
                    AlarmaMuyBajo = x.AlarmaMuyBajo.HasValue ? x.AlarmaMuyBajo.Value.ToString() : String.Empty,
                    Capacidad = x.Capacidad.HasValue ? x.Capacidad.Value.ToString() : String.Empty,
                });
            return Json(articulosAsignados.ToArray(), JsonRequestBehavior.AllowGet);
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

        public JsonResult GetLocacionesAsignacion(string operadorID)
        {
            Guid operadorGuid = new Guid(operadorID);
            List<SelectListItem> locaciones = new List<SelectListItem>();
            var lista = db.ArticulosAsignaciones.Where(x => operadorGuid == Guid.Empty || x.Locacion.OperadorID == operadorGuid && x.MaquinaID!=null).
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
            var lista = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.MaquinaID != null).
                        Select(x => x.Articulo).Distinct();

            foreach (var item in lista)
            {
                articulos.Add(new SelectListItem { Text = item.Nombre, Value = item.ArticuloID.ToString() });
            }

            return Json(new SelectList(articulos, "Value", "Text"));
        }

        public JsonResult GetZonasByLocacionAndArticulo(string articuloID, string locacionID)
        {
            Guid articuloGuid = new Guid(articuloID);
            Guid locacionGuid = new Guid(locacionID);
            List<SelectListItem> zonas = new List<SelectListItem>();

            var lista = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID==articuloGuid && x.MaquinaID != null).
                        Select(x => x.NroZona).Distinct();

            foreach (var item in lista)
            {
                zonas.Add(new SelectListItem { Text = item.Value.ToString(), Value = item.Value.ToString() });
            }

            return Json(new SelectList(zonas, "Value", "Text"));
        }
                          
        public JsonResult GetMaquinasByLocacionAndArticuloAndZona(string articuloID, string locacionID, string ZonaNro)
        {
            Guid articuloGuid = new Guid(articuloID);
            Guid locacionGuid = new Guid(locacionID);
            int zonaNro = Convert.ToInt32(ZonaNro);
            List<SelectListItem> maquinas = new List<SelectListItem>();

            var lista = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID == articuloGuid && x.NroZona == zonaNro && x.MaquinaID != null).
                        Select(x => x.Maquina).Distinct();

            foreach (var item in lista)
            {
                maquinas.Add(new SelectListItem { Text = item.NombreAlias, Value = item.MaquinaID.ToString() });
            }

            return Json(new SelectList(maquinas, "Value", "Text"));
        }
        
        public JsonResult GetArticuloAsignacionID(string articuloID, string locacionID, string ZonaNro, string maquinaID)
        {
            Guid articuloGuid = new Guid(articuloID);
            Guid locacionGuid = new Guid(locacionID);
            Guid maquinaGuid = new Guid(maquinaID);
            int zonaNro = Convert.ToInt32(ZonaNro);
            List<SelectListItem> articulosAsignaciones = new List<SelectListItem>();

            var articuloAsignacion = db.ArticulosAsignaciones.Where(x => x.LocacionID == locacionGuid && x.ArticuloID == articuloGuid && x.NroZona == zonaNro 
                                                && x.MaquinaID == maquinaGuid).FirstOrDefault();

            Stock stock = articuloAsignacion.Stocks.FirstOrDefault();

            return Json(new { ArticuloAsignacionID = articuloAsignacion.Id,
                              StockID = (stock!= null)?stock.StockID:Guid.Empty,
                              Cantidad = (stock != null) ? stock.Cantidad : 0,
                              Capacidad = articuloAsignacion.Capacidad }, JsonRequestBehavior.AllowGet);
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
            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => operadorID == Guid.Empty || x.Locacion.OperadorID == operadorID).ToList().Where(filters);

            XSSFWorkbook workbook = new XSSFWorkbook();
            short doubleFormat = workbook.CreateDataFormat().GetFormat("$#,0.00");

            ISheet sheet = workbook.CreateSheet("Artículos");

            int amountOfColumns = 0;

            IRow headerRow = sheet.CreateRow(0);

            if (esSuperAdmin)
                headerRow.CreateCell(amountOfColumns++).SetCellValue("Operador");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Locación");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Máquina");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Nombre Zona");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Artículo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Precio");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Control de Stock");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Alarma Activa");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Alarma Bajo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Alarma Muy Bajo");
            headerRow.CreateCell(amountOfColumns++).SetCellValue("Capacidad");

            XSSFCellStyle headerCellStyle = ExcelHelper.GetHeaderCellStyle(workbook);

            for (int i = 0; i < amountOfColumns; i++)
            {
                headerRow.Cells[i].CellStyle = headerCellStyle;
            }
            
            var rowNumber = 1;
            int colIdx;

            XSSFCellStyle defaultCellStyle = ExcelHelper.GetDefaultCellStyle(workbook);

            XSSFCellStyle dateCellStyle = ExcelHelper.GetDefaultCellStyle(workbook, isForDate: true);

            foreach (var item in articulosAsignaciones.ToList())
            {

                IRow row = sheet.CreateRow(rowNumber++);

                colIdx = 0;

                if (esSuperAdmin)
                    row.CreateCell(colIdx++).SetCellValue(item.Locacion.Operador.Nombre);
                row.CreateCell(colIdx++).SetCellValue(item.Locacion.Nombre);
                row.CreateCell(colIdx++).SetCellValue(item.MaquinaID.HasValue ? item.Maquina.getDescripcionMaquina() : "Sin asignar");
                row.CreateCell(colIdx++).SetCellValue(
                                        item.NroZona.HasValue ? (item.NroZona.Value == 1 ? item.Locacion.NombreZona1 : item.NroZona.Value == 2 ? item.Locacion.NombreZona2 :
                                        item.NroZona.Value == 3 ? item.Locacion.NombreZona3 : item.NroZona.Value == 4 ? item.Locacion.NombreZona4 :
                                        item.NroZona.Value == 5 ? item.Locacion.NombreZona5 : string.Empty) : string.Empty);
                row.CreateCell(colIdx++).SetCellValue(item.Articulo.Nombre);
                row.CreateCell(colIdx++).SetCellValue(Convert.ToDouble(item.Precio));
                row.CreateCell(colIdx++).SetCellValue((item.ControlStock) ? "Si":"No");
                row.CreateCell(colIdx++).SetCellValue(((item.AlarmaActiva.HasValue) ? (item.AlarmaActiva.Value) ? "Si":"No" : "-"));
                row.CreateCell(colIdx++).SetCellValue(item.AlarmaBajo.HasValue ? item.AlarmaBajo.Value.ToString() : String.Empty);
                row.CreateCell(colIdx++).SetCellValue(item.AlarmaMuyBajo.HasValue ? item.AlarmaMuyBajo.Value.ToString() : String.Empty);
                row.CreateCell(colIdx++).SetCellValue(item.Capacidad.HasValue ? item.Capacidad.Value.ToString() : String.Empty);

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

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Artículos Asignaciones " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        //public JsonResult EliminarStockYReposicionAsociada(string id)
        //{
        //    ArticuloAsignacion articuloAsignacion = db.ArticulosAsignaciones.Find(new Guid(id));

        //    var stocks = db.Stocks.Where(x => x.ArticuloAsignacionID == articuloAsignacion.Id);
        //    var stocksID = stocks.Select(x => x.StockID);
        //    var stocksHistoricos = db.StocksHistoricos.Where(x => stocksID.Contains(x.StockID.Value));

        //    db.StocksHistoricos.RemoveRange(stocksHistoricos);
        //    db.Stocks.RemoveRange(stocks);
        //   // db.SaveChanges();

        //    return Json("Ok", JsonRequestBehavior.AllowGet);
        //}

    }
}