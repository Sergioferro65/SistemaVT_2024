using System;
using System.Collections.Generic;
using System.Linq;
using BugsMVC.DAL;
using System.Net.Mail;
using BugsMVC.Models;
using System.Configuration;

using MercadoPago.Resources;
using MercadoPago.DataStructures.Payment;
using MercadoPago.Common;

using System.Collections;
using System.Data.Entity;
using System.IO;
using BugsMVC.Controllers;
using System.Net.Http;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data;

namespace StockNotifier
{
    class Program
    {
        public static BugsContext db;

        public static async Task Main(string[] args)
        {
            try { 

            writeLog("Main");

                db = new BugsContext();

                var stocks = db.Stocks.ToList();
                var maquinas = db.Maquinas.ToList();
                var mercadoPagos = db.MercadoPago;
                int tiempoMP = 5;

                var mailsStockMuyBajo = new List<Stock>();
                var mailsStockBajo = new List<Stock>();
                var mailsMaquinaSinConexion = new List<Maquina>();
                var mailsMaquinaInhibidas = new List<Maquina>();

                writeLog("Control de stock");

                foreach (var stock in stocks)
                {
                    if (stock.ArticuloAsignacion.ControlStock && stock.ArticuloAsignacion.AlarmaActiva.HasValue && stock.ArticuloAsignacion.AlarmaActiva.Value && (stock.FechaAviso == null || stock.FechaAviso.Value.Date < DateTime.Now.Date))
                    {
                        if (stock.Cantidad < stock.ArticuloAsignacion.AlarmaBajo && stock.Cantidad < stock.ArticuloAsignacion.AlarmaMuyBajo)
                        {
                            mailsStockMuyBajo.Add(stock);
                        }
                        else if (stock.Cantidad < stock.ArticuloAsignacion.AlarmaBajo)
                        {
                            mailsStockBajo.Add(stock);
                        }
                    }
                }

                writeLog("Control Maquinas Conectadas");
                var test = maquinas.Where(x => x.Estado == "Asignada");

                foreach (var maquina in maquinas.Where(x => x.Estado == "Asignada"))
                {
                    if (maquina.FechaUltimaConexion.HasValue && (DateTime.Now - maquina.FechaUltimaConexion.Value).TotalMinutes >= maquina.Operador.TiempoAvisoConexion)
                    {
                        if (maquina.AlarmaActiva.HasValue && maquina.AlarmaActiva == true
                        && (maquina.FechaAviso == null || maquina.FechaAviso.Value.Date < DateTime.Now.Date
                        || (maquina.FechaAviso.Value.Date >= DateTime.Now.Date && maquina.EstadoConexion != "Sin Conexión")))
                        {
                            mailsMaquinaSinConexion.Add(maquina);
                        }

                        if (maquina.EstadoConexion != "Sin Conexión")
                        {
                            maquina.FechaEstado = DateTime.Now;
                            maquina.EstadoConexion = "Sin Conexión";
                        }

                    }
                    else
                    {
                        if (maquina.FechaUltimoOk.HasValue && (DateTime.Now - maquina.FechaUltimoOk.Value).TotalMinutes >= maquina.Operador.TiempoAvisoInhibicion)
                        {
                            if (maquina.AlarmaActiva.HasValue && maquina.AlarmaActiva == true
                            && (maquina.FechaAviso == null || (((DateTime.Now - maquina.FechaUltimaConexion.Value).TotalMinutes < maquina.Operador.TiempoAvisoConexion
                            && maquina.EstadoConexion != "Inactiva") || maquina.FechaAviso.Value.Date < DateTime.Now.Date && maquina.EstadoConexion != "Inactiva")))
                            {
                                mailsMaquinaInhibidas.Add(maquina);
                            }

                            if (maquina.EstadoConexion != "Inactiva")
                            {
                                maquina.FechaEstado = DateTime.Now;
                                maquina.EstadoConexion = "Inactiva";
                            }
                        }
                        else
                        {
                            if (maquina.FechaUltimaConexion.HasValue && maquina.FechaUltimoOk.HasValue
                            && (DateTime.Now - maquina.FechaUltimoOk.Value).TotalMinutes < maquina.Operador.TiempoAvisoInhibicion
                            && (DateTime.Now - maquina.FechaUltimaConexion.Value).TotalMinutes < maquina.Operador.TiempoAvisoConexion
                            )
                            {
                                if (maquina.EstadoConexion != "Activa")
                                {
                                    maquina.FechaEstado = DateTime.Now;
                                    maquina.EstadoConexion = "Activa";
                                }

                                maquina.FechaAviso = null;
                            }
                        }
                    }

                    // db.SaveChanges();

                    var sql = "UPDATE [dbo].[Maquina] SET [FechaEstado] = @FechaEstado, [EstadoConexion] = @EstadoConexion, [FechaAviso] = @FechaAviso WHERE [MaquinaID] = @MaquinaID";

                    db.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@FechaEstado", maquina.FechaEstado),
                        new SqlParameter("@EstadoConexion", maquina.EstadoConexion),
                        new SqlParameter("@FechaAviso", maquina.FechaAviso != null ? maquina.FechaAviso : (object)DBNull.Value),
                        new SqlParameter("@MaquinaID", maquina.MaquinaID)
                    );

                }

                writeLog("Control Devoluciones");

                foreach (var mercadoPago in mercadoPagos.Where(x => x.MercadoPagoEstadoFinancieroId == (int)MercadoPagoEstadoFinanciero.States.ACREDITADO
                                                            && (x.MercadoPagoEstadoTransmisionId != (int)MercadoPagoEstadoTransmision.States.TERMINADO_OK)
                                                            ).ToList())
                {
                    writeLog("Maquina:" + mercadoPago.MaquinaId);

                    bool devolver = true;

                    if (mercadoPago.MercadoPagoEstadoTransmisionId == (int)MercadoPagoEstadoTransmision.States.EN_PROCESO && (DateTime.Now - mercadoPago.Fecha).TotalMinutes < tiempoMP || (mercadoPago.Reintentos == 3))
                    {
                        devolver = false;
                    }

                    if (devolver)
                    {
                        //Devolver dinero
                        Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == mercadoPago.MaquinaId).FirstOrDefault();
                        Operador operador = db.Operadores.Where(x => x.OperadorID == maquina.OperadorID).FirstOrDefault();



                        if (mercadoPago.Comprobante != "" && mercadoPago.Entidad == "MP" && mercadoPago.UrlDevolucion != null)
                        {

                            writeLog("Devolviendo al Operador:" + operador.ClientId);

                            MercadoPago.SDK.CleanConfiguration();
                            MercadoPago.SDK.ClientId = mercadoPago.UrlDevolucion.Split('_')[0];
                            MercadoPago.SDK.ClientSecret = mercadoPago.UrlDevolucion.Split('_')[1];

                            long id = 0;
                            long.TryParse(mercadoPago.Comprobante, out id);
                            writeLog("Buscando comprobante " + mercadoPago.Comprobante + " en MP");
                            Payment payment = Payment.FindById(id);

                            if (payment.Errors == null)
                            {

                                writeLog("Comprobante encontrado");

                                payment.Refund();
                                writeLog("Respuesta Mercado Pago " + payment.Status);
                                if (payment.Status == PaymentStatus.refunded)
                                {
                                    writeLog("Actualizando registro en MercadoPagoTable");
                                    mercadoPago.Descripcion = "Envio ok.";
                                    mercadoPago.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                                    mercadoPago.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;
                                }
                                else
                                {
                                    mercadoPago.Reintentos = mercadoPago.Reintentos + 1;
                                    mercadoPago.Descripcion = "Se realizo el intento de devolución: " + mercadoPago.Reintentos.ToString();
                                    writeLog("Devolución MercadoPagoTable Reintento: " + mercadoPago.Reintentos.ToString());

                                    if (mercadoPago.Reintentos == 3)
                                    {
                                        mercadoPago.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                                        mercadoPago.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.AVISO_FALLIDO;
                                        mercadoPago.Descripcion = "No se logró realizar la devolución, tras 3 intentos.";
                                    }
                                }
                                //db.Entry(mercadoPago).State = EntityState.Modified;
                                //db.SaveChanges();

                                var sql = @"
                                UPDATE [dbo].[MercadoPagoTable]
                                SET 
                                    [MercadoPagoEstadoFinancieroId] = @EstadoFinancieroId,
                                    [MercadoPagoEstadoTransmisionId] = @EstadoTransmisionId,
                                    [Descripcion] = @Descripcion,
                                    [Reintentos] = @Reintentos
                                WHERE 
                                    [MercadoPagoId] = @MercadoPagoId";

                               var parameters = new[]
                               {
                                new SqlParameter("@EstadoFinancieroId", SqlDbType.Int) { Value = mercadoPago.MercadoPagoEstadoFinancieroId },
                                new SqlParameter("@EstadoTransmisionId", SqlDbType.Int) { Value = mercadoPago.MercadoPagoEstadoTransmisionId },
                                new SqlParameter("@Descripcion", SqlDbType.NVarChar, -1) { Value = mercadoPago.Descripcion },
                                new SqlParameter("@Reintentos", SqlDbType.Int) { Value = mercadoPago.Reintentos },
                                new SqlParameter("@MercadoPagoId", SqlDbType.Int) { Value = mercadoPago.MercadoPagoId}
                            };

                             db.Database.ExecuteSqlCommand(sql, parameters);

                            }
                            else
                            {

                                writeLog("No se encontro el pago");
                                //No se encontró el pago,  no deberia suceder.
                                //El clientID o secret son incorrectos
                            }
                            //MercadoPago entity = db.MercadoPago.Where(x => x.Comprobante == Comprobante).First();
                        }
                        else
                        {
                            //Aca deberia registrar un estado correspondiente cuando el operador no tiene cargado clientID o secretToken.
                            writeLog("Se envia mensaje de rechazo la entidad pagadora");
                            HttpResponseMessage response = await PagosClienteController.EnviarRechazoAsync(mercadoPago);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                mercadoPago.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                                mercadoPago.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;
                            }
                            else
                            {
                                writeLog("No se pudo enviar mensaje a la entidad pagadora");
                                mercadoPago.MercadoPagoEstadoTransmisionId = (int)MercadoPagoEstadoTransmision.States.ERROR_CONEXION;
                                mercadoPago.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.AVISO_FALLIDO;
                            }

                            //db.Entry(mercadoPago).State = EntityState.Modified;
                            //db.SaveChanges();
                            var sql = @"
                                UPDATE [dbo].[MercadoPagoTable]
                                SET 
                                    [MercadoPagoEstadoFinancieroId] = @EstadoFinancieroId,
                                    [MercadoPagoEstadoTransmisionId] = @EstadoTransmisionId
                                WHERE 
                                    [MercadoPagoId] = @MercadoPagoId";

                            var parameters = new[]
                            {
                                new SqlParameter("@EstadoFinancieroId", SqlDbType.Int) { Value = mercadoPago.MercadoPagoEstadoFinancieroId },
                                new SqlParameter("@EstadoTransmisionId", SqlDbType.Int) { Value = mercadoPago.MercadoPagoEstadoTransmisionId },
                                new SqlParameter("@MercadoPagoId", SqlDbType.Int) { Value = mercadoPago.MercadoPagoId}
                            };

                            db.Database.ExecuteSqlCommand(sql, parameters);

                        }
                    }
                }

                ProcesarListaStock("SistemaVT - Alarma Stock Muy Bajo", mailsStockMuyBajo);
                ProcesarListaStock("SistemaVT - Alarma Stock Bajo", mailsStockBajo);

                ProcesarListaMaquina("SistemaVT - Alarma Máquina Sin Conexión", mailsMaquinaSinConexion);
                ProcesarListaMaquina("SistemaVT - Alarma Máquina Inactiva", mailsMaquinaInhibidas);

            }

            catch (Exception ex)
            {

                writeLog(ex.Message);


            }
        }


        private static void ProcesarListaStock(string subject, List<Stock> Lowstocks)
        {
            List<AlarmaConfiguracionDetalle> alarmasConfiguracionDetalles = db.AlarmaConfiguracionDetalle.Where(x => x.AlarmaConfiguracion.TipoDeAlarmaID == TipoDeAlarma.IdControlStock).ToList();

            foreach (var item in alarmasConfiguracionDetalles)
            {
                List<Stock> stockAInformar = Lowstocks.Where(x => ((item.AlarmaConfiguracion.LocacionID.HasValue && item.AlarmaConfiguracion.LocacionID == x.ArticuloAsignacion.LocacionID) 
                                || item.AlarmaConfiguracion.OperadorID == x.ArticuloAsignacion.Locacion.OperadorID)).ToList();

                var mailsDestinatarios = db.Users.Where(x => item.UsuarioID == x.UsuarioID).Select(x => x.Email).FirstOrDefault();

                string body = string.Empty;

                foreach (var stock in stockAInformar)
                {
                    body += string.Format("El artículo \"{0}\" (Máquina \"{1}\", Nro Serie \"{2}\"- Locación \"{3}\") se encuentra {4} de stock. <br />",
                    stock.ArticuloAsignacion.Articulo.Nombre,
                    stock.ArticuloAsignacion.Maquina.NombreAlias != null ? stock.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + " - " 
                        + stock.ArticuloAsignacion.Maquina.NumeroSerie + '(' + stock.ArticuloAsignacion.Maquina.NombreAlias + ')' 
                        : stock.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + '-' + stock.ArticuloAsignacion.Maquina.NumeroSerie,
                    stock.ArticuloAsignacion.Maquina.NumeroSerie,
                    stock.ArticuloAsignacion.Maquina.LocacionID.HasValue ? stock.ArticuloAsignacion.Maquina.Locacion.Nombre : "No Disponible",
                    subject.Contains("Muy")?"muy bajo" : "bajo");

                    stock.FechaAviso = DateTime.Now;
                }

                try
                {
                    if (stockAInformar.Count() > 0)
                    {
                        SendMail(subject, body, mailsDestinatarios);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
        }

        private static void ProcesarListaMaquina(string subject, List<Maquina> maquinas)
        {
            List<AlarmaConfiguracionDetalle> alarmasConfiguracionDetalles = db.AlarmaConfiguracionDetalle.Where(x => x.AlarmaConfiguracion.TipoDeAlarmaID == TipoDeAlarma.IdControlEstadoMaquina).ToList();

            foreach (var item in alarmasConfiguracionDetalles)
            {
                List<Maquina> maquinasAInformar = maquinas.Where(x => ((item.AlarmaConfiguracion.LocacionID.HasValue && item.AlarmaConfiguracion.LocacionID == x.LocacionID) ||
                               item.AlarmaConfiguracion.OperadorID == x.OperadorID)).ToList();

                var mailsDestinatarios = db.Users.Where(x => item.UsuarioID == x.UsuarioID).Select(x => x.Email).First();

                string body = string.Empty;

                foreach (var maquina in maquinasAInformar)
                {
                    body += string.Format("La Máquina  ({0} - Locación {1} ) se encuentra {2}. <br /> ",
                        maquina.NombreAlias != null ? maquina.MarcaModelo.MarcaModeloNombre + " - " + maquina.NumeroSerie + '(' + maquina.NombreAlias + ')'
                        : maquina.MarcaModelo.MarcaModeloNombre + '-' + maquina.NumeroSerie
                        , maquina.Locacion.Nombre
                        , maquina.EstadoConexion);

                    maquina.FechaAviso = DateTime.Now;
                }

                try
                {
                    if (maquinasAInformar.Count() > 0)
                    {
                        SendMail(subject, body, mailsDestinatarios);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    
                }
            }
        }

        static public void SendMail(string subject, string body, string mailsDestinatario)
        {

                MailMessage mail = new MailMessage("noresponder@bugssa.com.ar", mailsDestinatario);
                SmtpClient client = new SmtpClient();

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                client.Send(mail);
        }


        static public void writeLog(string massage)
        {
            if (true)
            {
                StreamWriter log;

                //Uso de rutas absolutas para los archivos de log en lugar de rutas relativas
                string logDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logFilePath = Path.Combine(logDirectory, "StockNotifier.log");


                if (!File.Exists(logFilePath))
                {
                    log = new StreamWriter(logFilePath);
                }
                else
                {
                    log = File.AppendText(logFilePath);
                }


                log.WriteLine(DateTime.Now + " - INFO - " + massage);
                log.WriteLine();

                log.Close();
            }
        }

    }
}
