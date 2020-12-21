using BugsMVC.DAL;
using BugsMVC.Models;

using MercadoPago;
using MercadoPago.Resources;
using MercadoPago.DataStructures.Payment;
using MercadoPago.Common;

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

namespace BugsMVC.Controllers
{
  
    public class PagosMPController : BaseController
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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(string topic, long id)
        {
            _userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            Task.Run(() => RegistrarPago(topic, id));
            return Json(new { result = "OK"},JsonRequestBehavior.AllowGet);
        }
        
        private void RegistrarPago(string topic, long id)


        {
            MercadoPago.SDK.CleanConfiguration();
            MercadoPago.SDK.ClientId = ConfigurationManager.AppSettings["ClientIDMercadoPago"];
            MercadoPago.SDK.ClientSecret = ConfigurationManager.AppSettings["ClientSecretMercadoPago"];

            //var mp = new MP(ConfigurationManager.AppSettings["ClientIDMercadoPago"],
            //            ConfigurationManager.AppSettings["ClientSecretMercadoPago"]);

            Log.Info("Llega notificacion de pago al sistema: topic=" + topic + ", id=" + id);

            if (topic != "payment")
                return;

            decimal monto = 0;
            Guid maquinaId = Guid.Empty;
            try
            {
                // Hacemos una busqueda por id de pago
                //Dictionary<string, string> filters = new Dictionary<string, string> { { "id", id } };

                Payment payment = Payment.FindById(id);

                if ( payment.Errors == null)
                {
                    Log.Info("El pago fue encontrado y se encuentra procesando los datos");

                    if (payment.Status == PaymentStatus.approved)
                    {

                        monto = (decimal)payment.TransactionAmount.Value;

                        maquinaId = new Guid(payment.ExternalReference);

                        Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == maquinaId).FirstOrDefault();

                        if (maquina != null) {
                            var paymentEntity = new MercadoPagoTable
                            {
                                Fecha = DateTime.Now,
                                Monto = monto,
                                MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.ACREDITADO,
                                MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.EN_PROCESO,
                                Maquina = maquina,
                                FechaModificacionEstadoTransmision = null,
                                Comprobante = id.ToString()
                            };

                            db.MercadoPago.Add(paymentEntity);
                            db.SaveChanges();
                            EnviarPagoAMaquina(paymentEntity);
                        } else {
                            Log.Error("El pago no esta aprobado: " + id);
                        }
                    }
                    else {
                        Log.Error("No se encontró la máquina" + maquinaId);
                    }



                    //Hashtable searchResult = mp.searchPayment(filters);

                    //if (searchResult != null && searchResult.Count > 0 && (int)searchResult["status"] == 200)
                    //{
                    //    var responseValues = (ArrayList)((Hashtable)searchResult["response"])["results"];
                    //    if (responseValues.Count > 0)
                    //    {
                    //        Log.Info("El pago fue encontrado y se encuentra procesando los datos");
                    //        Hashtable firstItem = (Hashtable)responseValues[0];
                    //        Hashtable resultCollection = (Hashtable)firstItem["collection"];

                    //        string[] referencias = resultCollection["external_reference"].ToString().Split('_');

                    //        if (referencias.Length > 0)
                    //        {
                    //            maquinaId = new Guid(referencias[1].ToString());
                    //            Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == maquinaId).FirstOrDefault();
                    //            if (maquina != null && resultCollection["status"].ToString() == "approved")
                    //            {
                    //                monto = Convert.ToDecimal(resultCollection["total_paid_amount"]);

                    //                //if (resultCollection["status"].ToString() == "approved")
                    //                //{
                    //                mercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.ACREDITADO;
                    //                mercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.EN_PROCESO;
                    //                //}
                    //                var payment = new MercadoPago
                    //                {
                    //                    Fecha = DateTime.Now,
                    //                    Monto = monto,
                    //                    MercadoPagoEstadoFinancieroId = mercadoPagoEstadoFinancieroId,
                    //                    MercadoPagoEstadoTransmisionId = mercadoPagoEstadoTransmisionId,
                    //                    Maquina = maquina,
                    //                    FechaModificacionEstadoTransmision = null,
                    //                    Comprobante = id
                    //                };

                    //                db.MercadoPago.Add(payment);
                    //                db.SaveChanges();

                    //                EnviarPagoAMaquina(payment);
                    //            }
                    //            else
                    //            {
                    //                Log.Error("No se encontró la máquina o el pago no esta aprobado: " + maquinaId);
                    //            }
                    //        }
                    //    }
                }

                else
                {
                    Log.Error("No se encontró el pago: " + id);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Hubo un error procesando el pago, la referencia recibida no cumple el formato dado", ex);
            }
        }

        private void EnviarPagoAMaquina(MercadoPagoTable mercadoPago)
        {
            int intentos = 0;
            bool volverAintentar = true;
            string mensaje = '$' + mercadoPago.MercadoPagoId.ToString()+ ',' + mercadoPago.MaquinaId.ToString()+ ',' + (mercadoPago.Monto * 100).ToString() + '!';

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
                        MercadoPago.SDK.CleanConfiguration();
                        MercadoPago.SDK.ClientId = ConfigurationManager.AppSettings["ClientIDMercadoPago"];
                        MercadoPago.SDK.ClientSecret = ConfigurationManager.AppSettings["ClientSecretMercadoPago"];

                        MercadoPagoTable entity = db.MercadoPago.Where(x => x.Comprobante == mercadoPago.Comprobante).First();
                        long id = 0;
                        long.TryParse(entity.Comprobante, out id);
                        Payment payment = Payment.FindById(id);
                        if (payment.Errors == null)
                        {
                            payment.Refund();
                            if (payment.Status == PaymentStatus.approved)
                            {

                                entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;
                                entity.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                                Log.Error("No se pudo realizar la conexión y se devolvio el dinero", e);
                                db.Entry(entity).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                Log.Info("Ya se devolvio el comprobante: " + mercadoPago.Comprobante);
                                //    //ver como registrar porque no pudo devolver el pago
                            }
                        } else
                        {
                            Log.Info("No se encontro el comprobante: " + mercadoPago.Comprobante);
                        }

                        //MP mp = new MP(entity.Maquina.Operador.ClientId, entity.Maquina.Operador.SecretToken);

                        //Hashtable result = mp.refundPayment(mercadoPago.Comprobante);
                        //if (result["status"].ToString() == "200")
                        //{
                        //    entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;
                        //    entity.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;

                        //    Log.Error("No se pudo realizar la conexión y se devolvio el dinero", e);
                        //    db.Entry(entity).State = EntityState.Modified;
                        //    db.SaveChanges();
                        //}
                        //else
                        //{
                        //    Log.Info("Ya se devolvio el comprobante: " + mercadoPago.Comprobante);
                        //    //ver como registrar porque no pudo devolver el pago
                        //}
                    }
                }
            }
        }


    }
}