using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class AlarmaConfiguracion
    {
        public Guid AlarmaConfiguracionID { get; set; }
        public int TipoDeAlarmaID { get; set; }
        public Guid? LocacionID { get; set; }
        public Guid OperadorID { get; set; }

        public virtual TipoDeAlarma TipoDeAlarma { get; set; }
        public virtual Locacion Locacion { get; set; }
        public virtual Operador Operador { get; set; }

        public virtual ICollection<AlarmaConfiguracionDetalle> AlarmaConfiguracionDetalles { get; set; }

    }
}