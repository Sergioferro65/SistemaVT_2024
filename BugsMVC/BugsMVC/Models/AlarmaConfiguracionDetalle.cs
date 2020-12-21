using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class AlarmaConfiguracionDetalle
    {
        public Guid AlarmaConfiguracionDetalleID { get; set; }
        public Guid AlarmaConfiguracionID { get; set; }
        public Guid UsuarioID { get; set; }

        public virtual AlarmaConfiguracion AlarmaConfiguracion { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}