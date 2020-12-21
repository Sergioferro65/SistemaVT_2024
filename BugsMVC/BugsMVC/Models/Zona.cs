using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Zona
    {
        [Key]
        public Guid ZonaID { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        public int Valor { get; set; }

        [DisplayName("Última Recarga")]
        public DateTime? UltimaRecarga { get; set; }

        [DisplayName("Jerarquía")]
        public Guid JerarquiaID { get; set; }
        public virtual Jerarquia Jerarquia { get; set; }

        //public virtual ICollection<Maquina> Maquinas { get; set; }
        //public virtual ICollection<ArticuloAsignacion> ArticulosAsignaciones { get; set; }
    }
}