using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ReporteCertificadoEntregaEppTrabajador
    {
        public ReporteCertificadoEntregaEppTrabajador()
        {
            Detalle = new List<ReporteCertificadoEntregaEppDetalle>();
        }

        public string RazonSocial { get; set; }

        public string CUIT { get; set; }

        public string Direccion { get; set; }

        public string Localidad { get; set; }

        public string CodigoPostal { get; set; }

        public string Provincia { get; set; }

        public string NombreCompleto { get; set; }

        public string DNI { get; set; }

        public int NumeroTrabajador { get; set; }

        public string DescripcionPuestoTrabajo { get; set; }

        public string ElementoProteccionNecesario { get; set; }

        public string Producto { get; set; }

        public string Periodo { get; set; }

        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public DateTime FechaConsulta { get; set; }

        public IEnumerable<ReporteCertificadoEntregaEppDetalle> Detalle { get; set; }

        public string InfoAdicional { get; set; }

        public string RevisionNro { get; set; }

        public string FechaRevision { get; set; }
    }

    public class ReporteCertificadoEntregaEppDetalle
    {
        public string Producto { get; set; }

        public string TipoModelo { get; set; }

        public string Marca { get; set; }

        public string PoseeCertificacion { get; set; }

        public string Cantidad { get; set; }        

        public string FechaEntrega { get; set; }

        public decimal PrecioEnMaquina { get; set; }

        public decimal Total { get; set; }

        public int Dia { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }

    }


}