using BugsMVC.DAL;
using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Commands.Alarma
{
    public class CreateAlarmaCommand
    {
        int TipoDeAlarmaID;
        Guid? LocacionID;
        Guid OperadorID;
        List<Guid> Usuarios;
        BugsContext Db;

        public CreateAlarmaCommand Configure(int tipoDeAlarmaID, Guid? locacionID, Guid operadorID, List<Guid> usuarios, BugsContext db)
        {
            TipoDeAlarmaID = tipoDeAlarmaID;
            LocacionID = locacionID;
            Usuarios = usuarios;
            OperadorID = operadorID;
            Db = db;
            return this;
        }

        public void Execute()
        {
            var alarmaConfiguracionDetalle = Db.AlarmaConfiguracionDetalle.Where(x =>x.AlarmaConfiguracion.OperadorID==OperadorID && x.AlarmaConfiguracion.TipoDeAlarmaID == TipoDeAlarmaID && x.AlarmaConfiguracion.LocacionID == LocacionID);

            var usuariosExistentes = alarmaConfiguracionDetalle.Select(y => y.UsuarioID);
            var intersecion = Usuarios.Intersect(usuariosExistentes);

            AlarmaConfiguracion alarmaConfiguracion = Db.AlarmaConfiguracion.Where(x => x.OperadorID == OperadorID && x.TipoDeAlarmaID == TipoDeAlarmaID && x.LocacionID == LocacionID).FirstOrDefault();

            if (alarmaConfiguracion == null)
            {
                alarmaConfiguracion = new AlarmaConfiguracion();
                alarmaConfiguracion.AlarmaConfiguracionID = Guid.NewGuid();
                alarmaConfiguracion.TipoDeAlarmaID = TipoDeAlarmaID;
                alarmaConfiguracion.LocacionID = LocacionID;
                alarmaConfiguracion.OperadorID = OperadorID; 
                Db.AlarmaConfiguracion.Add(alarmaConfiguracion);
            }

            Db.AlarmaConfiguracionDetalle.RemoveRange(alarmaConfiguracionDetalle.Where(x => !intersecion.Contains(x.UsuarioID)));

            foreach (var item in Usuarios.Where(x => !intersecion.Contains(x)) )
                {
                    AlarmaConfiguracionDetalle AlarmaConfiguracionDetalle = new AlarmaConfiguracionDetalle();
                    AlarmaConfiguracionDetalle.AlarmaConfiguracionDetalleID = Guid.NewGuid();
                    AlarmaConfiguracionDetalle.AlarmaConfiguracion = alarmaConfiguracion;
                    AlarmaConfiguracionDetalle.UsuarioID = item;
                    Db.AlarmaConfiguracionDetalle.Add(AlarmaConfiguracionDetalle);
                }
                        
            Db.SaveChanges();
        }
    }
}