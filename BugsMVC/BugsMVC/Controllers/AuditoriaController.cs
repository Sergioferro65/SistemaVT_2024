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
using BugsMVC.Helpers;

namespace BugsMVC.Controllers
{
    public class AuditoriaController : BaseController
    {
        private BugsContext db = new BugsContext();

        // GET: Auditoria
        public ActionResult Index()
        {
            return View(db.RegistrosAuditoria.ToList());
        }

        // GET: Auditoria/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auditoria auditoria = db.RegistrosAuditoria.Find(id);
            if (auditoria == null)
            {
                return HttpNotFound();
            }
            return View(auditoria);
        }

        public JsonResult GetAllAuditorias()
        {
            var operadorID = ViewHelper.GetCurrentOperadorId();

            var maquinas = db.RegistrosAuditoria
                .Select(x => new
                {
                    AuditoriaID = x.AuditoriaID,
                    UserName = x.UserName,
                    IPAddress = x.IPAddress,
                    AreaAccessed = x.AreaAccessed,
                    TimeAccessed = x.TimeAccessed
                });

            return Json(maquinas.ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteAll()
        {
            //var stocksHistoricos = db.StocksHistoricos.Where(x => stocksID.Contains(x.StockID.Value));

            //db.StocksHistoricos.RemoveRange(stocksHistoricos);

            //db.RegistrosAuditoria.RemoveRange(db.RegistrosAuditoria.ToList());
            db.RegistrosAuditoria.ToList().ForEach(x => db.RegistrosAuditoria.Remove(x));
            db.SaveChanges();

            //objectSet.ToList().ForEach(x => objectSet.Remove(x));

            return Json("Ok",JsonRequestBehavior.AllowGet);
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
