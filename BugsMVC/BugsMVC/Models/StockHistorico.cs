using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class StockHistorico
    {
        [Key]
        public Guid StockHistoricoID { get; set; }
        public Guid? StockID { get; set; }
        public Guid TipoDeMovimientoID { get; set; }
        public DateTime Fecha { get; set; }
        public Guid? UsuarioID { get; set; }
        public decimal Cantidad { get; set; }

        [Editable(false)]
        public DateTime? FechaAviso { get; set; }

        public virtual Stock Stock { get; set; }
        public virtual TipoDeMovimiento TipoDeMovimiento { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}