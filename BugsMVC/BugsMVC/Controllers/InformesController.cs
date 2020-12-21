using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugsMVC.Models;
using BugsMVC.DAL;
using System.Data.SqlClient;
using System.Data;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using BugsMVC.Handlers;
using System.Globalization;
using BugsMVC.Security;
using BugsMVC.Models.Results;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class InformesController : BaseController
    {
        private BugsContext db = new BugsContext();

        public ActionResult CertificadoEntregaEpp()
        {
            ViewBag.Locaciones = CargarMultiSelectListLocaciones();
            ViewBag.Usuarios = new SelectList(string.Empty, "UsuarioID", "Nombre");

            return View();
        }

        [Audit]
        [HttpPost]
        public ActionResult CertificadoEntregaEpp([Bind(Include = "FechaDesde,FechaHasta,Usuarios,Locaciones")] ReporteCertificadoEntregaEpp reporte)
        {
            var operatorID = GetUserOperadorID();

            if (reporte.FechaHasta > DateTime.Now)
            {
                ModelState.AddModelError("FechaHasta", "La fecha Hasta no puede ser mayor a la actual.");
            }

            if (ModelState.IsValid)
            {
                DateTime fechaHasta = reporte.FechaHasta.AddDays(1);
                DateTime fechaConsulta = DateTime.Now;
                string usuariosId = string.Join("','", reporte.Usuarios);
                string[] todasLocaciones = db.Locaciones.Where(x => operatorID == Guid.Empty || x.OperadorID == operatorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID.ToString()).ToArray();
                string[] locacionesSeleccionadas = reporte.Locaciones.Contains("Todos") ? todasLocaciones : reporte.Locaciones;

                string locacionesId = string.Join("','", locacionesSeleccionadas);

                reporte.ReportePorTrabajador = GetReportePorTrabajadorData(usuariosId, locacionesId, reporte.FechaDesde, fechaHasta)
                                                            .GroupBy(x => new
                                                            {
                                                                Locacion = (x.LocacionID != null) ? x.NombreLocacion : "Locación NO Registrada.",
                                                                x.CUIT,
                                                                x.Direccion,
                                                                x.Localidad,
                                                                x.CodigoPostal,
                                                                x.Provincia,
                                                                x.NombreUsuario,
                                                                x.Apellido,
                                                                x.Dni,
                                                                x.Numero
                                                            })
                                        .Select(x => new ReporteCertificadoEntregaEppTrabajador()
                                        {
                                            RazonSocial = x.Key.Locacion,
                                            CUIT = x.Key.CUIT,
                                            Direccion = x.Key.Direccion,
                                            Localidad = x.Key.Localidad,
                                            CodigoPostal = x.Key.CodigoPostal,
                                            Provincia = x.Key.Provincia,
                                            NombreCompleto = String.IsNullOrEmpty(x.Key.NombreUsuario) ? String.IsNullOrEmpty(x.Key.Apellido) ? "CONSUMIDOR " + x.Key.Numero : x.Key.Apellido
                                                            : String.IsNullOrEmpty(x.Key.Apellido) ? x.Key.NombreUsuario : x.Key.NombreUsuario + ", " + x.Key.Apellido,
                                            NumeroTrabajador = x.Key.Numero.Value,
                                            DNI = x.Key.Dni.ToString(),
                                            Periodo = " ",
                                            FechaDesde = reporte.FechaDesde,
                                            FechaHasta = reporte.FechaHasta,
                                            FechaConsulta = fechaConsulta,
                                            Detalle = x.OrderBy(o => o.FechaTransaccion)
                                                        .GroupBy(z => new
                                                        {
                                                            Producto = string.IsNullOrEmpty(z.NombreArticulo) ? "ARTÍCULO " + z.ValorVenta : z.NombreArticulo,
                                                            TipoModelo = string.IsNullOrEmpty(z.TipoModelo) ? "N/D" : z.TipoModelo,
                                                            Marca = string.IsNullOrEmpty(z.TipoModelo) ? "N/D" : z.Marca,
                                                            PoseeCertificacion = z.PoseeCertificacion,
                                                            Dia = z.FechaTransaccion.Day,
                                                            Mes = z.FechaTransaccion.Month,
                                                            Anio = z.FechaTransaccion.Year,
                                                            Cantidad = z.Cantidad,
                                                            ValorVenta = z.ValorVenta
                                                        })
                                                        .Select(y => new ReporteCertificadoEntregaEppDetalle()
                                                        {
                                                            Producto = y.Key.Producto,
                                                            TipoModelo = y.Key.TipoModelo,
                                                            Marca = y.Key.Marca,
                                                            PoseeCertificacion = y.Key.PoseeCertificacion,
                                                            Cantidad = y.Key.Cantidad.ToString(),
                                                            PrecioEnMaquina = y.Key.ValorVenta,
                                                            Total = y.Key.ValorVenta * y.Key.Cantidad,
                                                            FechaEntrega = y.Key.Dia.ToString() + "/" + y.Key.Mes.ToString() + "/" + y.Key.Anio.ToString(),
                                                            Dia = y.Key.Dia,
                                                            Mes = y.Key.Mes,
                                                            Año = y.Key.Anio
                                                        }).OrderBy(d => d.Año).ThenBy(d => d.Mes).ThenBy(d => d.Dia)
                                        }).ToList();

            }

            if (reporte.Locaciones == null)
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Usuarios = new MultiSelectList(string.Empty, "Value", "Text");
            }
            else
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Usuarios = CargarMultiSelectListUsuarios(reporte.Locaciones, reporte.Usuarios);
            }

            return View(reporte);
        }

        public ActionResult Consumo()
        {
            ViewBag.Locaciones = CargarMultiSelectListLocaciones();
            ViewBag.Usuarios = new SelectList(string.Empty, "UsuarioID", "Nombre");

            return View();
        }

        [Audit]
        [HttpPost]
        public ActionResult Consumo([Bind(Include = "FechaDesde,FechaHasta,Usuarios,Locaciones")] ReporteCertificadoEntregaEpp reporte)
        {
            var operatorID = GetUserOperadorID();
            
            if (reporte.FechaHasta > DateTime.Now)
            {
                ModelState.AddModelError("FechaHasta", "La fecha Hasta no puede ser mayor a la actual.");
            }

            if (ModelState.IsValid)
            {
                DateTime fechaHasta = reporte.FechaHasta.AddDays(1);
                DateTime fechaConsulta = DateTime.Now;
                string usuariosId = string.Join("','", reporte.Usuarios);
                string[] todasLocaciones = db.Locaciones.Where(x => operatorID == Guid.Empty || x.OperadorID == operatorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID.ToString()).ToArray();
                string[] locacionesSeleccionadas = reporte.Locaciones.Contains("Todos") ? todasLocaciones : reporte.Locaciones;
                string locacionesId = string.Join("','", locacionesSeleccionadas);

                reporte.ReportePorTrabajador = GetReportePorTrabajadorData(usuariosId, locacionesId , reporte.FechaDesde, fechaHasta)
                                                            .GroupBy(x => new
                                                            {
                                                                Locacion = (x.LocacionID != null) ? x.NombreLocacion : "Locación NO Registrada.",
                                                                x.CUIT,
                                                                x.Direccion,
                                                                x.Localidad,
                                                                x.CodigoPostal,
                                                                x.Provincia,
                                                                x.NombreUsuario,
                                                                x.Apellido,
                                                                x.Dni,
                                                                x.Numero
                                                            })
                                        .Select(x => new ReporteCertificadoEntregaEppTrabajador()
                                        {
                                            RazonSocial = x.Key.Locacion,
                                            CUIT = x.Key.CUIT,
                                            Direccion = x.Key.Direccion,
                                            Localidad = x.Key.Localidad,
                                            CodigoPostal = x.Key.CodigoPostal,
                                            Provincia = x.Key.Provincia,
                                            NombreCompleto = String.IsNullOrEmpty(x.Key.NombreUsuario) ? String.IsNullOrEmpty(x.Key.Apellido) ? "CONSUMIDOR " + x.Key.Numero : x.Key.Apellido
                                                            : String.IsNullOrEmpty(x.Key.Apellido) ? x.Key.NombreUsuario : x.Key.NombreUsuario + ", " + x.Key.Apellido,
                                            //NombreCompleto = String.IsNullOrEmpty(x.Key.NombreUsuario) ? String.IsNullOrEmpty(x.Key.Apellido) ? "" : x.Key.Apellido
                                            //                : String.IsNullOrEmpty(x.Key.Apellido) ? x.Key.NombreUsuario : x.Key.NombreUsuario + ", " + x.Key.Apellido,
                                            NumeroTrabajador = x.Key.Numero.Value,
                                            DNI = x.Key.Dni.ToString(),
                                            Periodo = " ",
                                            FechaDesde = reporte.FechaDesde,
                                            FechaHasta = reporte.FechaHasta,
                                            FechaConsulta = fechaConsulta,
                                            Detalle = x.OrderBy(o => o.FechaTransaccion)
                                                        .GroupBy(z => new
                                                        {
                                                            Producto = string.IsNullOrEmpty(z.NombreArticulo) ? "ARTÍCULO " + z.ValorVenta : z.NombreArticulo,
                                                            TipoModelo = string.IsNullOrEmpty(z.TipoModelo) ? "N/D" : z.TipoModelo,
                                                            Marca = string.IsNullOrEmpty(z.TipoModelo) ? "N/D" : z.Marca,
                                                            PoseeCertificacion = " ",
                                                            Dia = z.FechaTransaccion.Day,
                                                            Mes = z.FechaTransaccion.Month,
                                                            Anio = z.FechaTransaccion.Year,
                                                            Cantidad = z.Cantidad,
                                                            ValorVenta = z.ValorVenta
                                                        })
                                                        .Select(y => new ReporteCertificadoEntregaEppDetalle()
                                                        {
                                                            Producto = y.Key.Producto,
                                                            TipoModelo = y.Key.TipoModelo,
                                                            Marca = y.Key.Marca,
                                                            PoseeCertificacion = y.Key.PoseeCertificacion,
                                                            Cantidad = y.Key.Cantidad.ToString(),
                                                            PrecioEnMaquina = y.Key.ValorVenta,
                                                            Total = y.Key.ValorVenta * y.Key.Cantidad,
                                                            FechaEntrega = y.Key.Dia.ToString() + "/" + y.Key.Mes.ToString() + "/" + y.Key.Anio.ToString(),
                                                            Dia = y.Key.Dia,
                                                            Mes = y.Key.Mes,
                                                            Año = y.Key.Anio
                                                        }).OrderBy(d => d.Año).ThenBy(d => d.Mes).ThenBy(d => d.Dia)
                                        }).ToList();
               
            }

            if (reporte.Locaciones == null)
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Usuarios = new MultiSelectList(string.Empty, "Value", "Text");
            }
            else
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Usuarios = CargarMultiSelectListUsuarios(reporte.Locaciones, reporte.Usuarios);
            }

            return View(reporte);
        }

        public ActionResult EntregaEPPTrabajador()
        {
            ViewBag.Locaciones = CargarMultiSelectListLocaciones();
            ViewBag.Trabajadores = new MultiSelectList(string.Empty, "Value", "Text");

            return View();
        }

        [Audit]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EntregaEPPTrabajador([Bind(Include = "FechaDesde,FechaHasta,Locaciones,Trabajadores")] ReporteEntregaEPPTrabajador reporte)
        {
            Guid operatorID = GetUserOperadorID();
            
            if (reporte.FechaHasta > DateTime.Now)
            {
                ModelState.AddModelError("FechaHasta", "La fecha Hasta no puede ser mayor a la actual.");
            }

            if (ModelState.IsValid)
            {
                List<Guid> locaciones =
                    reporte.Locaciones.Contains("Todos") ? db.Locaciones.Where(x => operatorID == Guid.Empty || x.OperadorID == operatorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID).ToList()
                    : reporte.Locaciones.Select(x => new Guid(x)).ToList();
                var fechaHasta = reporte.FechaHasta.AddDays(1);
                var fechaConsulta = DateTime.Now;
                bool ObtenerTodosTrabajadores = false;
                bool ObtenerTrabajadoresSinAlta = false;

                if (reporte.Trabajadores.Contains("Todos"))
                {
                    ObtenerTodosTrabajadores = true;
                    var list = new List<string>(reporte.Trabajadores);
                    list.Remove("Todos");
                    reporte.Trabajadores = list.ToArray();
                }
                else if (reporte.Trabajadores.Contains("SinAlta"))
                {
                    ObtenerTrabajadoresSinAlta = true;
                    var list = new List<string>(reporte.Trabajadores);
                    list.Remove("SinAlta");
                    reporte.Trabajadores = list.ToArray();
                }
                List<Guid> trabajadores = reporte.Trabajadores.Select(x => new Guid(x)).ToList();

                var query = db.Transacciones.Join(db.Usuarios, tran => tran.UsuarioID, usr => usr.UsuarioID, (tran, usr) => new { tran, usr })
                                            .Join(db.Locaciones, tranUsr => tranUsr.tran.LocacionID, loc => loc.LocacionID, (tranUsr, loc) => new { tranUsr, loc })
                                            .Join(db.Articulos, tranUsrLoc => tranUsrLoc.tranUsr.tran.ArticuloID, art => art.ArticuloID, (tranUsrLoc, art) => new { tranUsrLoc, art })
                                        .Where(x => x.tranUsrLoc.tranUsr.tran.FechaTransaccion.HasValue && x.tranUsrLoc.tranUsr.tran.FechaTransaccion >= reporte.FechaDesde &&
                                                    x.tranUsrLoc.tranUsr.tran.FechaTransaccion <= fechaHasta &&
                                                    (ObtenerTodosTrabajadores || trabajadores.Contains(x.tranUsrLoc.tranUsr.usr.UsuarioID)) &&
                                                    (operatorID == Guid.Empty || x.tranUsrLoc.tranUsr.tran.OperadorID == operatorID) &&
                                                    (locaciones.Contains(x.tranUsrLoc.loc.LocacionID)))
                                        .GroupBy(x => new
                                        {
                                            Locacion = x.tranUsrLoc.loc.Nombre,
                                            x.tranUsrLoc.loc.CUIT,
                                            x.tranUsrLoc.loc.Direccion,
                                            x.tranUsrLoc.loc.Localidad,
                                            x.tranUsrLoc.loc.CodigoPostal,
                                            x.tranUsrLoc.loc.Provincia
                                        })
                                        .Select(x => new ReporteEntregaEPPTrabajadorLocacion()
                                        {
                                            RazonSocial = x.Key.Locacion,
                                            CUIT = x.Key.CUIT,
                                            Direccion = x.Key.Direccion,
                                            Localidad = x.Key.Localidad,
                                            CodigoPostal = x.Key.CodigoPostal,
                                            Provincia = x.Key.Provincia,
                                            Periodo = " ",
                                            FechaDesde = reporte.FechaDesde,
                                            FechaHasta = reporte.FechaHasta,
                                            FechaConsulta = fechaConsulta,
                                            Detalle = x.GroupBy(z => new
                                            {
                                                Nombre = z.tranUsrLoc.tranUsr.usr.Nombre,
                                                Apellido = z.tranUsrLoc.tranUsr.usr.Apellido,
                                                Legajo = z.tranUsrLoc.tranUsr.usr.Legajo,
                                                Numero = z.tranUsrLoc.tranUsr.usr.Numero
                                            })
                                                        .Select(y => new ReporteEntregaEPPTrabajadorDetalle()
                                                        {
                                                            //Nombre = y.Key.Nombre + " " + y.Key.Apellido,
                                                            Nombre = String.IsNullOrEmpty(y.Key.Nombre) ? String.IsNullOrEmpty(y.Key.Apellido) ? "CONSUMIDOR " + y.Key.Numero : y.Key.Apellido + " - " + y.Key.Numero
                                                            : String.IsNullOrEmpty(y.Key.Apellido) ? y.Key.Nombre + " - " + y.Key.Numero : y.Key.Nombre + ", " + y.Key.Apellido + " - " + y.Key.Numero,
                                                            Legajo = y.Key.Legajo,
                                                            Cantidades = y.GroupBy(g => new
                                                            {
                                                                Nombre = g.art.Nombre
                                                            }).OrderBy(e => e.Key.Nombre)
                                                            .Select(c => new ArticuloCantidad()
                                                            {
                                                                Nombre = c.Key.Nombre,
                                                                Cantidad = c.Count(),
                                                                ValorVenta = 0
                                                            })
                                                        })
                                        });

                List<ReporteEntregaEPPTrabajadorLocacion> query2 = new List<ReporteEntregaEPPTrabajadorLocacion>();

                if (ObtenerTrabajadoresSinAlta || ObtenerTodosTrabajadores)
                {
                    query2 = db.Transacciones.Join(db.Usuarios, tran => tran.UsuarioID, usr => usr.UsuarioID, (tran, usr) => new { tran, usr })
                                     .Join(db.Locaciones, tranUsr => tranUsr.tran.LocacionID, loc => loc.LocacionID, (tranUsr, loc) => new { tranUsr, loc })
                                 .Where(x => x.tranUsr.tran.FechaTransaccion.HasValue && x.tranUsr.tran.FechaTransaccion >= reporte.FechaDesde &&
                                             x.tranUsr.tran.FechaTransaccion <= fechaHasta &&
                                             (ObtenerTodosTrabajadores || ObtenerTrabajadoresSinAlta || trabajadores.Contains(x.tranUsr.usr.UsuarioID)) &&
                                             (operatorID == Guid.Empty || x.tranUsr.tran.OperadorID == operatorID) &&
                                             (locaciones.Contains(x.loc.LocacionID)) &&
                                             (x.tranUsr.tran.ArticuloID == null))
                                 .GroupBy(x => new
                                 {
                                     Locacion = x.loc.Nombre,
                                     x.loc.CUIT,
                                     x.loc.Direccion,
                                     x.loc.Localidad,
                                     x.loc.CodigoPostal,
                                     x.loc.Provincia
                                 })
                                 .Select(x => new ReporteEntregaEPPTrabajadorLocacion()
                                 {
                                     RazonSocial = x.Key.Locacion,
                                     CUIT = x.Key.CUIT,
                                     Direccion = x.Key.Direccion,
                                     Localidad = x.Key.Localidad,
                                     CodigoPostal = x.Key.CodigoPostal,
                                     Provincia = x.Key.Provincia,
                                     Periodo = " ",
                                     FechaDesde = reporte.FechaDesde,
                                     FechaHasta = reporte.FechaHasta,
                                     FechaConsulta = fechaConsulta,
                                     Detalle = x.GroupBy(z => new
                                     {
                                         Nombre = z.tranUsr.usr.Nombre,
                                         Apellido = z.tranUsr.usr.Apellido,
                                         Legajo = z.tranUsr.usr.Legajo,
                                         Numero = z.tranUsr.usr.Numero
                                     })
                                                 .Select(y => new ReporteEntregaEPPTrabajadorDetalle()
                                                 {
                                                     //Nombre = (y.Key.Nombre != null) ? y.Key.Nombre + " " + y.Key.Apellido : "Sin Alta " + y.Key.Numero,
                                                     Nombre = String.IsNullOrEmpty(y.Key.Nombre) ? String.IsNullOrEmpty(y.Key.Apellido) ? "CONSUMIDOR " + y.Key.Numero : y.Key.Apellido + " - " + y.Key.Numero
                                                            : String.IsNullOrEmpty(y.Key.Apellido) ? y.Key.Nombre + " - " + y.Key.Numero : y.Key.Nombre + ", " + y.Key.Apellido + " - " + y.Key.Numero,
                                                     Legajo = y.Key.Legajo,
                                                     Cantidades = y.GroupBy(g => new
                                                     {
                                                         Nombre = "ARTÍCULO " + g.tranUsr.tran.ValorVenta,
                                                         ValorVenta = g.tranUsr.tran.ValorVenta
                                                     }).OrderBy(e => e.Key.ValorVenta)
                                                     .Select(c => new ArticuloCantidad()
                                                     {
                                                         Nombre = c.Key.Nombre,
                                                         Cantidad = c.Count(),
                                                         ValorVenta = c.Key.ValorVenta
                                                     })
                                                 })
                                 }).ToList();
                }
           

                reporte.ReportePorTrabajador = query2 != null ? query.ToList().Union(query2.ToList()).ToList() : query.ToList();
            }

            if (reporte.Locaciones == null)
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Trabajadores = new MultiSelectList(string.Empty, "Value", "Text");
            }
            else
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Trabajadores = CargarMultiSelectListUsuarios(reporte.Locaciones, reporte.Trabajadores);
            }
            return View(reporte);
        }            

        public ActionResult EntregaTotalEPP()
        {
            ViewBag.Locaciones = CargarMultiSelectListLocaciones();
            ViewBag.Articulos = new MultiSelectList(string.Empty, "Value", "Text");
            return View();
        }

        [Audit]
        [HttpPost]
        public ActionResult EntregaTotalEPP([Bind(Include = "FechaDesde,FechaHasta,Locaciones,Articulos")] ReporteEntregaTotalEPP reporte)
        {
            Guid operatorID = GetUserOperadorID();

            if (reporte.FechaHasta > DateTime.Now)
            {
                ModelState.AddModelError("FechaHasta", "La fecha Hasta no puede ser mayor a la actual.");
            }

            if (ModelState.IsValid)
            {
                DateTime fechaHasta = reporte.FechaHasta.AddDays(1);
                DateTime fechaConsulta = DateTime.Now;

                string articulosId, valoresVenta;
                separarArticuloValorVenta(reporte.Articulos, out articulosId, out valoresVenta);

                string[] todasLocaciones = db.Locaciones.Where(x => operatorID == Guid.Empty || x.OperadorID == operatorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID.ToString()).ToArray();
                string[] locacionesSeleccionadas = reporte.Locaciones.Contains("Todos") ? todasLocaciones : reporte.Locaciones;
                string locacionesId = string.Join("','", locacionesSeleccionadas);

                reporte.ReportePorLocacion = GetReportePorLocacionData(operatorID, articulosId, locacionesId, valoresVenta, reporte.FechaDesde, fechaHasta)
                                                 .GroupBy(x => new
                                                 {
                                                     Locacion = (x.LocacionID != null) ? x.NombreLocacion : "Locación NO Registrada.",
                                                     x.CUIT,
                                                     x.Direccion,
                                                     x.Localidad,
                                                     x.CodigoPostal,
                                                     x.Provincia
                                                 })
                                                .Select(x => new ReporteEntregaTotalEPPLocacion()
                                                {
                                                    RazonSocial = x.Key.Locacion,
                                                    CUIT = x.Key.CUIT,
                                                    Direccion = x.Key.Direccion,
                                                    Localidad = x.Key.Localidad,
                                                    CodigoPostal = x.Key.CodigoPostal,
                                                    Provincia = x.Key.Provincia,
                                                    Periodo = string.Empty,
                                                    FechaDesde = reporte.FechaDesde,
                                                    FechaHasta = reporte.FechaHasta,
                                                    FechaConsulta = fechaConsulta,
                                                    Detalle = x.GroupBy(z => new
                                                    {
                                                        Articulo = string.IsNullOrEmpty(z.NombreArticulo) ? "ARTÍCULO " + z.ValorVenta : z.NombreArticulo,
                                                        Cantidad = z.Cantidad,
                                                        ValorVenta = z.ValorVenta,
                                                        CostoReal = z.CostoReal
                                                    }).Select(y => new ReporteEntregaTotalEPPDetalle()
                                                    {
                                                        Articulo = y.Key.Articulo,
                                                        CantidadTotal = y.Key.Cantidad.ToString(),
                                                        PrecioEnMaquina = y.Key.ValorVenta,
                                                        PrecioUnitarioReal = y.Key.CostoReal.HasValue ? y.Key.CostoReal.Value : 0,
                                                        Total = Convert.ToDecimal(y.Key.Cantidad) * (y.Key.CostoReal.HasValue ? y.Key.CostoReal.Value : 0),
                                                        Observaciones = string.Empty
                                                    }).OrderBy(e => e.Articulo)
                                                }).ToList();
            }

            if (reporte.Locaciones == null)
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Articulos = new MultiSelectList(string.Empty, "Value", "Text");
            }
            else
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Articulos = CargarMultiSelectListArticulos(reporte.Locaciones, reporte.Articulos);
            }

            return View(reporte);
        }

        private static void separarArticuloValorVenta(string[] ArticulosValorVenta, out string articulosId, out string valoresVenta)
        {
            articulosId = string.Empty;
            valoresVenta = string.Empty;
            foreach (var item in ArticulosValorVenta)
            {
                var arraySplit = item.Split(';');
                if (articulosId != string.Empty || valoresVenta != string.Empty)
                {
                    articulosId += ',';
                    valoresVenta += ',';
                }
                articulosId += arraySplit[0] == string.Empty ? null : arraySplit[0];
                valoresVenta += arraySplit[1] == string.Empty ? null : arraySplit[1];
            }
        }

        public ActionResult Ventas()
        {
            ViewBag.Locaciones = CargarMultiSelectListLocaciones();
            ViewBag.Articulos = new MultiSelectList(string.Empty, "Value", "Text");

            return View();
        }

        [Audit]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Ventas([Bind(Include = "FechaDesde,FechaHasta,Locaciones,Articulos")] ReporteEntregaTotalEPP reporte)
        {
            Guid operatorID = GetUserOperadorID();

            //Ver si
            if (reporte.FechaHasta > DateTime.Now)
            {
                ModelState.AddModelError("FechaHasta", "La fecha Hasta no puede ser mayor a la actual.");
            }

            if (ModelState.IsValid)
            {
                DateTime fechaHasta = reporte.FechaHasta.AddDays(1);
                DateTime fechaConsulta = DateTime.Now;

                string articulosId, valoresVenta;
                separarArticuloValorVenta(reporte.Articulos, out articulosId, out valoresVenta);

                string[] todasLocaciones = db.Locaciones.Where(x => operatorID == Guid.Empty || x.OperadorID == operatorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID.ToString()).ToArray();
                string[] locacionesSeleccionadas = reporte.Locaciones.Contains("Todos") ? todasLocaciones : reporte.Locaciones;

                string locacionesId = string.Join("','", locacionesSeleccionadas);

                reporte.ReportePorLocacion = GetReportePorLocacionData(operatorID, articulosId, locacionesId, valoresVenta, reporte.FechaDesde, fechaHasta)
                                                .GroupBy(x => new
                                                {
                                                    Locacion = (x.LocacionID != null) ? x.NombreLocacion : "Locación NO Registrada.",
                                                    x.CUIT,
                                                    x.Direccion,
                                                    x.Localidad,
                                                    x.CodigoPostal,
                                                    x.Provincia,
                                                })
                                               .Select(x => new ReporteEntregaTotalEPPLocacion()
                                               {
                                                   RazonSocial = x.Key.Locacion,
                                                   CUIT = x.Key.CUIT,
                                                   Direccion = x.Key.Direccion,
                                                   Localidad = x.Key.Localidad,
                                                   CodigoPostal = x.Key.CodigoPostal,
                                                   Provincia = x.Key.Provincia,
                                                   Periodo = string.Empty,
                                                   FechaDesde = reporte.FechaDesde,
                                                   FechaHasta = reporte.FechaHasta,
                                                   FechaConsulta = fechaConsulta,
                                                   Detalle = x.GroupBy(z => new
                                                   {
                                                       Articulo = string.IsNullOrEmpty(z.NombreArticulo) ? "ARTÍCULO " + z.ValorVenta : z.NombreArticulo,
                                                       Cantidad = z.Cantidad,
                                                       ValorVenta = z.ValorVenta,
                                                       CostoReal = z.CostoReal
                                                   }).Select(y => new ReporteEntregaTotalEPPDetalle()
                                                               {
                                                                   Articulo = y.Key.Articulo,
                                                                   CantidadTotal = y.Key.Cantidad.ToString(),
                                                                   PrecioEnMaquina = y.Key.ValorVenta,
                                                                   PrecioUnitarioReal = y.Key.CostoReal.HasValue ? y.Key.CostoReal.Value : 0,
                                                                   Total = Convert.ToDecimal(y.Key.Cantidad) * y.Key.ValorVenta,
                                                                   Observaciones = string.Empty
                                                               }).OrderBy(e => e.Articulo)
                                               }).ToList();
            }

            if (reporte.Locaciones == null)
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Articulos = new MultiSelectList(string.Empty, "Value", "Text");
            }
            else
            {
                ViewBag.Locaciones = CargarMultiSelectListLocaciones();
                ViewBag.Articulos = CargarMultiSelectListArticulos(reporte.Locaciones, reporte.Articulos);
            }

            return View(reporte);
        }

        private MultiSelectList CargarMultiSelectListLocaciones()
        {
            Guid operadorID = GetUserOperadorID();

            List<SelectListItem> locaciones = new List<SelectListItem>();

            IQueryable<Locacion> locacionByOperador = db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID);

            if (locacionByOperador.Count() != 0)
            {
                locaciones.Add(new SelectListItem() { Text = "TODOS", Value = "Todos" });
                locaciones.AddRange(locacionByOperador.Select(x => new SelectListItem()
                {
                    Text = (x.Nombre != null) ? x.Nombre : string.Empty,
                    Value = x.LocacionID.ToString()
                }).OrderBy(x => x.Text));
            }

            return new MultiSelectList(locaciones, "Value", "Text");
        }

        private MultiSelectList CargarMultiSelectListArticulos(string[] locaciones, string[] articulosSeleccionados)
        {
            List<Guid?> articulosID = new List<Guid?>();
            List<SelectListItem> articulos = new List<SelectListItem>();
            articulos.Add(new SelectListItem() { Text = "TODOS", Value = "Todos;0" });
            articulos.Add(new SelectListItem() { Text = "ARTÍCULOS SIN ALTA", Value = "SinAlta;0" });

            if (locaciones.Contains("Todos"))
            {
                var operadorID = GetUserOperadorID();
                articulos.AddRange(db.Transacciones.Where(x =>operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => new { x.Articulo.Nombre, x.ValorVenta }).Select(x => new SelectListItem()
                {
                    Text = (x.ArticuloID == null || string.IsNullOrEmpty(x.Articulo.Nombre)) ? "ARTÍCULO " + x.ValorVenta : x.Articulo.Nombre + " - " + x.ValorVenta,
                    Value = x.ArticuloID.ToString() + ";" + x.ValorVenta.ToString()
                }).Distinct());

            }
            else
            {
                if (locaciones.Length == 1 && locaciones[0] == string.Empty)
                {
                    return new MultiSelectList(string.Empty, "Value", "Text");
                }
                foreach (string item in locaciones)
                {
                    articulos.AddRange(db.Transacciones.Where(x => x.LocacionID == new Guid(item)).OrderBy(x => new { x.Articulo.Nombre, x.ValorVenta }).Select(x => new SelectListItem()
                    {
                        Text = (x.ArticuloID == null || string.IsNullOrEmpty(x.Articulo.Nombre)) ? "ARTÍCULO " + x.ValorVenta : x.Articulo.Nombre + " - " + x.ValorVenta,
                        Value = x.ArticuloID.ToString() + ";" + x.ValorVenta.ToString()
                    }).Distinct());
                }
            }

            return new MultiSelectList(articulos, "Value", "Text", articulosSeleccionados.AsEnumerable());
        }

        private MultiSelectList CargarMultiSelectListUsuarios(string[] locaciones, string[] usuariosSeleccionados)
        {
            Guid operadorID = GetUserOperadorID();
            List<SelectListItem> usuarios = new List<SelectListItem>();
            List<Guid> locacionesID = new List<Guid>();

            if (locaciones != null)
            {
                if (locaciones.Contains("Todos"))
                {
                    locacionesID = db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => new { x.Nombre, x.Numero }).Select(x => x.LocacionID).ToList();
                }
                else
                {
                    locacionesID = locaciones.Select(x => new Guid(x)).ToList();
                }
            }

            var usuariosByLocaciones = db.Usuarios.Where(x => x.LocacionID.HasValue && locacionesID.Contains(x.LocacionID.Value)).OrderBy(x => new { x.Nombre, x.Numero });

            if (usuariosByLocaciones.Count() != 0)
            {
                usuarios.Add(new SelectListItem() { Text = "TODOS", Value = "Todos" });
                usuarios.Add(new SelectListItem() { Text = "CONSUMIDORES SIN ALTA", Value = "SinAlta" });
                usuarios.AddRange(usuariosByLocaciones.Select(x => new SelectListItem()
                {
                    Text = String.IsNullOrEmpty(x.Nombre) ? String.IsNullOrEmpty(x.Apellido) ? "CONSUMIDOR " + x.Numero : x.Apellido + " - " + x.Numero
                         : String.IsNullOrEmpty(x.Apellido) ? x.Nombre + " - " + x.Numero : x.Nombre + ", " + x.Apellido + " - " + x.Numero,
                    Value = x.UsuarioID.ToString()
                }));
            }


            return new MultiSelectList(usuarios, "Value", "Text", usuariosSeleccionados.AsEnumerable());
        }

        public Guid GetUserOperadorID()
        {
            string userId = User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

            Guid operadorID = Guid.Empty;
            if (User.IsInRole("SuperAdmin"))
            {
                operadorID = (!String.IsNullOrEmpty((string)HttpContext.Session["AdminOperadorID"])) ? new Guid((string)HttpContext.Session["AdminOperadorID"]) : Guid.Empty;
            }
            else
            {
                operadorID = (currentUser.Usuario != null && currentUser.Usuario.OperadorID.HasValue) ? currentUser.Usuario.OperadorID.Value : Guid.Empty;
            }

            return operadorID;
        }

        public JsonResult GetArticulosByLocacionesMultiSelectList(string[] locaciones)
        {
            List<Guid?> articulosID = new List<Guid?>();
            List<SelectListItem> articulos = new List<SelectListItem>();
            articulos.Add(new SelectListItem() { Text = "TODOS", Value = "Todos;0" });
            articulos.Add(new SelectListItem() { Text = "ARTÍCULOS SIN ALTA", Value = "SinAlta;0" });

            if (locaciones.Contains("Todos"))
            {
                var operadorID = GetUserOperadorID();
                articulos.AddRange(db.Transacciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).Select(x => new SelectListItem()
                {
                    Text = (x.ArticuloID == null || string.IsNullOrEmpty(x.Articulo.Nombre)) ? "ARTÍCULO " + x.ValorVenta : x.Articulo.Nombre + " - " + x.ValorVenta,
                    Value = x.ArticuloID.ToString()+";"+x.ValorVenta.ToString()
                }).Distinct().OrderBy(x => x.Text));
            }
            else
            {
                if (locaciones.Length == 1 && locaciones[0] == string.Empty)
                {
                    return Json(new MultiSelectList(string.Empty, "Value", "Text"), JsonRequestBehavior.AllowGet);
                }
                foreach (string item in locaciones)
                {
                    articulos.AddRange(db.Transacciones.Where(x =>x.LocacionID == new Guid(item)).Select(x => new SelectListItem()
                    {
                        Text = (x.ArticuloID == null || string.IsNullOrEmpty(x.Articulo.Nombre)) ? "ARTÍCULO " + x.ValorVenta : x.Articulo.Nombre + " - " + x.ValorVenta,
                        Value = x.ArticuloID.ToString() + ";" + x.ValorVenta.ToString()

                    }).Distinct().OrderBy(x => x.Text));
                }
            }
                        
            return Json(new MultiSelectList(articulos.ToList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocaciones()
        {
            List<Guid> locacionesID = new List<Guid>();
            var operadorID = GetUserOperadorID();

            var ret = new MultiSelectList(db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).Select(x => new SelectListItem()
            {
                Text = x.Nombre,
                Value = x.LocacionID.ToString()
            }).OrderBy(x => x.Text).ToList(), "Value", "Text");

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUsuariosByLocacionesMultiSelectList(string[] locaciones)
        {
            List<Guid> locacionesID = new List<Guid>();
            if (locaciones.Contains("Todos"))
            {
                var operadorID = GetUserOperadorID();
                locacionesID = db.Locaciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).OrderBy(x => x.Nombre).Select(x => x.LocacionID).ToList();
            }
            else
            {
                if (locaciones.Length == 1 && locaciones[0] == string.Empty)
                {
                    return Json(new MultiSelectList(string.Empty, "Value", "Text"), JsonRequestBehavior.AllowGet);
                }
                foreach (string item in locaciones)
                {
                    locacionesID.Add(new Guid(item));
                }
            }

            List<SelectListItem> usuarios = new List<SelectListItem>();
            var usuariosByLocaciones = db.Usuarios.Where(x => x.LocacionID.HasValue && locacionesID.Contains(x.LocacionID.Value)).OrderBy(x => new { x.Nombre, x.Numero });

            if (usuariosByLocaciones.Count() != 0)
            {
                usuarios.Add(new SelectListItem() { Text = "TODOS", Value = "Todos" });
                usuarios.Add(new SelectListItem() { Text = "CONSUMIDORES SIN ALTA", Value = "SinAlta" });
                usuarios.AddRange(usuariosByLocaciones.Select(x => new SelectListItem()
                {
                    Text = String.IsNullOrEmpty(x.Nombre) ? String.IsNullOrEmpty(x.Apellido) ? "CONSUMIDOR " + x.Numero : x.Apellido + " - " + x.Numero
                                : String.IsNullOrEmpty(x.Apellido) ? x.Nombre + " - " + x.Numero : x.Nombre + ", " + x.Apellido + " - " + x.Numero,
                    Value = x.UsuarioID.ToString()
                }));
            }
            
            return Json(new MultiSelectList(usuarios, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        private IQueryable<ReportePorTrabajadorResult> GetReportePorTrabajadorData(string usuariosId, string locacionesId, DateTime fechaDesde, DateTime fechaHasta)
        {
            return db.Database.SqlQuery<ReportePorTrabajadorResult>("CargaInformeTransacciones @usuariosId, @locacionesId, @fechaDesde, @fechaHasta",
                        new SqlParameter("@usuariosId", usuariosId),
                        new SqlParameter("@locacionesId", locacionesId),
                        new SqlParameter("@fechaDesde", fechaDesde),
                        new SqlParameter("@fechaHasta", fechaHasta))
                        .ToList()
                        .AsQueryable();
        }

        private IQueryable<ReportePorLocacionResult> GetReportePorLocacionData(Guid operadorId,string articulosId, string locacionesId, string valoresVenta, DateTime fechaDesde, DateTime fechaHasta)
        {
            return db.Database.SqlQuery<ReportePorLocacionResult>("CargaInformeEntregaTotalEPP @operadorId, @articulosId, @locacionesId, @valoresVenta,@fechaDesde, @fechaHasta",
                new SqlParameter("@operadorId", operadorId),
                new SqlParameter("@articulosId", articulosId),
                new SqlParameter("@locacionesId", locacionesId),
                new SqlParameter("@valoresVenta", valoresVenta),
                new SqlParameter("@fechaDesde", fechaDesde),
                new SqlParameter("@fechaHasta", fechaHasta))
                .ToList()
                .AsQueryable();             
        }
    }
}