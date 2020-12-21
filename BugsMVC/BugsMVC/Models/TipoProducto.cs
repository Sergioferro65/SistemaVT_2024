using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class TipoProducto
    {
        public TipoProducto()
        {
        }

        [Key]
        public Guid TipoProductoID { get; set; }

        [Required(ErrorMessage ="Campo Obligatorio")]
        [StringLength(200, ErrorMessage = "El tamaño máximo del texto debe ser 200 caracteres")]
        [DisplayName("Tipo Producto")]
        public string Nombre { get; set; }

        public virtual ICollection<Maquina> Maquinas { get; set; }
    }
}