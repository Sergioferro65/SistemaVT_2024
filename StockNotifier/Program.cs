using System;
using System.Collections.Generic;
using System.Linq;
using BugsMVC.DAL;
using System.Net.Mail;
using BugsMVC.Models;
using System.Configuration;
using mercadopago;
using System.Collections;
using System.Data.Entity;

namespace StockNotifier
{
    class Program
    {
        public static BugsContext db;
        static void Main(string[] args)
        {
            db= new BugsContext();

            var stocks = db.Stocks.ToList();
            var maquinas = db.Maquinas.ToList();
            var mercadoPagos = db.MercadoPago;
            int tiempoMP = 5;

            var mailsStockMuyBajo = new List<Stock>();
            var mailsStockBajo = new List<Stock>();
            var mailsMaquinaSinConexion = new List<Maquina>();
            var mailsMaquinaInhibidas = new List<Maquina>();

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
                db.SaveChanges();
            }

            foreach (var mercadoPago in mercadoPagos.Where(x=>x.MercadoPagoEstadoFinancieroId == (int)MercadoPagoEstadoFinanciero.States.ACREDITADO
                                                        && (x.MercadoPagoEstadoTransmisionId != (int)MercadoPagoEstadoTransmision.States.TERMINADO_OK)
                                                        ).ToList())
            {
                if ((DateTime.Now - mercadoPago.Fecha).TotalMinutes >= tiempoMP)
                {
                    //Devolver dinero
                    Maquina maquina = db.Maquinas.Where(x => x.MaquinaID == mercadoPago.MaquinaId).FirstOrDefault();
                    Operador operador = db.Operadores.Where(x => x.OperadorID == maquina.OperadorID).FirstOrDefault();

                    if(operador.ClientId != null && operador.SecretToken != null)
                    {
                        var mp = new MP(operador.ClientId, operador.SecretToken);
                        Dictionary<string, string> filters = new Dictionary<string, string> { { "id", mercadoPago.Comprobante } };
                        Hashtable searchResult = mp.searchPayment(filters);

                        int status = Convert.ToInt32(searchResult["status"]);
                        if (searchResult != null && searchResult.Count > 0 && status == 200)
                        {
                            var responseValues = (ArrayList)((Hashtable)searchResult["response"])["results"];
                            Hashtable firstItem = (Hashtable)responseValues[0];
                            Hashtable resultCollection = (Hashtable)firstItem["collection"];

                            if (resultCollection["status"].ToString() == "refunded")
                            {
                                //El pago ya se devolvio, seguramente desde la pagina de mercadoPago
                                mercadoPago.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;

                                db.Entry(mercadoPago).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                Hashtable result = mp.refundPayment(mercadoPago.Comprobante);
                                if (result["status"].ToString() == "200")
                                {
                                    mercadoPago.MercadoPagoEstadoFinancieroId = (int)MercadoPagoEstadoFinanciero.States.DEVUELTO;

                                    db.Entry(mercadoPago).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            //No se encontró el pago,  no deberia suceder.
                            //El clientID o secret son incorrectos
                        }
                        //MercadoPago entity = db.MercadoPago.Where(x => x.Comprobante == Comprobante).First();
                    }
                    else
                    {
                        //Aca deberia registrar un estado correspondiente cuando el operador no tiene cargado clientID o secretToken.
                    }
                }
            }

            ProcesarListaStock("SistemaVT - Alarma Stock Muy Bajo", mailsStockMuyBajo);
            ProcesarListaStock("SistemaVT - Alarma Stock Bajo", mailsStockBajo);

            ProcesarListaMaquina("SistemaVT - Alarma Máquina Sin Conexión", mailsMaquinaSinConexion);
            ProcesarListaMaquina("SistemaVT - Alarma Máquina Inactiva", mailsMaquinaInhibidas);
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
    }
}
