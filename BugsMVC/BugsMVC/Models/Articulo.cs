using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Articulo
    {
        public Articulo()
        {
            CostoReal = 0;
        }

        [Key]
        [DisplayName("Artículo")]
        public Guid ArticuloID { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [StringLength(50)]
        [DisplayName("Artículo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Costo Real")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CostoReal { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public int UnidadMedida { get; set; }

        [StringLength(100)]
        public string Marca { get; set; }

        [StringLength(100)]
        public string Modelo { get; set; }

        [StringLength(15)]
        [DisplayName("Certificación")]
        public string Certificacion { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Operador")]
        public Guid OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

      //  public virtual ICollection<Maquina> Maquinas { get; set; }
        public virtual ICollection<Transaccion> Transacciones { get; set; }
        public virtual ICollection<ArticuloAsignacion> ArticulosAsignaciones { get; set; }
    }
}