using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace BugsMVC.Models
{
    public class UsuarioViewModel
    {
        public UsuarioViewModel()
        {
            Numero = 0;
            ClaveTerminal = 0;
            Inhibido = false;
            Efectivo = 0;
            CreditoZona1 = 0;
            CreditoZona2 = 0;
            CreditoZona3 = 0;
            CreditoZona4 = 0;
            CreditoZona5 = 0;
            MostrarDatosOpcionales = false;
            MostrarDatosMonetarios = false;
            MostrarCredencialesWeb = false;
            UltimaRecargaZona1 = DateTime.Now;
            UltimaRecargaZona2 = DateTime.Now;
            UltimaRecargaZona3 = DateTime.Now;
            UltimaRecargaZona4 = DateTime.Now;
            UltimaRecargaZona5 = DateTime.Now;
        }

        [Key]
        public Guid UsuarioID { get; set; }

        [StringLength(30)]
        public string Apellido { get; set; }

        [StringLength(20)]
        public string Nombre { get; set; }

        [StringLength(10)]
        public string Legajo { get; set; }

        [DisplayName("DNI")]
        [Range(0, 99999999)]
        public int? Dni { get; set; }

        [DisplayName("Número")]
        [Range(1, 99999999, ErrorMessage = "El número de Usuario debe estar entre 1 y 99999999")]
        [Required(ErrorMessage="Campo obligatorio")]
        public int Numero { get; set; }

        [DisplayName("Clave Terminal")]
        [Range(1, 9999, ErrorMessage = "La Clave Terminar debe estar entre 1 y 9999")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public int ClaveTerminal { get; set; }

        [DisplayName("Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [DisplayName("Inhibído")]
        public bool Inhibido { get; set; }

        [DisplayName("Fecha de Inhibición")]
        public DateTime? FechaInhibido { get; set; }

        public DateTime FechaCreacion { get; set; }

        [Editable(false)]
        public DateTime? UltimoUsoVT { get; set; }

        [Editable(false)]
        [DisplayName("Efectivo")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal Efectivo { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona5 { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public DateTime? UltimaRecargaZona1 { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public DateTime? UltimaRecargaZona2 { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public DateTime? UltimaRecargaZona3 { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public DateTime? UltimaRecargaZona4 { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public DateTime? UltimaRecargaZona5 { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public int? Clave { get; set; }

        [ScaffoldColumn(false)]
        [Editable(false)]
        public String Email { get; set; }

        public string Locacion_Description { get; set; }

        public string Jerarquia_Description { get; set; }

        public string OperadorNombre { get; set; }

        public bool MostrarDatosOpcionales { get; set; }
        public bool MostrarDatosMonetarios { get; set; }
        public bool MostrarCredencialesWeb { get; set; }

        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

        [DisplayName("Locación")]
        public Guid? LocacionID { get; set; }
        public virtual Locacion Locacion { get; set; }

        [DisplayName("Jerarquía")]
        public Guid? JerarquiaID { get; set; }
        public virtual Jerarquia Jerarquia { get; set; }

        public virtual RegisterViewModel ApplicationUser { get; set; }
        public string[] RolesSeleccionados { get; set; }
        
        [DisplayName("Cantidad de Consumidores")]
        public int? CantidadConsumidores { get; set; }

        public bool GenerarCredencialesWeb { get; set; }

        public string NombreZona1 { get; set; }
        public string NombreZona2 { get; set; }
        public string NombreZona3 { get; set; }
        public string NombreZona4 { get; set; }
        public string NombreZona5 { get; set; }

        public virtual ICollection<Transaccion> Transacciones { get; set; }
    }
}
