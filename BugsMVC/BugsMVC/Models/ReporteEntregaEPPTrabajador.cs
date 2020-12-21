using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ReporteEntregaEPPTrabajador
    {
        public ReporteEntregaEPPTrabajador()
        {
            ReportePorTrabajador = new List<ReporteEntregaEPPTrabajadorLocacion>();
        }

        [DisplayName("Fecha Desde")]
        [Required(ErrorMessage="Campo obligatorio")]
        public DateTime FechaDesde { get; set; }

        [DisplayName("Fecha Hasta")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaHasta { get; set; }

        [DisplayName("Trabajadores")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string[] Trabajadores { get; set; }

        [DisplayName("Locaciones")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string[] Locaciones { get; set; }

        public List<ReporteEntregaEPPTrabajadorLocacion> ReportePorTrabajador { get; set; }
    }
}