using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ReporteCertificadoEntregaEpp
    {
        public ReporteCertificadoEntregaEpp()
        {
            ReportePorTrabajador = new List<ReporteCertificadoEntregaEppTrabajador>();
        }

        [DisplayName("Fecha Desde")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaDesde { get; set; }

        [DisplayName("Fecha Hasta")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaHasta { get; set; }

        [DisplayName("Trabajadores")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string[] Usuarios { get; set; }

        [DisplayName("Locaciones")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string[] Locaciones { get; set; }

        public bool ObtenerTodos { get; set; }
        public bool ObtenerTodosLocaciones { get; set; }

        public List<ReporteCertificadoEntregaEppTrabajador> ReportePorTrabajador { get; set; }

    }
}