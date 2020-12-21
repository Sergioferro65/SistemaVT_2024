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
    public class ModeloTerminalController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: ModeloTerminal
        public ActionResult Index()
        {
            return View(db.ModelosTerminal.ToList());
        }

        public JsonResult GetAllModelosTerminal()
        {
            var modelosTerminal = db.ModelosTerminal.Select(x => new
            {
                ModeloTerminalID = x.ModeloTerminalID,
                Modelo = x.Modelo
            });

            return Json(modelosTerminal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: ModeloTerminal/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModeloTerminal modeloTerminal = db.ModelosTerminal.Find(id);
            if (modeloTerminal == null)
            {
                return HttpNotFound();
            }
            return View(modeloTerminal);
        }

        // GET: ModeloTerminal/Create
        public ActionResult Create()
        {
            return View(new ModeloTerminal());
        }

        // POST: ModeloTerminal/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ModeloTerminalID,Modelo")] ModeloTerminal modeloTerminal)
        {
            var existe = db.ModelosTerminal.Any(x => x.Modelo == modeloTerminal.Modelo);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre seleccionado ya existe.");
            }

            if (ModelState.IsValid)
            {
                modeloTerminal.ModeloTerminalID = Guid.NewGuid();
                db.ModelosTerminal.Add(modeloTerminal);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(modeloTerminal);
        }

        // GET: ModeloTerminal/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModeloTerminal modeloTerminal = db.ModelosTerminal.Find(id);
            if (modeloTerminal == null)
            {
                return HttpNotFound();
            }
            return View(modeloTerminal);
        }

        // POST: ModeloTerminal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Audit]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ModeloTerminalID,Modelo")] ModeloTerminal modeloTerminal)
        {
            var existe = db.ModelosTerminal.Any(x => x.Modelo == modeloTerminal.Modelo
                && x.ModeloTerminalID != modeloTerminal.ModeloTerminalID);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "El nombre que desea guardar ya existe.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(modeloTerminal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(modeloTerminal);
        }

        // GET: ModeloTerminal/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModeloTerminal modeloTerminal = db.ModelosTerminal.Find(id);
            if (modeloTerminal == null)
            {
                return HttpNotFound();
            }
            return View(modeloTerminal);
        }

        // POST: ModeloTerminal/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            ModeloTerminal modeloTerminal = db.ModelosTerminal.Find(id);
            var transaccionesTextos = db.TransaccionesTextos.Where(x => x.ModeloTerminalID == modeloTerminal.ModeloTerminalID);
            var transacciones = db.Transacciones.Where(x => x.ModeloTerminalID == modeloTerminal.ModeloTerminalID);
            var terminales = db.Terminales.Where(x => x.ModeloTerminalID == modeloTerminal.ModeloTerminalID);

            db.Terminales.RemoveRange(terminales);
            db.Transacciones.RemoveRange(transacciones);
            db.TransaccionesTextos.RemoveRange(transaccionesTextos);
            db.ModelosTerminal.Remove(modeloTerminal);
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
