using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class TipoDeMovimiento
    {
        [Key]
        public Guid TipoDeMovimientoID { get; set; }

        public string Nombre { get; set; }

        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        public virtual ICollection<StockHistorico> StocksHistoricos { get; set; }
    }
}