using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ReporteEntregaTotalEPP
    {
        public ReporteEntregaTotalEPP()
        {
            ReportePorLocacion = new List<ReporteEntregaTotalEPPLocacion>();
        }

        [DisplayName("Fecha Desde")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaDesde { get; set; }

        [DisplayName("Fecha Hasta")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaHasta { get; set; }

        [DisplayName("Artículos")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string[] Articulos { get; set; }

        [DisplayName("Locaciones")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string[] Locaciones { get; set; }

        public bool ObtenerTodos { get; set; }
        public bool ObtenerTodosLocaciones { get; set; }
        public bool IncluirSinAlta { get { return Articulos != null && Articulos.Contains("SinAlta"); } }

        public List<ReporteEntregaTotalEPPLocacion> ReportePorLocacion { get; set; }
    }
}