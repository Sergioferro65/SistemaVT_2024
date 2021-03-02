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
        public ActionResult Index(long? operador,string topic, long? id)
        {
            _userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            Task.Run(() => RegistrarPago(topic, id,operador));
            return Json(new { result = "OK"},JsonRequestBehavior.AllowGet);
        }
        
        private void RegistrarPago(string topic, long? id,long? operador)


        {

            if (id == null) {
                Log.Info("No existe id");
                return;
            }

            Operador op = db.Operadores.FirstOrDefault(o=>  o.Numero == operador);

            if (op == null) {
                Log.Info("No existe el operador número:" + operador);
                return;
            }

            string clientId = ConfigurationManager.AppSettings["ID_"+ op.Numero.ToString() ];
            string secretToken = ConfigurationManager.AppSettings["TOKEN_"+ op.Numero.ToString()];


            if (clientId == null || secretToken == null) {
                Log.Info("Las credenciales del operador número:" + operador+" no estan registradas");
                return;
            }

            MercadoPago.SDK.CleanConfiguration();
            //MercadoPago.SDK.ClientId = op.ClientId;
            //MercadoPago.SDK.ClientSecret = op.SecretToken;
            MercadoPago.SDK.ClientId = clientId;
            MercadoPago.SDK.ClientSecret = secretToken;


            Log.Info("Llega notificacion de pago al sistema: topic=" + topic + ", id=" + id+"Operador="+operador);

            if (topic != "payment")
                return;

            if (db.MercadoPago.FirstOrDefault((e) => e.Comprobante == id.ToString() && e.Entidad == "MP") != null)
            {
                Log.Info("La notificación ya fue recibida y procesada anteriormente");
                return;
            }

            decimal monto = 0;
            Guid maquinaId = Guid.Empty;
            try
            {


                

                Payment payment = Payment.FindById(id);

                if ( payment.Errors == null)
                {
                    Log.Info("El pago fue encontrado y se encuentra procesando los datos");

                    if (payment.Status == PaymentStatus.approved)
                    {

                        monto = (decimal)payment.TransactionAmount.Value;

                        Log.Info("External Reference:"+ payment.ExternalReference);

                        //maquinaId = new Guid(payment.ExternalReference);

                        Guid maqId = new Guid((string)payment.ExternalReference);

                        Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == maqId).FirstOrDefault();

                        if (maquina != null) {
                            var paymentEntity = new MercadoPagoTable
                            {
                                Fecha = DateTime.Now,
                                Monto = monto,
                                MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.ACREDITADO,
                                MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.EN_PROCESO,
                                Maquina = maquina,
                                FechaModificacionEstadoTransmision = null,
                                Comprobante = id.ToString(),
                                Entidad = "MP"
                            };

                            db.MercadoPago.Add(paymentEntity);
                            db.SaveChanges();
                            EnviarPagoAMaquina(paymentEntity);
                        } else {
                            Log.Error("El pago no esta aprobado: " + id);
                        }
                    }
                    else {
                        Log.Error("Mercado Pago status:" + payment.Status);
                    }

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

                        MercadoPagoTable entity = db.MercadoPago.Where(x => x.Comprobante == mercadoPago.Comprobante).First();

                        MercadoPago.SDK.CleanConfiguration();
                        MercadoPago.SDK.ClientId = entity.Maquina.Operador.ClientId;
                        MercadoPago.SDK.ClientSecret = entity.Maquina.Operador.SecretToken;

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
                                entity.Descripcion = "Error al conectar socket";
                                entity.FechaModificacionEstadoTransmision = DateTime.Now;
                                Log.Error("No se pudo realizar la conexión y se devolvio el dinero", e);
                                db.Entry(entity).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                Log.Info("Ya se devolvio el comprobante: " + mercadoPago.Comprobante);
                                entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.AVISO_FALLIDO ;
                                entity.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                                entity.Descripcion = "Error al conectar socket y al conectar servidor de MP";
                                entity.FechaModificacionEstadoTransmision = DateTime.Now;
                                Log.Error("No se pudo realizar la conexión y no se pudo devolver el dinero", e);
                                db.Entry(entity).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        } else
                        {
                            Log.Info("No se encontro el comprobante: " + mercadoPago.Comprobante);
                        }
                    }
                }
            }
        }


    }
}