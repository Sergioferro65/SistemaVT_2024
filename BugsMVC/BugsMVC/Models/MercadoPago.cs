using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class MercadoPagoTable
    {
        [Key]
        public int MercadoPagoId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public Guid MaquinaId { get; set; }
        public int MercadoPagoEstadoFinancieroId { get; set; }
        public int MercadoPagoEstadoTransmisionId { get; set; }
        public string Comprobante { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaModificacionEstadoTransmision { get; set; }

        public virtual Maquina Maquina { get; set; }
        public virtual MercadoPagoEstadoTransmision MercadoPagoEstadoTransmision { get; set; }
        public virtual MercadoPagoEstadoFinanciero MercadoPagoEstadoFinanciero { get; set; }
    }
}