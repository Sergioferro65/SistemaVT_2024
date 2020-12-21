using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ArticuloAsignacion
    {
        public ArticuloAsignacion()
        {
            AlarmaActiva = false;
        }

        [Key]
        public Guid Id { get; set; }

        [DisplayName("Artículo")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid ArticuloID { get; set; }

        [DisplayName("Locación")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid LocacionID { get; set; }

        [DisplayName("Zona")]
        public int? NroZona { get; set; }

        [DisplayName("Máquina")]
        public Guid? MaquinaID { get; set; }

        [DisplayName("Precio")]
        [Required(ErrorMessage = "Campo obligatorio")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal Precio { get; set; }

        [DisplayName("Alarma Bajo")]
        public int? AlarmaBajo { get; set; }

        [DisplayName("Alarma Muy Bajo")]
        public int? AlarmaMuyBajo { get; set; }

        [DisplayName("Capacidad")]
        public int? Capacidad { get; set; }

        [DisplayName("Alarma Activa")]
        [UIHint("Checkbox")]
        public bool? AlarmaActiva { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Control de Stock")]
        public bool ControlStock { get; set; }

        [NotMapped]
        public bool ControlStockInicio { get; set; }

        public virtual ICollection<Stock> Stocks { get; set; }

        public virtual Locacion Locacion { get; set; }
        public virtual Maquina Maquina { get; set; }
        public virtual Articulo Articulo { get; set; }
    }
}