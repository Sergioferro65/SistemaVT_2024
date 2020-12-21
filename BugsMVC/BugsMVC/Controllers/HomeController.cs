using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Json;
using BugsMVC.DAL;


namespace BugsMVC.Controllers
{
    public class HomeController : BaseController
    {
        private BugsContext db = new BugsContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetTestData()
        {
            var date = DateTime.Now.AddDays(-7);

            var transacciones = db.Transacciones.Where(x => x.FechaTransaccion < date);
            barChartData chartData = new barChartData();

            if (transacciones.Count() > 0)
            {
                chartData.label = "Ventas";
                chartData.color = "#9289ca";

                List<object[]> ff = new List<object[]>();

                foreach (var transaccion in transacciones)
                {
                    object[] dato = new object[2];

                    dato[0] = transaccion.FechaTransaccion.Value.ToShortDateString();
                    dato[1] = transacciones.Where(x => x.FechaTransaccion == transaccion.FechaTransaccion).Sum(x => x.ValorVenta);

                    ff.Add(dato);
                }

                chartData.data = ff.ToArray();
            }

            return Json(chartData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

        public ActionResult ErrorPage()
        {
            return View();
        }
    } 
  
    public class barChartData
    {
        public string label {get; set;}
        public string color { get; set; }

        public object[] data { get; set; }
    }

    


} 

