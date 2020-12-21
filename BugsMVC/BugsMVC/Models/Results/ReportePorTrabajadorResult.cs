using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.Results
{
    public class ReportePorTrabajadorResult
    {
        public Guid LocacionID { get; set; }
        public decimal ValorVenta { get; set; }
        public string NombreLocacion { get; set; }
        public string CUIT { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public string CodigoPostal { get; set; }
        public string Provincia { get; set; }
        public string NombreUsuario { get; set; }
        public string Apellido { get; set; }
        public int? Dni { get; set; }
        public int? Numero { get; set; }
        public string NombreArticulo { get; set; }
        public string TipoModelo { get; set; }
        public string Marca { get; set; }
        public string PoseeCertificacion { get; set; }

        public DateTime FechaTransaccion { get; set; }
        public int Cantidad { get; set; }
    }
}