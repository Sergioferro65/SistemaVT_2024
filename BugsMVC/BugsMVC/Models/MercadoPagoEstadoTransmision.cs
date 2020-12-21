using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class MercadoPagoEstadoTransmision
    {
        [Key]
        public int Id { get; set; }
        public string Descripcion { get; set; }

        public enum States
        {
            EN_PROCESO = 1,
            TERMINADO_OK,
            TERMINADO_MAL,
            ERROR_CONEXION
        }

        public virtual ICollection<MercadoPagoTable> MercadoPagos { get; set; }
    }
}