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
using BugsMVC.Handlers;
using BugsMVC.Security;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class MarcaModeloController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: MarcaModelo
        public ActionResult Index()
        {
            return View(db.MarcasModelos.ToList());
        }

        public JsonResult GetAllMarcaModelos()
        {
            var marcaModelos = db.MarcasModelos.Select(x => new
            {
                MarcaModeloID = x.MarcaModeloID,
                MarcaModeloNombre = x.MarcaModeloNombre
            });

            return Json(marcaModelos.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: MarcaModelo/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MarcaModelo marcaModelo = db.MarcasModelos.Find(id);
            if (marcaModelo == null)
            {
                return HttpNotFound();
            }
            return View(marcaModelo);
        }

        // GET: MarcaModelo/Create
        public ActionResult Create()
        {
            return View(new MarcaModelo());
        }

        // POST: MarcaModelo/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MarcaModeloID,MarcaModeloNombre")] MarcaModelo marcaModelo)
        {
            var existe = db.MarcasModelos.Any(x => x.MarcaModeloNombre == marcaModelo.MarcaModeloNombre);

            if (existe)
            {
                ModelState.AddModelError("MarcaModeloNombre", "El nombre seleccionado ya existe.");
            }

            if (ModelState.IsValid)
            {
                marcaModelo.MarcaModeloID = Guid.NewGuid();
                db.MarcasModelos.Add(marcaModelo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(marcaModelo);
        }

        // GET: MarcaModelo/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MarcaModelo marcaModelo = db.MarcasModelos.Find(id);
            if (marcaModelo == null)
            {
                return HttpNotFound();
            }
            return View(marcaModelo);
        }

        // POST: MarcaModelo/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MarcaModeloID,MarcaModeloNombre")] MarcaModelo marcaModelo)
        {
            var existe = db.MarcasModelos.Any(x => x.MarcaModeloNombre == marcaModelo.MarcaModeloNombre
                && x.MarcaModeloID != marcaModelo.MarcaModeloID);

            if (existe)
            {
                ModelState.AddModelError("MarcaModeloNombre", "El nombre que desea guardar ya existe.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(marcaModelo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(marcaModelo);
        }

        // GET: MarcaModelo/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MarcaModelo marcaModelo = db.MarcasModelos.Find(id);
            if (marcaModelo == null)
            {
                return HttpNotFound();
            }
            return View(marcaModelo);
        }

        // POST: MarcaModelo/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            MarcaModelo marcaModelo = db.MarcasModelos.Find(id);
            var maquinas = db.Maquinas.Where(x => x.MarcaModeloID == marcaModelo.MarcaModeloID);
            var maquinasID = maquinas.Select(x => x.MaquinaID);
            var transacciones = db.Transacciones.Where(x =>x.MaquinaID.HasValue && maquinasID.Contains(x.MaquinaID.Value));
            var articulosAsignaciones = db.ArticulosAsignaciones.Where(x => x.MaquinaID.HasValue &&
                                                            maquinasID.Contains(x.MaquinaID.Value));
            var articulosAsignacionesID = articulosAsignaciones.Select(x => x.Id);
            var stocks = db.Stocks.Where(x => articulosAsignacionesID.Contains(x.ArticuloAsignacionID));
            var stocksID = stocks.Select(x => x.StockID);
            var stocksHistoricos = db.StocksHistoricos.Where(x => x.StockID.HasValue && stocksID.Contains(x.StockID.Value));

            db.StocksHistoricos.RemoveRange(stocksHistoricos);
            db.Stocks.RemoveRange(stocks);
            db.ArticulosAsignaciones.ToList().ForEach(x => x.MaquinaID = null);
            db.Transacciones.RemoveRange(transacciones);
            db.Maquinas.RemoveRange(maquinas);
            db.MarcasModelos.Remove(marcaModelo);
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
