using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Stock
    {
        public Stock()
        {
            Cantidad = 0;
        }

        [Key]
        public Guid StockID { get; set; }

        [DisplayName("Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        public Guid ArticuloAsignacionID { get; set; }

        [DisplayName("Fecha Aviso")]
        public DateTime? FechaAviso { get; set; }

        [DisplayName("Fecha Edición Web")]
        public DateTime? FechaEdicionWeb { get; set; }

        public Guid? UsuarioIDEdicionWeb { get; set; }

        [DisplayName("Fecha Edición VT")]
        public DateTime? FechaEdicionVT { get; set; }

        public virtual ArticuloAsignacion ArticuloAsignacion { get; set; }
        public virtual Usuario UsuarioEdicionWeb { get; set; }

        public virtual ICollection<StockHistorico> StocksHistoricos { get; set; }
    }
}