using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Usuario
    {
        public Usuario()
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

        [StringLength(30)]
        public string Nombre { get; set; }

        [StringLength(10)]
        public string Legajo { get; set; }

        [DisplayName("DNI")]
        [Range(0, 99999999)]
        public int? Dni { get; set; }

        [DisplayName("Número")]
        [Range(0, 99999999)]
        [Required]
        public int Numero { get; set; }

        [DisplayName("Clave Terminal")]
        [Range(0, 9999)]
        [Required]
        public int ClaveTerminal { get; set; }

        [DisplayName("Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [DisplayName("Fecha de Creacion")]
        public DateTime FechaCreacion { get; set; }

        [DisplayName("Inhibído")]
        public bool Inhibido { get; set; }

        [DisplayName("Fecha de Inhibición")]
        public DateTime? FechaInhibido { get; set; }

        [DisplayName("Último Uso VT")]
        [Editable(false)]
        public DateTime? UltimoUsoVT { get; set; }

        [Editable(false)]
        [DisplayName("Efectivo")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage="Campo obligatorio",AllowEmptyStrings=true)]
        public decimal Efectivo { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
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

        public int Recarga_Recargado { get; set; }
        public int Recarga_Recortado { get; set; }
        
        public decimal? Recarga_CreditoInicialZona1 { get; set; }
        public decimal? Recarga_CreditoInicialZona2 { get; set; }
        public decimal? Recarga_CreditoInicialZona3 { get; set; }
        public decimal? Recarga_CreditoInicialZona4 { get; set; }
        public decimal? Recarga_CreditoInicialZona5 { get; set; }

        public decimal? Recarga_CreditoIntermedioZona1 { get; set; }
        public decimal? Recarga_CreditoIntermedioZona2 { get; set; }
        public decimal? Recarga_CreditoIntermedioZona3 { get; set; }
        public decimal? Recarga_CreditoIntermedioZona4 { get; set; }
        public decimal? Recarga_CreditoIntermedioZona5 { get; set; }

        public decimal? Recarga_CreditoFinalZona1 { get; set; }
        public decimal? Recarga_CreditoFinalZona2 { get; set; }
        public decimal? Recarga_CreditoFinalZona3 { get; set; }
        public decimal? Recarga_CreditoFinalZona4 { get; set; }
        public decimal? Recarga_CreditoFinalZona5 { get; set; }

        public decimal? Recarga_EfectivoInicial { get; set; }
        public decimal? Recarga_EfectivoFinal { get; set; }
        public decimal Recarga_RecargaTotalTeorica { get; set; }

        
        [DisplayName("Es Servicio Técnico")]
        public bool? EsServicioTecnico { get; set; }

        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

        [DisplayName("Locación")]
        public Guid? LocacionID { get; set; }
        public virtual Locacion Locacion { get; set; }

        [DisplayName("Jerarquia")]
        public Guid? JerarquiaID { get; set; }
        public virtual Jerarquia Jerarquia { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public virtual ICollection<StockHistorico> StocksHistoricos { get; set; }
        public virtual ICollection<Transaccion> Transacciones { get; set; }
        public virtual ICollection<AlarmaConfiguracionDetalle> AlarmaConfiguracionDetalles { get; set; }
    }
}