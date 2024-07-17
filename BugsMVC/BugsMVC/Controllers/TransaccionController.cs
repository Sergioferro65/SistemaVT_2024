using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugsMVC.DAL;
using BugsMVC.Models;
using Microsoft.AspNet.Identity;
using System.Web.UI.WebControls;
using System.IO;
using BugsMVC.Helpers;
using Newtonsoft.Json;
using System.Linq.Dynamic;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using BugsMVC.Security;
using BugsMVC.Handlers;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Json;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using OfficeOpenXml;

namespace BugsMVC.Controllers
{
    [AuthorizeRoles]
    public class TransaccionController : BaseController
    {
        private BugsContext db = new BugsContext();
        // GET: Transaccion
        public ActionResult Index(bool clsSession = true)
        {
            if (clsSession)
            {
                TempData.Clear();
                 Session[JQGridFilterRule.Keys.Transaccion.FILTERS] = null;
                 Session[JQGridFilterRule.Keys.Transaccion.SIDX]= null;
            }

            return View();
        }

        public ActionResult IndexMal(bool clsSession = true)
        {
            if (clsSession)
            {
                TempData.Clear();
                Session["filtersTMal"] = null;
                Session["sortTMal"] = null;
            }

            return View("IndexMal");
        }

        public ActionResult IndexGrupoMal(bool clsSession = true)
        {
            if (clsSession)
            {
                TempData.Clear();
                Session["filtersGrupoTMal"] = null;
                Session["sortGrupoTMal"] = null;
            }

            return View("IndexGrupoMal");
        }

        public JsonResult GetAllTransaccionesMal(string sidx, string sord, int page, int rows, string filters)
        {
            Guid operadorID = GetUserOperadorID();
            var sessionFilters = Session["filtersTMal"] as List<Filter>;
            var sessionSort = Session["sortTMal"] as Dictionary<string, string>;
            bool hasFilter = false;
            bool hasSession = false;
            bool hasSortSession = false;

            //var tempListFilters = Session["stringFilters"] as Dictionary<string, string>;

            if (sessionFilters != null && sessionFilters.Any())
                hasSession = true;

            if (filters != null)
                hasFilter = true;

            if (sessionSort != null && sessionSort.Any())
                hasSortSession = true;

            List<Filter> parsedFilters = null;

            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;

            var transacciones = db.TransaccionesMal.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID)
            .Select(x => new
            {
                Terminal = (x.Terminal != null) ? x.Terminal.NumeroSerie.ToString() : "Terminal NO Registrada.",
                Operador = (x.Operador != null) ? x.Operador.Nombre.ToString() : "Operador NO Registrado.",
                Maquina = (x.Maquina != null) ? x.Maquina.NumeroSerie.ToString() : "Máquina NO Registrada.",
                Locacion = (x.Locacion != null) ? x.Locacion.Nombre.ToString() : "Locación NO Registrada.",
                x.Transaccion,
                IdTransaccionMal = x.IdTransaccionMal.ToString(),
                x.Motivo,
                x.FechaDescarga,
            });

            if (hasFilter || hasSession)
            {

                if (hasFilter)
                {//Unicamente filters viene vacio en la primer carga.

                    JObject data = JsonConvert.DeserializeObject<JObject>(filters);
                    var itemFilters = data.Last.Values().Select(x => x.ToObject<Filter>()).ToList();
                    parsedFilters = itemFilters;
                    Session["filtersTMal"] = parsedFilters;
                    TempData.Clear();

                }
                else if (hasSession)
                {
                    //Si existia session.
                    parsedFilters = sessionFilters;
                    Session["filtersTMal"] = null;
                }

                foreach (var item in parsedFilters)
                {

                    TempData[item.field] = item.data;

                    if (item.field == "Transaccion" || item.field == "Motivo" || item.field == "IdTransaccionMal")
                    {
                        //Si es texto.
                        transacciones = transacciones.Where(string.Format("{0}.ToUpper().Contains(@0)", item.field), item.data.ToString());
                    }
                    else if (item.field == "FechaDescarga")
                    {
                        //Si es fecha
                        DateTime fech = Convert.ToDateTime(item.data);
                        transacciones = transacciones.Where(o => o.FechaDescarga.Value.Year == fech.Year && o.FechaDescarga.Value.Month == fech.Month && o.FechaDescarga.Value.Day == fech.Day);

                    }
                    else if (item.field == "Terminal")
                    {
                        transacciones = transacciones.Where(o => o.Terminal.Contains(item.data));
                    }
                    else if (item.field == "Operador")
                    {
                        transacciones = transacciones.Where(o => o.Operador.Contains(item.data));
                    }
                    else if (item.field == "Locacion")
                    {
                        transacciones = transacciones.Where(o => o.Locacion.Contains(item.data));
                    }
                    else if (item.field == "Maquina")
                    {
                        transacciones = transacciones.Where(o => o.Maquina.Contains(item.data));
                    }
                }
            }

            if (sidx != "" && sidx != null || hasSortSession)
            {

                if (!hasSortSession)
                {
                    Dictionary<string, string> sorted = new Dictionary<string, string>();
                    sorted.Add(sidx, sord);
                    Session["sortMal"] = sorted;
                }
                else if (sidx == "" || sidx == null)
                {
                    sidx = sessionSort.First().Key;
                    sord = sessionSort.First().Value;
                }


                switch (sidx)
                {
                    case "Transaccion":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Transaccion);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Transaccion);
                        break;

                    case "Motivo":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Motivo);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Motivo);
                        break;

                    case "IdTransaccionMal":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.IdTransaccionMal);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.IdTransaccionMal);
                        break;

                    case "FechaDescarga":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.FechaDescarga);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.FechaDescarga);
                        break;

                    case "Terminal":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Terminal);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Terminal);
                        break;

                    case "Operador":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Operador);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Operador);
                        break;

                    case "Maquina":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Maquina);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Maquina);
                        break;

                    case "Locacion":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Locacion);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Locacion);
                        break;
                }
            }
            else
                transacciones = transacciones.OrderByDescending(s => s.IdTransaccionMal);

            //Get Total Row Count
            int totalRecords = transacciones.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            TempData.Keep();

            transacciones = transacciones.Skip(pageIndex * pageSize).Take(pageSize);

            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = transacciones
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [Audit]
        public JsonResult GetAllGrupoTransaccionesMal(string sidx, string sord, int page, int rows, string filters)
        {
            Guid operadorID = GetUserOperadorID();
            var sessionFilters = Session["filtersGrupoTMal"] as List<Filter>;
            var sessionSort = Session["sortGrupoTMal"] as Dictionary<string, string>;
            bool hasFilter = false;
            bool hasSession = false;
            bool hasSortSession = false;

            if (sessionFilters != null && sessionFilters.Any())
                hasSession = true;

            if (filters != null)
                hasFilter = true;

            if (sessionSort != null && sessionSort.Any())
                hasSortSession = true;

            List<Filter> parsedFilters = null;

            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;

            var transacciones = db.GrupoTransaccionesMal.Where(x => operadorID == Guid.Empty)//.ToList()
            .Select(x => new
            {
                NSerie = x.NSerie,
                Fecha = x.Fecha,
                Motivo = x.Motivo,
                TextoIn = x.TextoIn,
            });

            if (hasFilter || hasSession)
            {
                if (hasFilter)
                {//Unicamente filters viene vacio en la primer carga.

                    JObject data = JsonConvert.DeserializeObject<JObject>(filters);
                    var itemFilters = data.Last.Values().Select(x => x.ToObject<Filter>()).ToList();
                    parsedFilters = itemFilters;
                    Session["filtersGrupoTMal"] = parsedFilters;
                    TempData.Clear();
                }
                else if (hasSession)
                {
                    //Si existia session.
                    parsedFilters = sessionFilters;
                    Session["filtersGrupoTMal"] = null;
                }

                foreach (var item in parsedFilters)
                {
                    TempData[item.field] = item.data;

                    if (item.field == "TextoIn" || item.field == "Motivo")
                    {
                        //Si es texto.
                        transacciones = transacciones.Where(string.Format("{0}.ToUpper().Contains(@0)", item.field), item.data.ToString());
                    }
                    else if (item.field == "Fecha")
                    {
                        //Si es fecha
                        DateTime fech = Convert.ToDateTime(item.data);
                        transacciones = transacciones.Where(o => o.Fecha.HasValue && o.Fecha.Value.Year == fech.Year && o.Fecha.Value.Month == fech.Month && o.Fecha.Value.Day == fech.Day);
                    }
                }
            }

            if (sidx != "" && sidx != null || hasSortSession)
            {
                if (!hasSortSession)
                {
                    Dictionary<string, string> sorted = new Dictionary<string, string>();
                    sorted.Add(sidx, sord);
                    Session["sortGrupoMal"] = sorted;
                }
                else if (sidx == "" || sidx == null)
                {
                    sidx = sessionSort.First().Key;
                    sord = sessionSort.First().Value;
                }

                switch (sidx)
                {
                    case "TextoIn":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.TextoIn);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.TextoIn);
                        break;

                    case "Motivo":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Motivo);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Motivo);
                        break;

                    case "Fecha":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Fecha);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Fecha);
                        break;

                    case "NumeroSerie":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.NSerie);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.NSerie);
                        break;
                }
            }
            else
                transacciones = transacciones.OrderByDescending(s => s.Fecha);

            //Get Total Row Count
            int totalRecords = transacciones.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            TempData.Keep();

            transacciones = transacciones.Skip(pageIndex * pageSize).Take(pageSize);

            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = transacciones
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeUser(accion = "ClearSession", controlador = "Usuario")]
        public ActionResult ClearSession()
        {
            Session[JQGridFilterRule.Keys.Transaccion.FILTERS] = null;
            Session[JQGridFilterRule.Keys.Transaccion.SORD] = null;
            Session[JQGridFilterRule.Keys.Transaccion.SIDX] = null;
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllTransacciones(string sidx, string sord, int page, int rows, string filters)
        {
            Guid operadorID = GetUserOperadorID();
            var sessionFilters =  Session[JQGridFilterRule.Keys.Transaccion.FILTERS] as List<Filter>;
            var sessionSort =  Session[JQGridFilterRule.Keys.Transaccion.SIDX] as Dictionary<string, string>;
            bool hasFilter = false;
            bool hasSession = false;
            bool hasSortSession = false;

            if (sessionFilters != null && sessionFilters.Any())
                hasSession = true;

            if (!string.IsNullOrEmpty(filters))
                hasFilter = true;

            if (sessionSort != null && sessionSort.Any())
                hasSortSession = true;

            List<Filter> parsedFilters = null;


            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;

            var transacciones = db.ViewTransaccion.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID).Select(x => new
            {
                TransaccionID = x.TransaccionID,
                OperadorNombre = x.OperadorNombre,
                FechaAltaBase = x.FechaAltaBase,
                FechaTransaccion = x.FechaTransaccion,
                CodigoTransaccion = x.CodigoTransaccion,
                EfectivoInicial = x.EfectivoInicial,
                EfectivoFinal = x.EfectivoFinal,
                CreditoInicialZona1 = x.CreditoInicialZona1,
                CreditoInicialZona2 = x.CreditoInicialZona2,
                CreditoInicialZona3 = x.CreditoInicialZona3,
                CreditoInicialZona4 = x.CreditoInicialZona4,
                CreditoInicialZona5 = x.CreditoInicialZona5,
                CreditoFinalZona1 = x.CreditoFinalZona1,
                CreditoFinalZona2 = x.CreditoFinalZona2,
                CreditoFinalZona3 = x.CreditoFinalZona3,
                CreditoFinalZona4 = x.CreditoFinalZona4,
                CreditoFinalZona5 = x.CreditoFinalZona5,
                NombreZona1 = x.NombreZona1,
                NombreZona2 = x.NombreZona2,
                NombreZona3 = x.NombreZona3,
                NombreZona4 = x.NombreZona4,
                NombreZona5 = x.NombreZona5,
                ValorVenta = x.ValorVenta,
                ValorRecarga = x.ValorRecarga,
                DescuentoAplicado = x.DescuentoAplicado,
                //UsuarioService = x.UsuarioService,
                TransaccionTexto = x.TextoTransaccion,
                Articulo = (x.Articulo != null) ? x.Articulo : "Artículo " + x.ValorVenta.ToString(),
                ModeloTerminal = (x.ModeloTerminal != null) ? x.ModeloTerminal : "Modelo Terminal NO Registrada",
                Terminal = (x.Terminal != null) ? x.Terminal.ToString() : "Terminal NO Registrada.",
                Maquina = (x.Maquina != null) ? x.Maquina.ToString() : "Máquina NO Registrada.",
                Locacion = (x.Locacion != null) ? x.Locacion.ToString() : "Locación NO Registrada.",
                Usuario = (x.UsuarioID != null) ? ((!String.IsNullOrEmpty(x.UsuarioNombre)) ? x.UsuarioNombre + ", " + x.UsuarioApellido : "Consumidor " + x.NumeroUsuario) : "Usuario NO Registrado.",
                NumeroUsuario = (x.NumeroUsuario != null) ? x.NumeroUsuario : 0,
                Jerarquia = (x.Jerarquia != null) ? x.Jerarquia : "Jerarquia NO Registrada.",
                ValorRecorte = (x.ValorRecorte != null) ? x.ValorRecorte : 0
            });

            //https://forums.asp.net/t/2142303.aspx?Sending+jqGrid+Data+To+MVC+Controller+Method+Via+EditUrl

            if (hasFilter || hasSession)
            {

                if (hasFilter)
                {//Unicamente filters viene vacio en la primer carga.

                    JObject data = JsonConvert.DeserializeObject<JObject>(filters);
                    var itemFilters = data.Last.Values().Select(x => x.ToObject<Filter>()).ToList();
                    parsedFilters = itemFilters;
                     Session[JQGridFilterRule.Keys.Transaccion.FILTERS] = parsedFilters;
                    TempData.Clear();

                }
                else if (hasSession)
                {
                    //Si existia session.
                    parsedFilters = sessionFilters;
                     Session[JQGridFilterRule.Keys.Transaccion.FILTERS] = null;
                }

                foreach (var item in parsedFilters)
                {

                    TempData[item.field] = item.data;

                    if (item.field == "TransaccionTexto" || item.field == "Usuario" || item.field == "CodigoTransaccion" || item.field == "OperadorNombre")
                    {
                        //Si es texto.
                        transacciones = transacciones.Where(string.Format("{0}.ToUpper().Contains(@0)", item.field), item.data.ToString());
                    }
                    else if (item.field == "ValorVenta" || item.field == "DescuentoAplicado" || item.field == "EfectivoFinal" ||
                              item.field == "ValorRecarga" || item.field == "EfectivoInicial" || item.field == "ValorRecorte")
                    {
                        //Si es decimal
                        transacciones = transacciones.Where(string.Format("{0} = @0", item.field), Convert.ToDecimal(item.data));
                    }
                    else if (item.field == "FechaTransaccion" || item.field == "FechaAltaBase")
                    {
                        //Si es fecha
                        DateTime fech = Convert.ToDateTime(item.data);
                        if (item.field == "FechaTransaccion")
                            transacciones = transacciones.Where(o => o.FechaTransaccion.Value.Year == fech.Year && o.FechaTransaccion.Value.Month == fech.Month && o.FechaTransaccion.Value.Day == fech.Day);
                        else
                            transacciones = transacciones.Where(o => o.FechaAltaBase.Value.Year == fech.Year && o.FechaAltaBase.Value.Month == fech.Month && o.FechaAltaBase.Value.Day == fech.Day);
                    }
                    else
                    {

                        //Filtros de otras entidades.
                        switch (item.field.ToString())
                        {
                            case "Jerarquia":
                                transacciones = transacciones.Where(o => o.Jerarquia.Contains(item.data));
                                break;

                            case "Locacion":
                                transacciones = transacciones.Where(o => o.Locacion.Contains(item.data));
                                break;

                            case "Articulo":
                                transacciones = transacciones.Where(o => o.Articulo.Contains(item.data));
                                break;

                            case "Maquina":
                                transacciones = transacciones.Where(o => o.Maquina.Contains(item.data));
                                break;

                            case "Terminal":
                                transacciones = transacciones.Where(o => o.Terminal.Contains(item.data));
                                break;

                            case "ModeloTerminal":
                                transacciones = transacciones.Where(o => o.ModeloTerminal.Contains(item.data));
                                break;

                            case "NumeroUsuario":
                                int value = Convert.ToInt32(item.data);
                                transacciones = transacciones.Where(o => o.NumeroUsuario.HasValue ? o.NumeroUsuario.Value == value : o.NumeroUsuario != null);
                                break;
                        }
                    }
                }

            }


            if (sidx != "" && sidx != null || hasSortSession)
            {

                if (!hasSortSession)
                {
                    Dictionary<string, string> sorted = new Dictionary<string, string>();
                    sorted.Add(sidx, sord);
                     Session[JQGridFilterRule.Keys.Transaccion.SIDX]= sorted;
                }
                else if (sidx == "" || sidx == null)
                {
                    sidx = sessionSort.First().Key;
                    sord = sessionSort.First().Value;
                }


                switch (sidx)
                {
                    case "OperadorNombre":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.OperadorNombre);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.OperadorNombre);
                        break;

                    case "TransaccionTexto":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.TransaccionTexto);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.TransaccionTexto);
                        break;

                    case "Usuario":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Usuario);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Usuario);
                        break;

                    case "CodigoTransaccion":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.CodigoTransaccion);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.CodigoTransaccion);
                        break;

                    case "ValorVenta":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.ValorVenta);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.ValorVenta);
                        break;

                    case "DescuentoAplicado":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.DescuentoAplicado);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.DescuentoAplicado);
                        break;

                    case "EfectivoFinal":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.EfectivoFinal);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.EfectivoFinal);
                        break;

                    case "ValorRecarga":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.ValorRecarga);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.ValorRecarga);
                        break;

                    case "EfectivoInicial":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.EfectivoInicial);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.EfectivoInicial);
                        break;

                    case "FechaTransaccion":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.FechaTransaccion);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.FechaTransaccion);
                        break;

                    case "FechaAltaBase":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.FechaAltaBase);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.FechaAltaBase);
                        break;

                    case "Jerarquia":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Jerarquia);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Jerarquia);
                        break;

                    case "Locacion":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Locacion);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Locacion);
                        break;

                    case "Articulo":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Articulo);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Articulo);
                        break;

                    case "Maquina":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Maquina);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Maquina);
                        break;

                    case "Terminal":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.Terminal);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.Terminal);
                        break;

                    case "ModeloTerminal":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.ModeloTerminal);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.ModeloTerminal);
                        break;

                    case "NumeroUsuario":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.NumeroUsuario);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.NumeroUsuario);
                        break;

                    case "ValorRecorte":
                        if (sord == "asc")
                            transacciones = transacciones.OrderBy(o => o.ValorRecorte);
                        else
                            transacciones = transacciones.OrderByDescending(o => o.ValorRecorte);
                        break;
                }




            }
            else
                transacciones = transacciones.OrderByDescending(s => s.FechaTransaccion);


            //Get Total Row Count
            int totalRecords = transacciones.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            TempData.Keep();

            //Setting Sorting
            //if (sord.ToUpper() == "DESC")
            //{

            transacciones = transacciones.Skip(pageIndex * pageSize).Take(pageSize);
            //}
            //else
            //{
            //    transacciones = transacciones.OrderBy(s => s.CustomerID);
            //    transacciones = transacciones.Skip(pageIndex * pageSize).Take(pageSize);
            //}
            //Setting Search
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //transacciones = transacciones.Where(m => m.CompanyName == searchString || m.ContactName == searchString);
            //}
            //Sending Json Object to View.


            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = transacciones
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [Audit]
        public ActionResult ExportData(string jqGridPostData)
        {
            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            JQGridPostData postData = JsonConvert.DeserializeObject<JQGridPostData>(fixedPostData);

            string filters = String.Empty;
            Dictionary<string, DateTime> dateFilters = new Dictionary<string, DateTime>();

            if (postData.filters != null)
            {

                for (int i = 0; i < postData.filters.rules.Count; i++)
                {
                    string col = postData.filters.rules[i].field;
                    string data = postData.filters.rules[i].data.ToLower();

                    //if (i > 0)
                    //{
                    //    filters += " && ";
                    //}
                    //else
                    //{
                    //    filters = string.Empty;
                    //}

                    if (col == "FechaTransaccion" || col == "FechaAltaBase")
                    {
                        dateFilters.Add(col, Convert.ToDateTime(data));
                    }
                    else
                    {
                        if (filters != String.Empty)
                            filters += " && ";

                        filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";

                    }
                }
            }
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");
            Guid operadorID = GetUserOperadorID();

            var transacciones = db.Transacciones.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID)
              .Select(x => new
              {
                  Locacion = (x.Locacion != null) ? x.Locacion.Nombre : "Locación NO Registrada.",
                  OperadorNombre = x.Operador.Nombre,
                  Articulo = (x.Articulo != null) ? x.Articulo.Nombre : "Artículo " + x.ValorVenta.ToString(),
                  Maquina = (x.Maquina != null) ? x.Maquina.NumeroSerie : "Máquina NO Registrada.",
                  ModeloTerminal = x.ModeloTerminal != null ? x.ModeloTerminal.Modelo : "Modelo Terminal no Registrada",
                  Terminal = (x.Terminal != null) ? x.Terminal.NumeroSerie.ToString() : "Terminal NO Registrada.",
                  Jerarquia = (x.Jerarquia != null) ? x.Jerarquia.Nombre : "Jerarquia NO Registrada.",
                  //Usuario = (x.Usuario != null) ? x.Usuario.Nombre : "Usuario NO Registrado.",
                  Usuario = (x.Usuario != null) ? ((!String.IsNullOrEmpty(x.Usuario.Nombre)) ? x.Usuario.Nombre + ", " + x.Usuario.Apellido : "Consumidor " + x.Usuario.Numero) : "Usuario NO Registrado.",
                  NumeroUsuario = (x.Usuario != null) ? x.Usuario.Numero : 0,
                  NombreZona1 = (x.Locacion != null) ? x.Locacion.NombreZona1 : "",
                  CreditoInicialZona1 = x.CreditoInicialZona1,
                  CreditoFinalZona1 = x.CreditoFinalZona1,
                  NombreZona2 = (x.Locacion != null) ? x.Locacion.NombreZona2 : "",
                  CreditoInicialZona2 = x.CreditoInicialZona2,
                  CreditoFinalZona2 = x.CreditoFinalZona2,
                  NombreZona3 = (x.Locacion != null) ? x.Locacion.NombreZona3 : "",
                  CreditoInicialZona3 = x.CreditoInicialZona3,
                  CreditoFinalZona3 = x.CreditoFinalZona3,
                  NombreZona4 = (x.Locacion != null) ? x.Locacion.NombreZona4 : "",
                  CreditoInicialZona4 = x.CreditoInicialZona4,
                  CreditoFinalZona4 = x.CreditoFinalZona4,
                  NombreZona5 = (x.Locacion != null) ? x.Locacion.NombreZona5 : "",
                  CreditoInicialZona5 = x.CreditoInicialZona5,
                  CreditoFinalZona5 = x.CreditoFinalZona5,
                  FechaTransaccion = x.FechaTransaccion,
                  FechaAltaBase = x.FechaAltaBase,
                  CodigoTransaccion = x.CodigoTransaccion,
                  EfectivoInicial = x.EfectivoInicial,
                  EfectivoFinal = x.EfectivoFinal,
                  ValorVenta = x.ValorVenta,
                  ValorRecarga = x.ValorRecarga,
                  DescuentoAplicado = x.DescuentoAplicado,
                  //UsuarioService = x.UsuarioService,
                  TransaccionTexto = x.TransaccionTexto.TextoTransaccion,
                  ValorRecorte = x.ValorRecorte
              });

            if (filters.Replace(" ", "").Replace("&&", "") == "")
            {

            }
            else
            {
                transacciones = transacciones.Where(filters);
            }

            if (dateFilters.Any())
            {
                foreach (var item in dateFilters)
                {

                    if (item.Key == "FechaTransaccion")
                        transacciones = transacciones.Where(o => o.FechaTransaccion.Value.Year == item.Value.Year && o.FechaTransaccion.Value.Month == item.Value.Month && o.FechaTransaccion.Value.Day == item.Value.Day);
                    else if (item.Key == "FechaAltaBase")
                        transacciones = transacciones.Where(o => o.FechaAltaBase.Value.Year == item.Value.Year && o.FechaAltaBase.Value.Month == item.Value.Month && o.FechaAltaBase.Value.Day == item.Value.Day);
                }
            }

            var pkg = new ExcelPackage();
            var workbook = pkg.Workbook;
            string doubleFormat = "$#,0.00";
            var sheet = workbook.Worksheets.Add("Transacciones");
            int amountOfColumns = 1;

            sheet.InsertRow(1, 1);
            ExcelRow headerRow = sheet.Row(1);

            if (esSuperAdmin)
                sheet.Cells[1, amountOfColumns++].Value = "Operador";

            sheet.Cells[1, amountOfColumns++].Value = "Tipo Transacción";
            sheet.Cells[1, amountOfColumns++].Value = "Código Transacción";
            sheet.Cells[1, amountOfColumns++].Value = "Valor Venta";
            sheet.Cells[1, amountOfColumns++].Value = "Descuento Aplicado";
            sheet.Cells[1, amountOfColumns++].Value = "Artículo";
            sheet.Cells[1, amountOfColumns++].Value = "Fecha Transacción";
            sheet.Cells[1, amountOfColumns++].Value = "Fecha Alta Base";
            sheet.Cells[1, amountOfColumns++].Value = "Usuario";
            sheet.Cells[1, amountOfColumns++].Value = "Número Usuario";
            sheet.Cells[1, amountOfColumns++].Value = "Jerarquía";
            sheet.Cells[1, amountOfColumns++].Value = "Locación";
            sheet.Cells[1, amountOfColumns++].Value = "Máquina";
            sheet.Cells[1, amountOfColumns++].Value = "Terminal";
            sheet.Cells[1, amountOfColumns++].Value = "Modelo Terminal";
            sheet.Cells[1, amountOfColumns++].Value = "Valor Recarga";
            sheet.Cells[1, amountOfColumns++].Value = "Valor Recorte";
            sheet.Cells[1, amountOfColumns++].Value = "Efectivo Inicial";
            sheet.Cells[1, amountOfColumns++].Value = "Efectivo Final";
            sheet.Cells[1, amountOfColumns++].Value = "Nombre Zona 1";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Inicial Zona 1";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Final Zona 1";
            sheet.Cells[1, amountOfColumns++].Value = "Nombre Zona 2";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Inicial Zona 2";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Final Zona 2";
            sheet.Cells[1, amountOfColumns++].Value = "Nombre Zona 3";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Inicial Zona 3";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Final Zona 3";
            sheet.Cells[1, amountOfColumns++].Value = "Nombre Zona 4";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Inicial Zona 4";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Final Zona 4";
            sheet.Cells[1, amountOfColumns++].Value = "Nombre Zona 5";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Inicial Zona 5";
            sheet.Cells[1, amountOfColumns++].Value = "Crédito Final Zona 5";

            var exportHelper = new EPPlusExcelExportHelper(workbook, doubleFormat);
            var headerCellStyle = exportHelper.GetHeaderCellStyle();

            for (int i = 1; i < amountOfColumns; i++)
            {
                sheet.Cells[1, i].StyleName = headerCellStyle.Name;
            }

            var rowNumber = 2;
            int colIdx;
            var results = transacciones.ToList().OrderByDescending(x => x.FechaTransaccion);
            foreach (var transaccion in results)
            {
                colIdx = 1;

                if (esSuperAdmin)
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.OperadorNombre);

                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.TransaccionTexto);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.CodigoTransaccion);
                exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.ValorVenta);
                exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.DescuentoAplicado);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Articulo);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.FechaTransaccion.HasValue ? transaccion.FechaTransaccion.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.FechaAltaBase.HasValue ? transaccion.FechaAltaBase.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Usuario);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NumeroUsuario.ToString());
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Jerarquia);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Locacion);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Maquina);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Terminal);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.ModeloTerminal);
                exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.ValorRecarga);
                exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.ValorRecorte.HasValue ? transaccion.ValorRecorte.Value : 0);
                exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.EfectivoInicial);
                exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.EfectivoFinal);
                if (!string.IsNullOrEmpty(transaccion.NombreZona1))
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NombreZona1);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoInicialZona1);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoFinalZona1);
                }
                else
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                }
                if (!string.IsNullOrEmpty(transaccion.NombreZona2))
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NombreZona2);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoInicialZona2);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoFinalZona2);
                }
                else
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                }
                if (!string.IsNullOrEmpty(transaccion.NombreZona3))
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NombreZona3);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoInicialZona3);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoFinalZona3);
                }
                else
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                }
                if (!string.IsNullOrEmpty(transaccion.NombreZona4))
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NombreZona4);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoInicialZona4);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoFinalZona4);
                }
                else
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                }
                if (!string.IsNullOrEmpty(transaccion.NombreZona5))
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NombreZona5);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoInicialZona5);
                    exportHelper.CreateNumericCell(sheet, rowNumber, colIdx++, transaccion.CreditoFinalZona5);
                }
                else
                {
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                    exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, "");
                }

                rowNumber++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            MemoryStream output = new MemoryStream();
            pkg.SaveAs(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Transacciones " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        [Audit]
        public ActionResult ExportDataTransaccionesMal(string jqGridPostData)
        {
            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            JQGridPostData postData = JsonConvert.DeserializeObject<JQGridPostData>(fixedPostData);

            string filters = String.Empty;
            Dictionary<string, DateTime> dateFilters = new Dictionary<string, DateTime>();

            if (postData.filters != null)
            {

                for (int i = 0; i < postData.filters.rules.Count; i++)
                {
                    string col = postData.filters.rules[i].field;
                    string data = postData.filters.rules[i].data.ToLower();

                    if (col == "FechaDescarga")
                    {
                        dateFilters.Add(col, Convert.ToDateTime(data));
                    }
                    else
                    {
                        if (filters != String.Empty)
                            filters += " && ";

                        filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";

                    }
                }
            }
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");
            Guid operadorID = GetUserOperadorID();

            var transacciones = db.TransaccionesMal.Where(x => operadorID == Guid.Empty || x.OperadorID == operadorID)
                .Select(x => new
                {
                    Terminal = (x.Terminal != null) ? x.Terminal.NumeroSerie.ToString() : "Terminal NO Registrada.",
                    Operador = (x.Operador != null) ? x.Operador.Nombre.ToString() : "Operador NO Registrado.",
                    Maquina = (x.Maquina != null) ? x.Maquina.NumeroSerie.ToString() : "Máquina NO Registrada.",
                    Locacion = (x.Locacion != null) ? x.Locacion.Nombre.ToString() : "Locación NO Registrada.",
                    //Operador = (x.Operador != null) ? x.Operador.OperadorID.ToString() : "Operador NO Registrado.",
                    //Maquina = (x.Maquina != null) ? x.Maquina.MaquinaID.ToString() : "Máquina NO Registrada.",
                    //Locacion = (x.Locacion != null) ? x.Locacion.LocacionID.ToString() : "Locación NO Registrada.",
                    Transaccion = x.Transaccion,
                    IdTransaccionMal = x.IdTransaccionMal.ToString(),
                    Motivo = x.Motivo,
                    FechaDescarga = x.FechaDescarga,
                });

            if (filters.Replace(" ", "").Replace("&&", "") == "")
            {

            }
            else
            {
                transacciones = transacciones.Where(filters);
            }

            if (dateFilters.Any())
            {
                foreach (var item in dateFilters)
                {

                    if (item.Key == "FechaDescarga")
                        transacciones = transacciones.Where(o => o.FechaDescarga.HasValue && o.FechaDescarga.Value.Year == item.Value.Year && o.FechaDescarga.Value.Month == item.Value.Month && o.FechaDescarga.Value.Day == item.Value.Day);
                }
            }

            var pkg = new ExcelPackage();
            var workbook = pkg.Workbook;
            string doubleFormat = "$#,0.00";
            var sheet = workbook.Worksheets.Add("Transacciones Mal");
            int amountOfColumns = 1;

            sheet.InsertRow(1, 1);
            ExcelRow headerRow = sheet.Row(1);

            //if (esSuperAdmin)
            sheet.Cells[1, amountOfColumns++].Value = "Operador";
            sheet.Cells[1, amountOfColumns++].Value = "Número Terminal";
            sheet.Cells[1, amountOfColumns++].Value = "Locación";
            sheet.Cells[1, amountOfColumns++].Value = "Máquina";
            sheet.Cells[1, amountOfColumns++].Value = "Fecha descarga";
            sheet.Cells[1, amountOfColumns++].Value = "Motivo";
            sheet.Cells[1, amountOfColumns++].Value = "Transacción";

            var exportHelper = new EPPlusExcelExportHelper(workbook, doubleFormat);
            var headerCellStyle = exportHelper.GetHeaderCellStyle();

            for (int i = 1; i < amountOfColumns; i++)
            {
                sheet.Cells[1, i].StyleName = headerCellStyle.Name;
            }

            var rowNumber = 2;
            int colIdx;
            var results = transacciones.ToList().OrderByDescending(x => x.FechaDescarga);
            foreach (var transaccion in results)
            {
                colIdx = 1;

                //if (esSuperAdmin)
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Operador);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Terminal);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Locacion);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Maquina);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.FechaDescarga.HasValue ? transaccion.FechaDescarga.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Motivo);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Transaccion);

                rowNumber++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            MemoryStream output = new MemoryStream();
            pkg.SaveAs(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Transacciones Mal " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }

        public ActionResult ExportDataGrupoTransaccionesMal(string jqGridPostData)
        {
            string fixedPostData = jqGridPostData.Replace(@"\", "").Replace(@"""{", "{").Replace(@"}""", "}");

            JQGridPostData postData = JsonConvert.DeserializeObject<JQGridPostData>(fixedPostData);

            string filters = String.Empty;
            Dictionary<string, DateTime> dateFilters = new Dictionary<string, DateTime>();

            if (postData.filters != null)
            {

                for (int i = 0; i < postData.filters.rules.Count; i++)
                {
                    string col = postData.filters.rules[i].field;
                    string data = postData.filters.rules[i].data.ToLower();

                    if (col == "Fecha")
                    {
                        dateFilters.Add(col, Convert.ToDateTime(data));
                    }
                    else
                    {
                        if (filters != String.Empty)
                            filters += " && ";

                        filters += " " + col + ".ToString().ToLower().Contains(\"" + data + "\") ";

                    }
                }
            }
            var esSuperAdmin = SecurityHelper.IsInRole("SuperAdmin");
            Guid operadorID = GetUserOperadorID();

            var transacciones = db.GrupoTransaccionesMal
                .Select(x => new
                {
                    NSerie = x.NSerie,
                    Fecha = x.Fecha,
                    Motivo = x.Motivo,
                    TextoIn = x.TextoIn,
                });

            if (filters.Replace(" ", "").Replace("&&", "") == "")
            {

            }
            else
            {
                transacciones = transacciones.Where(filters);
            }

            if (dateFilters.Any())
            {
                foreach (var item in dateFilters)
                {

                    if (item.Key == "Fecha")
                        transacciones = transacciones.Where(o => o.Fecha.HasValue && o.Fecha.HasValue && o.Fecha.Value.Year == item.Value.Year && o.Fecha.Value.Month == item.Value.Month && o.Fecha.Value.Day == item.Value.Day);
                }
            }

            var pkg = new ExcelPackage();
            var workbook = pkg.Workbook;
            string doubleFormat = "$#,0.00";
            var sheet = workbook.Worksheets.Add("Grpo Transacciones Mal");
            int amountOfColumns = 1;

            sheet.InsertRow(1, 1);
            ExcelRow headerRow = sheet.Row(1);

            //if (esSuperAdmin)
            sheet.Cells[1, amountOfColumns++].Value = "Terminal";
            sheet.Cells[1, amountOfColumns++].Value = "Fecha";
            sheet.Cells[1, amountOfColumns++].Value = "Motivo";
            sheet.Cells[1, amountOfColumns++].Value = "Texto In";

            var exportHelper = new EPPlusExcelExportHelper(workbook, doubleFormat);
            var headerCellStyle = exportHelper.GetHeaderCellStyle();

            for (int i = 1; i < amountOfColumns; i++)
            {
                sheet.Cells[1, i].StyleName = headerCellStyle.Name;
            }

            var rowNumber = 2;
            int colIdx;
            var results = transacciones.ToList().OrderByDescending(x => x.Fecha);
            foreach (var transaccion in results)
            {
                colIdx = 1;

                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.NSerie.ToString());
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Fecha.HasValue ? transaccion.Fecha.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.Motivo);
                exportHelper.CreateTextCell(sheet, rowNumber, colIdx++, transaccion.TextoIn);

                rowNumber++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            MemoryStream output = new MemoryStream();
            pkg.SaveAs(output);

            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte de Grupo Transacciones Mal " + DateTime.Now.ToString("dd-MM-yyyy hhmmss") + ".xlsx");
        }


        // GET: Transaccion/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaccion transaccion = db.Transacciones.Find(id);
            if (transaccion == null)
            {
                return HttpNotFound();
            }
            return View(transaccion);
        }

        public ActionResult DetailsTMal(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransaccionesMal transaccion = db.TransaccionesMal.Find(id);
            if (transaccion == null)
            {
                return HttpNotFound();
            }
            return View("DetailsTMal", transaccion);
        }

        public ActionResult DeleteTMRange()
        {
            TransaccionDeleteRangeViewModel viewModel = new TransaccionDeleteRangeViewModel();
            return View(viewModel);
        }


        public ActionResult DeleteRange()
        {
            TransaccionDeleteRangeViewModel viewModel = new TransaccionDeleteRangeViewModel();
            return View(viewModel);
        }

        [Audit]
        [HttpPost]
        public ActionResult DeleteTMRange(TransaccionDeleteRangeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.FechaHasta = viewModel.FechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
                Guid operadorID = GetUserOperadorID();

                List<TransaccionesMal> transaccionesMal = db.TransaccionesMal.Where(x => x.FechaDescarga.HasValue &&
                x.FechaDescarga.Value >= viewModel.FechaDesde &&
                x.FechaDescarga.Value <= viewModel.FechaHasta &&
                (operadorID == Guid.Empty || x.OperadorID == operadorID)).ToList();

                db.TransaccionesMal.RemoveRange(transaccionesMal);
                db.SaveChanges();
                return RedirectToAction("IndexMal");
            }

            return View(viewModel);
        }

        [Audit]
        [HttpPost]
        public ActionResult DeleteRange(TransaccionDeleteRangeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.FechaHasta = viewModel.FechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
                Guid operadorID = GetUserOperadorID();

                List<Transaccion> transacciones = db.Transacciones.Where(x => x.FechaTransaccion.HasValue &&
                x.FechaTransaccion.Value >= viewModel.FechaDesde &&
                x.FechaTransaccion.Value <= viewModel.FechaHasta &&
                (operadorID == Guid.Empty || x.OperadorID == operadorID)).ToList();

                db.Transacciones.RemoveRange(transacciones);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Transaccion entity = db.Transacciones.Find(id);

            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        public ActionResult DeleteTMal(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TransaccionesMal entity = db.TransaccionesMal.Find(id);

            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        // POST: Operador/Delete/5
        [Audit]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();

            Transaccion entity = db.Transacciones.Find(id);
            db.Transacciones.Remove(entity);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Audit]
        [HttpPost, ActionName("DeleteTMal")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTMalConfirmed(Guid id)
        {
            db = HttpContext.GetOwinContext().Get<BugsContext>();

            TransaccionesMal entity = db.TransaccionesMal.Find(id);
            db.TransaccionesMal.Remove(entity);
            db.SaveChanges();

            return RedirectToAction("IndexMal");
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

        public JsonResult DeleteAll()
        {
            db.TransaccionesMal.ToList().ForEach(x => db.TransaccionesMal.Remove(x));
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private class Filter
        {
            public string field { get; set; }
            public string op { get; set; }
            public string data { get; set; }
        }
    }
}
