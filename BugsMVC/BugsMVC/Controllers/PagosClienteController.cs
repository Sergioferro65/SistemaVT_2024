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
using System.Web.Script.Serialization;
using System.Net.Http.Headers;



namespace BugsMVC.Controllers
{
    public class PagosClienteController : BaseController
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

        public class PagoClienteDTO
        {
            public string MaquinaId;
            public decimal Importe;
            public string UrlDevolucion;
            public string TokenCliente;
            public string RefCliente;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index()
        {
            _userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            PagoClienteDTO pago = null;
            try
            {
                pago = JsonConvert.DeserializeObject<PagoClienteDTO>(json);

                Guid maqId = new Guid((string)pago.MaquinaId);
                Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == maqId).FirstOrDefault();

                if (maquina == null)
                {
                    Log.Info("No se encontro la maquina=" + pago.MaquinaId);
                    return Json(new { result = "Invalid MaquinaId" }, JsonRequestBehavior.DenyGet);
                }

                if (maquina.Operador == null)
                {
                    Log.Info("No se encontro el operador de la maquina=" + pago.MaquinaId);
                    return Json(new { result = "Invalid MaquinaId" }, JsonRequestBehavior.DenyGet);
                }

                if (maquina.Operador.SecretToken != pago.TokenCliente)
                {
                    Log.Info("El token que envia el cliente no coincide con el del operador de la maquina=" + pago.MaquinaId);
                    return Json(new { result = "Invalid token" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
               return Json(new { result = "Bad Request" }, JsonRequestBehavior.DenyGet);
            }

            Task.Run(() => RegistrarPago(pago.MaquinaId, pago.Importe, pago.UrlDevolucion, pago.TokenCliente, pago.RefCliente));

            return Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);

        }
        private void RegistrarPago(string maquinaId, decimal importe, string urlDevolucion, string tokenCliente, string refCliente)

        {
            
            Log.Info("Llega notificacion de pago al sistema: maquina=" + maquinaId + ", importe=" + importe);

            var paymentEntity = new MercadoPagoTable
            {
                Fecha = DateTime.Now,
                Monto = importe,
                MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.ACREDITADO,
                MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.EN_PROCESO,
                FechaModificacionEstadoTransmision = null,
                MaquinaId = new Guid((string)maquinaId),
                Comprobante = refCliente,
                Descripcion = urlDevolucion,
                Entidad="BG",
                UrlDevolucion = urlDevolucion 
            };

            db.MercadoPago.Add(paymentEntity);
            db.SaveChanges();
            EnviarPagoAMaquina(paymentEntity.MercadoPagoId, maquinaId, importe, urlDevolucion, tokenCliente, refCliente);
            
        }

        private async void EnviarPagoAMaquina(int idPago, string maquinaId, decimal importe, string urlDevolucion, string tokenCliente, string refCliente)
        {
            int intentos = 0;
            bool volverAintentar = true;
            string mensaje = '$' + idPago.ToString() + ',' + maquinaId.ToString() + ',' + (importe).ToString() + '!';

            while (intentos < 3 && volverAintentar)
            {
                Log.Info("Intento numero: " + intentos + "Volver a intentar en: " + volverAintentar);

                int tiempoSleep = Convert.ToInt32(tiempoEspera[intentos]) * 1000;
                intentos++;
                volverAintentar = false;
                try
                {
                    TcpClient tcpclnt = new TcpClient();

                    Thread.Sleep(tiempoSleep);

                    tcpclnt.Connect(ip, Convert.ToInt32(puerto));

                    Stream stm = tcpclnt.GetStream();

                    ASCIIEncoding asen = new ASCIIEncoding();
                    byte[] ba = asen.GetBytes(mensaje);

                    stm.Write(ba, 0, ba.Length);

                    byte[] bb = new byte[100];
                    int k = stm.Read(bb, 0, 100);

                    tcpclnt.Close();
                }
                catch (Exception e)
                {
                    volverAintentar = true;

                    if (intentos >= 3)
                    {
                        //Devolver 

                        MercadoPagoTable entity = db.MercadoPago.Where(x => x.MercadoPagoId == idPago).First();

                        if (entity != null)
                        {
                            entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;
                            entity.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                            entity.Descripcion = "Error al conectar socket";
                            entity.FechaModificacionEstadoTransmision = DateTime.Now;
                            Log.Error("No se pudo realizar la conexión y se devolvio el dinero", e);
                            db.Entry(entity).State = EntityState.Modified;
                            db.SaveChanges();

                            await EnviarRechazoAsync(entity);
                        }
                        else
                        {
                            Log.Info("No se encontro el pago a devolver: " + idPago);
                        }

                    }
                }
            }
        }

        public class Rechazo
        {
            public string RefCliente;
            public string MaquinaId;
            public decimal Importe;
            public int EstadoId;
        }

        public static async Task<HttpResponseMessage> EnviarRechazoAsync(MercadoPagoTable entity)
        {
            Rechazo rechazo = new Rechazo();
            rechazo.RefCliente = entity.Comprobante;
            rechazo.MaquinaId = entity.MaquinaId.ToString();
            rechazo.Importe = entity.Monto;
            rechazo.EstadoId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;

            var json = new JavaScriptSerializer().Serialize(rechazo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await client.PostAsync(entity.UrlDevolucion , content);

        }
    }
}