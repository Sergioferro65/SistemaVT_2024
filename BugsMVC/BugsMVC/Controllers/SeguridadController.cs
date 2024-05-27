        using BugsMVC.Commands.Seguridad;
        using BugsMVC.DAL;
        using BugsMVC.Handlers;
        using BugsMVC.Models;
        using BugsMVC.Models.ViewModels;
        using BugsMVC.Security;
        using System;
        using System.Collections.Generic;
        using System.Data.Entity;
        using System.Linq;
        using System.Net;
        using System.Web;
        using System.Web.Mvc;

        namespace BugsMVC.Controllers
        {
            [AuthorizeRoles]
            public class SeguridadController : BaseController
            {
                private BugsContext db = new BugsContext();

                public JsonResult GetConfiguracionSeguridad(string IdRol)
                {
                    List<int> funcionesNoListadas = new List<int>();
                    funcionesNoListadas.Add((int)Helpers.Permissions.Auditoria.Index);
                    funcionesNoListadas.Add((int)Helpers.Permissions.ModelosMaquina.Index);
                    funcionesNoListadas.Add((int)Helpers.Permissions.ModeloTerminal.Index);
                    funcionesNoListadas.Add((int)Helpers.Permissions.Operadores.Index);
                    funcionesNoListadas.Add((int)Helpers.Permissions.Seguridad.Seguridad);
                    funcionesNoListadas.Add((int)Helpers.Permissions.TransaccionTexto.Index);
                    funcionesNoListadas.Add((int)Helpers.Permissions.Consumidor.Index);
                    funcionesNoListadas.Add((int)Helpers.Permissions.Consumidor.MiCuenta);

                    var query =
                                  (from fact in db.Funciones.Where(x =>  !funcionesNoListadas.Contains(x.Id))
                                   join desc in db.FuncionRoles.Where(x => x.IdRol == IdRol) on fact.Id equals desc.IdFuncion
                                   into FactDesc
                                   from fd in FactDesc.DefaultIfEmpty()
                                   select new SeguridadDetalleViewModel
                                   {
                                       TieneAcceso = fd.IdFuncion == null ? "NO" : "SI",
                                       IdFuncion = fact.Id,
                                       Funcion = fact.Descripcion,
                                      // Operador = fact.Operador.Nombre julian
                                   }
                                  ).OrderBy(x=>x.Funcion).ToList();

                    return Json(query.ToArray(), JsonRequestBehavior.AllowGet);
                }

                public ActionResult Create()
                {
                    ViewBag.IdRol = new SelectList(db.Roles.Where(x=>x.Name != "Consumidor").OrderBy(x => x.Weight), "Id", "Name");

                    SeguridadViewModel viewModel = new SeguridadViewModel();
                    return View(viewModel);
                }

                public JsonResult CambiarEstado(int IdFuncion, string IdRol)
                {
                    FuncionRol entity = db.FuncionRoles.Where(x => x.IdFuncion == IdFuncion && x.IdRol == IdRol).FirstOrDefault() ?? new FuncionRol();

                    if (entity.Id != 0)
                    {                
                        db.FuncionRoles.Remove(entity);
                    }
                    else
                    {
                        entity.IdFuncion = IdFuncion;
                        entity.IdRol = IdRol;
                        db.FuncionRoles.Add(entity);
                    }

                    db.SaveChanges();

                    return Json("", JsonRequestBehavior.AllowGet);
                }

                public ActionResult CreateSuccess()
                {
                    return View();
                }

                public ActionResult PorInforme()
                { 
                    SeguridadPorInformeViewModel viewModel = new SeguridadPorInformeViewModel();

                    ViewBag.FuncionID = new SelectList(db.Funciones.Where(x=>x.PorOperador).OrderBy(x => x.Descripcion), "Id", "Descripcion");
                    ViewBag.Operadores = new MultiSelectList(string.Empty, "OperadorID", "Nombre");
                    return View(viewModel);
                }

                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult PorInforme([Bind(Include = "FuncionID,Operadores")] SeguridadPorInformeViewModel viewModel)
                {
                    if (viewModel.FuncionID == 0)
                    {
                        ModelState.AddModelError("FuncionID", "Debe seleccionar un Informe");
                    }

                    if (ModelState.IsValid)
                    {
                        List<Guid> operadores = (viewModel.Operadores != null) ? viewModel.Operadores.Select(x => new Guid(x)).ToList() : new List<Guid>();

                        var command = new SeguridadPorInformeCommand();
                        command.Configure(viewModel.FuncionID, operadores, db).Execute();

                        return RedirectToAction("CreateSuccess");
                    }

                    ViewBag.FuncionID = new SelectList(db.Funciones.Where(x => x.PorOperador).OrderBy(x => x.Descripcion), "Id", "Descripcion", viewModel.FuncionID);

                        ViewBag.Operadores = new MultiSelectList(string.Empty, "OperadorID", "Nombre");
                        
                    return View("PorInforme", viewModel);
                }

                public JsonResult GetOperadoresByFuncionMultiSelectList(int funcionId)
                {
                    var valoresSeleccionados = db.FuncionOperador.Where(x=>x.FuncionId == funcionId).Select(x=>x.OperadorId.ToString()).ToArray();

                    var ret = new MultiSelectList(db.Operadores.Select(x => new SelectListItem()
                    {
                        Text = x.Nombre,
                        Value = x.OperadorID.ToString()
                    }).OrderBy(x => x.Text).ToList(), "Value", "Text");

                    return Json(new { operadores = ret, selectValue = valoresSeleccionados }, JsonRequestBehavior.AllowGet);
                }
            }
        }