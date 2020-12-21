using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ReporteEntregaEPPTrabajadorLocacion
    {
        public ReporteEntregaEPPTrabajadorLocacion()
        {
            Detalle = new List<ReporteEntregaEPPTrabajadorDetalle>();
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

        public IEnumerable<ReporteEntregaEPPTrabajadorDetalle> Detalle { get; set; }

        public decimal Total { get; set; }
    }

    public class ReporteEntregaEPPTrabajadorDetalle
    {
        private Dictionary<string, int> cantidadesDictionary;

        public string Nombre { get; set; }

        public string Legajo { get; set; }

        public IEnumerable<ArticuloCantidad> Cantidades { get; set; }

        public Dictionary<string, int> CantidadesDictionary
        {
            get
            {
                if (cantidadesDictionary == null)
                {
                    cantidadesDictionary = Cantidades.ToDictionary(d => d.Nombre, d => d.Cantidad);
                }
                return cantidadesDictionary;
            }
        }
    }

    public class ArticuloCantidad
    {
        public string Nombre { get; set; }

        public int Cantidad { get; set; }

        public decimal ValorVenta { get; set; }
    }

}