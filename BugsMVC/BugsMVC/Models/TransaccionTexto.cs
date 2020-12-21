using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class TransaccionTexto
    {
        public TransaccionTexto()
        {
            SumaEnVentas = false;
            SumaEnRecargas = false;
        }

        [Key]
        public Guid TransaccionTextoID { get; set; }

        [DisplayName("Código Transacción")]
        [StringLength(3, ErrorMessage = "El {0} debe tener como máximo {1} caracteres.")]
        public string CodigoTransaccion { get; set; }

        [DisplayName("Suma En Ventas")]
        public bool SumaEnVentas { get; set; }

        [DisplayName("Suma En Recargas")]
        public bool SumaEnRecargas { get; set; }

        [DisplayName("Suma En Efectivo")]
        public bool SumaEnEfectivo { get; set; }

        [DisplayName("Tipo Transacción")]
        [StringLength(100, ErrorMessage = "El {0} debe tener como máximo {1} caracteres.")]
        public string TextoTransaccion { get; set; }

        [DisplayName("Modelo Terminal")]
        public Guid ModeloTerminalID { get; set; }
        public ModeloTerminal ModeloTerminal { get; set; }

        public virtual ICollection<Transaccion> Transacciones { get; set; }
    }
}