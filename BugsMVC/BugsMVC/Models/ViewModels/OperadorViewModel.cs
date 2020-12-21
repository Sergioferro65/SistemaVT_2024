using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class OperadorViewModel
    {
        [StringLength(100)]
        public string Nombre { get; set; }

        [DisplayName("Número")]
        [Range(0, 65535, ErrorMessage = "El número de Operador debe estar entre 0 y 65535")]
        public int Numero { get; set; }

        public Guid OperadorID { get; set; }

        [DisplayName("Tiempo Aviso Inhibición (Minutos)")]
        [Range(1, 65535, ErrorMessage = "El Tiempo de Aviso Inhibición debe estar entre 1 y 65535")]
        public int TiempoAvisoInhibicion { get; set; }

        [DisplayName("Tiempo Aviso Conexión  (Minutos)")]
        [Range(1, 65535, ErrorMessage = "El Tiempo de Aviso Conexión debe estar entre 1 y 65535")]
        public int TiempoAvisoConexion { get; set; }

        //public Guid? OperadorAdminID { get; set; }

        public bool IsCreate { get; set; }

        //public virtual Operador OperadorAdmin { get; set; }

        public virtual RegisterViewModel ApplicationUser { get; set; }

        //public virtual ICollection<Locacion> Locaciones { get; set; }

        public static OperadorViewModel From(Operador entity)
        {
            OperadorViewModel viewModel = new OperadorViewModel();

            viewModel.OperadorID = entity.OperadorID;
            viewModel.Nombre = entity.Nombre;
            viewModel.Numero = entity.Numero;
            viewModel.TiempoAvisoConexion = entity.TiempoAvisoConexion;
            viewModel.TiempoAvisoInhibicion = entity.TiempoAvisoInhibicion;
            //viewModel.OperadorAdminID = entity.OperadorAdminID;

            return viewModel;
        }

        public Operador ToEntity(Operador entity)
        {
            entity.OperadorID = this.OperadorID;
            entity.Nombre = this.Nombre;
            entity.Numero = this.Numero;
            entity.TiempoAvisoConexion = this.TiempoAvisoConexion;
            entity.TiempoAvisoInhibicion = this.TiempoAvisoInhibicion;
            //entity.OperadorAdminID = this.OperadorAdminID;

            return entity;
        }

    }
}