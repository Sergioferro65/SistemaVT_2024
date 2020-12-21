using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class TipoDeAlarma
    {
        [Key]
        public int TipoDeAlarmaID { get; set; }
        
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        public virtual ICollection<AlarmaConfiguracion> AlarmaConfiguraciones { get; set; }

        public const int IdControlStock = 1;
        public const int IdControlEstadoMaquina = 2;

        //public enum Types
        //{
        //    ControlStock = 1,
        //    FueraServicio
        //}
    }
}