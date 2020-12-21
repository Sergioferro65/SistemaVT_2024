using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.Results
{
    public class ReportePorLocacionResult
    {
        public Guid LocacionID { get; set; }
        public string NombreLocacion { get; set; }
        public string CUIT { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public string CodigoPostal { get; set; }
        public string Provincia { get; set; }

        public string NombreArticulo { get; set; }

        public DateTime FechaTransaccion { get; set; }
        public decimal ValorVenta { get; set; }
        public decimal? CostoReal { get; set; }


        public int Cantidad { get; set; }
    }
}