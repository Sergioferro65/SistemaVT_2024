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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Json;

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
        private string MAQ_9 = "MAQ_99999";
        private int OP_9 = 9999;

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
        public ActionResult Index(long? operador, string topic, long? id)
        {
            _userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            Task.Run(() => RegistrarPago(topic, id, operador));
            return Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
        }

        public void RegistrarPago(string topic, long? id, long? operador)
        {

            if (id == null)
            {
                Log.Info("No existe id");
                return;
            }

            Operador op = db.Operadores.FirstOrDefault(o => o.Numero == operador);

            if (op == null)
            {
                Log.Info("No existe el operador número:" + operador);
                //guardar en la db ESTAO F3 y T4
                Operador op_inexistente = db.Operadores.FirstOrDefault(o => o.Numero == OP_9);
              
                GuardarNoEncontrados(op_inexistente, "Operador Inexistente Número: " + operador.ToString());
                return;
            }

            
            string clientId =  op.ClientId;
            string secretToken = op.SecretToken;

            if (clientId == null || secretToken == null)
            {
                Log.Info("Las credenciales del operador número:" + operador + " no estan registradas");
                return;
            }

            MercadoPago.SDK.CleanConfiguration();
            MercadoPago.SDK.ClientId = clientId;
            MercadoPago.SDK.ClientSecret = secretToken;


            Log.Info("Llega notificacion de pago al sistema: topic=" + topic + ", id=" + id + " ,Operador=" + operador);

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


                if (payment.Errors == null)
                {
                    Log.Info("El pago fue encontrado y se encuentra procesando los datos");

                    if (payment.Status == PaymentStatus.approved)
                    {

                        monto = (decimal)payment.TransactionAmount.Value;

                        /*Log.Info("External Reference:" + payment.ExternalReference + "
                         Sergio Abril 2024 */
                        Log.Info("External Reference:" + payment.ExternalReference +" ,Monto:"+monto);
                        Log.Info("CurrencyId: " + payment.CurrencyId);
                        Log.Info("Date Aproved: " + payment.DateApproved);
                        Log.Info("Payment Method: " + payment.PaymentMethodId);
                        Log.Info("Colector ID: " + payment.CollectorId);
                        Log.Info("Issuer ID: " + payment.IssuerId);

                        Maquina maquina=null;
                        string maqId = string.Empty;
                        string pos_id = "";
                        
                        if (payment.ExternalReference == null )
                        {
                            //Datos del Response Payment
                            var responsepayment = ObtenerResponsePayment(payment);
                            Log.Info("Pos_ID: " + responsepayment.pos_id);


                            if (responsepayment != null && responsepayment.pos_id != string.Empty)
                            {
                                //POS ID
                                pos_id = responsepayment.pos_id;
                                Log.Info("Pos ID obtenido:" + pos_id + " para el operador: " + op.Nombre);
                                maqId = (string)payment.ExternalReference;
                                maquina = db.Maquinas.Where((x) => x.Mensaje == pos_id && x.OperadorID == op.OperadorID).FirstOrDefault();

                            }
                            //else No contiene ExternalReference ni POS_ID entonces maquina es igual a null y realiza la devolución


                        }
                        else
                        {
                           //External Reference de Payment
                            Log.Info("External Reference Actualizado:" + payment.ExternalReference + " para el operador: " + op.Nombre);
                            maqId = (string)payment.ExternalReference;
                            maquina = db.Maquinas.Where((x) => x.NotasService == maqId && x.OperadorID == op.OperadorID).FirstOrDefault();


                        }


                        if (maquina != null)
                        {
                            var paymentEntity = new MercadoPagoTable
                            {
                                Fecha = DateTime.Now,
                                Monto = monto,
                                MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.ACREDITADO,
                                MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.EN_PROCESO,
                                Maquina = maquina,
                                FechaModificacionEstadoTransmision = null,
                                Comprobante = id.ToString(),
                                Entidad = "MP",
                                UrlDevolucion = clientId + "_" + secretToken
                            };

                            db.MercadoPago.Add(paymentEntity);
                            db.SaveChanges();
                            EnviarPagoAMaquina(paymentEntity);
                        }
                        else
                        {

                            Log.Error("No se encontro la maquina: " + maqId + " para el operador: " + op.Nombre + " del pago en curso");
                            var maquinabusq = maqId;
                            //Se realiza la devolución en un maquina definida para el operador
                            maqId = MAQ_9;
                            Maquina maquina_operador = db.Maquinas.Where((x) => x.NotasService == maqId && x.OperadorID == op.OperadorID).FirstOrDefault();

                            if(maquina_operador!=null)
                            {
                                var paymentEntity = new MercadoPagoTable
                                {
                                    Fecha = DateTime.Now,
                                    Monto = monto,
                                    MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.ACREDITADO,
                                    MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.EN_PROCESO,
                                    Maquina = maquina_operador,
                                    FechaModificacionEstadoTransmision = null,
                                    Comprobante = id.ToString(),
                                    Entidad = "MP",
                                    UrlDevolucion = clientId + "_" + secretToken
                                };

                                db.MercadoPago.Add(paymentEntity);
                                db.SaveChanges();

                                GuardarenDevolucion(paymentEntity,"No se encontro la maquina: " + maquinabusq + " para el operador: " + op.Nombre + ". ");
                            }
                            else
                            {
                                Log.Error("No se encontro la maquina: " + maqId + " para el operador: " + op.Nombre + " proceso de devolución");

                            }

                        }
                    }
                    else
                    {
                        Log.Error("Mercado Pago status:" + payment.Status);
                    }

                }

                else
                {
                    Log.Error("No se encontró el pago: " + id);
                    //Registro en db como EstadoFinanciero 3 y estado Transmision 4
                    GuardarNoEncontrados(op, "ID operación Payment sin datos");
                    
                }
            }
            catch (Exception ex)
            {
                //si en EnviarPagoAMaquina hay un error, entonce viene por esta seccion
                Log.Error("Hubo un error procesando el pago, la referencia recibida no cumple el formato dado", ex);
            }
        }

        private void EnviarPagoAMaquina(MercadoPagoTable mercadoPago)
        {
            int intentos = 0;
            bool volverAintentar = true;
            string mensaje = '$' + mercadoPago.MercadoPagoId.ToString() + ',' + mercadoPago.MaquinaId.ToString() + ',' + (mercadoPago.Monto * 100).ToString() + '!';

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
                        Log.Error("No se pudo realizar la conexión ",e);

                        GuardarenDevolucion(mercadoPago, "Error al conectar socket. ");
                    }


                }
            }
        }


        private Boolean GuardarNoEncontrados(Operador op, String descripcion)
        {
            Boolean result = true;

            var maqId = MAQ_9;
            Maquina maquina_operador = db.Maquinas.Where((x) => x.NotasService == maqId && x.OperadorID == op.OperadorID).FirstOrDefault();

            //No se realiza la devolución porque no existe el idPayment
            if(maquina_operador != null)
            {
                MercadoPagoTable entity = new MercadoPagoTable
                {
                    Fecha = DateTime.Now,
                    Monto = 0,
                    Maquina = maquina_operador,
                    MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.AVISO_FALLIDO,
                    MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION,
                    Descripcion = descripcion,
                    Comprobante = "99999999",
                    FechaModificacionEstadoTransmision = DateTime.Now,
                    Entidad = "MP"
                };

                db.MercadoPago.Add(entity);
                db.SaveChanges();
            }


            return result;

        }

        private Boolean GuardarenDevolucion(MercadoPagoTable mercadoPago, string Descripcion)
        {

            Boolean result = true;

            MercadoPagoTable entity = db.MercadoPago.Where(x => x.Comprobante == mercadoPago.Comprobante).First();

            MercadoPago.SDK.CleanConfiguration();
            MercadoPago.SDK.ClientId = entity.Maquina.Operador.ClientId;
            MercadoPago.SDK.ClientSecret = entity.Maquina.Operador.SecretToken;

            long id = 0;
            long.TryParse(entity.Comprobante, out id);
            Payment payment = Payment.FindById(id);
            if (payment.Errors == null)
            {
                //*IMPORTANTE COMENTAR payment.Refund(); PARA QUE NO REALICE DEVOLUCIONES EN UN DEBUG DE PRUEBA
                payment.Refund();
                if (payment.Status == PaymentStatus.approved)
                {

                    entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;
                    entity.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                    entity.Descripcion = Descripcion + "Se realizó la devolución.";// "Error al conectar socket";
                    entity.FechaModificacionEstadoTransmision = DateTime.Now;
                    Log.Error("No se pudo realizar la conexión y se devolvio el dinero");//, e);
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    Log.Info("Ya se devolvio el comprobante: " + mercadoPago.Comprobante);
                    entity.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.AVISO_FALLIDO;
                    entity.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                    entity.Descripcion = Descripcion + " y al conectar servidor de MP";// "Error al conectar socket y al conectar servidor de MP";
                    entity.FechaModificacionEstadoTransmision = DateTime.Now;
                    Log.Error("No se pudo realizar la conexión y no se pudo devolver el dinero");//, e)
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                Log.Info("No se encontro el comprobante: " + mercadoPago.Comprobante);
            }


            return result;
        }

        private PaymentoResponse ObtenerResponsePayment(Payment payment)
        {
            try
            {
                JObject jsonobject = payment.GetJsonSource();
                JsonReader ob = jsonobject.CreateReader();
                JsonSerializer serializer = new JsonSerializer();
                PaymentoResponse responsepayment = serializer.Deserialize<PaymentoResponse>(ob);
                return responsepayment;
            }
            catch (Exception ex)
            {
                Log.Error("Hubo un error en PagosMPController->ObtenerResponsePayment", ex);
                return null;
            }
        }

    }
}