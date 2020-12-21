using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class MercadoPagoEstadoFinanciero
    {
        [Key]
        public int Id { get; set; }
        public string Descripcion { get; set; }

        public enum States
        {
            DEVUELTO = 1,
            ACREDITADO
        }


        public virtual ICollection<MercadoPagoTable> MercadoPagos { get; set; }
    }
}