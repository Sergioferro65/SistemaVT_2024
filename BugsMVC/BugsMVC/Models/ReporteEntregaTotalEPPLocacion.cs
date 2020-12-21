using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ReporteEntregaTotalEPPLocacion
    {
        public ReporteEntregaTotalEPPLocacion()
        {
            Detalle = new List<ReporteEntregaTotalEPPDetalle>();
        }

        public string RazonSocial { get; set; }

        public string CUIT { get; set; }

        public string Direccion { get; set; }

        public string Localidad { get; set; }

        public string CodigoPostal { get; set; }

        public string Provincia { get; set; }

        public string Periodo { get; set; }

        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public DateTime FechaConsulta { get; set; }

        public IEnumerable<ReporteEntregaTotalEPPDetalle> Detalle { get; set; }

        public decimal Total { get; set; } 
    }

    public class ReporteEntregaTotalEPPDetalle
    {
        public string Articulo { get; set; }

        public string CantidadTotal { get; set; }

        public decimal PrecioEnMaquina { get; set; }

        public decimal PrecioUnitarioReal { get; set; }

        public decimal Total { get; set; }

        public string Observaciones { get; set; }
    }
}