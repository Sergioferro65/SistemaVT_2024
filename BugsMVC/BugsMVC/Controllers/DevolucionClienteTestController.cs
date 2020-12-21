using BugsMVC.DAL;
using BugsMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;



namespace BugsMVC.Controllers
{
    public class DevolucionClienteTestController : BaseController
    {
        private BugsContext db = new BugsContext();
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string ip = ConfigurationManager.AppSettings["IP"];
        private string puerto = ConfigurationManager.AppSettings["Puerto"];
        private string[] tiempoEspera = new string[]{ "0",
                                              ConfigurationManager.AppSettings["TiempoIntento1"],
                                              ConfigurationManager.AppSettings["TiempoIntento2"]};

        private ApplicationUserManager _userManager;

        static HttpClient client = new HttpClient();
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
        public class Rechazo
        {
            public string RefCliente;
            public string MaquinaId;
            public decimal Importe;
            public int EstadoId;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index()
        {
            _userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            Log.Info(json);
                
            return Json(new { result = json }, JsonRequestBehavior.AllowGet);

        }
        

        
        
       

        
    }
}