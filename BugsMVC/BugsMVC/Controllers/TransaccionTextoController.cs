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
using BugsMVC.Security;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class TransaccionTextoController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: TransaccionTexto
        public ActionResult Index()
        {
            var transaccionesTextos = db.TransaccionesTextos.Include(t => t.ModeloTerminal);
            return View(transaccionesTextos.ToList());
        }

        public JsonResult GetAllTransaccionesTextos()
        {
            var transaccionesTextos = db.TransaccionesTextos.Include(t => t.ModeloTerminal).ToList()
                .Select(x => new
                {
                    TransaccionTextoID = x.TransaccionTextoID,
                    CodigoTransaccion = x.CodigoTransaccion,
                    ModeloTerminal = x.ModeloTerminal.Modelo,
                    SumaEnEfectivo = x.SumaEnEfectivo,
                    SumaEnRecargas = x.SumaEnRecargas,
                    SumaEnVentas = x.SumaEnVentas,
                    TextoTransaccion = x.TextoTransaccion
                });

            return Json(transaccionesTextos.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: TransaccionTexto/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransaccionTexto transaccionTexto = db.TransaccionesTextos.Find(id);
            if (transaccionTexto == null)
            {
                return HttpNotFound();
            }
            return View(transaccionTexto);
        }

        // GET: TransaccionTexto/Create
        public ActionResult Create()
        {
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal.OrderBy(x => x.Modelo), "ModeloTerminalID", "Modelo");
            return View(new TransaccionTexto());
        }

        // POST: TransaccionTexto/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TransaccionTextoID,CodigoTransaccion,SumaEnEfectivo,SumaEnVentas,SumaEnRecargas,TextoTransaccion,ModeloTerminalID")] TransaccionTexto transaccionTexto)
        {
            if (ModelState.IsValid)
            {
                transaccionTexto.TransaccionTextoID = Guid.NewGuid();
                db.TransaccionesTextos.Add(transaccionTexto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal.OrderBy(x => x.Modelo), "ModeloTerminalID", "Modelo", transaccionTexto.ModeloTerminalID);
            return View(transaccionTexto);
        }

        // GET: TransaccionTexto/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransaccionTexto transaccionTexto = db.TransaccionesTextos.Find(id);
            if (transaccionTexto == null)
            {
                return HttpNotFound();
            }
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal.OrderBy(x => x.Modelo), "ModeloTerminalID", "Modelo", transaccionTexto.ModeloTerminalID);
            return View(transaccionTexto);
        }

        // POST: TransaccionTexto/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransaccionTextoID,CodigoTransaccion,SumaEnEfectivo,SumaEnVentas,SumaEnRecargas,TextoTransaccion,ModeloTerminalID")] TransaccionTexto transaccionTexto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaccionTexto).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ModeloTerminalID = new SelectList(db.ModelosTerminal.OrderBy(x => x.Modelo), "ModeloTerminalID", "Modelo", transaccionTexto.ModeloTerminalID);
            return View(transaccionTexto);
        }

        // GET: TransaccionTexto/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransaccionTexto transaccionTexto = db.TransaccionesTextos.Find(id);
            if (transaccionTexto == null)
            {
                return HttpNotFound();
            }
            return View(transaccionTexto);
        }

        // POST: TransaccionTexto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            TransaccionTexto transaccionTexto = db.TransaccionesTextos.Find(id);
            db.Transacciones.RemoveRange(transaccionTexto.Transacciones);
            db.TransaccionesTextos.Remove(transaccionTexto);
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
