using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Jerarquia
    {
        public Jerarquia()
        {
            RecargaZona1 = 0;
            RecargaZona2 = 0;
            RecargaZona3 = 0;
            RecargaZona4 = 0;
            RecargaZona5 = 0;
            DescuentoPorcentualZona1 = 0;
            DescuentoPorcentualZona2 = 0;
            DescuentoPorcentualZona3 = 0;
            DescuentoPorcentualZona4 = 0;
            DescuentoPorcentualZona5 = 0;
            MontoRecorteZona1 = 0;
            MontoRecorteZona2 = 0;
            MontoRecorteZona3 = 0;
            MontoRecorteZona4 = 0;
            MontoRecorteZona5 = 0;
            PeriodoRecargaZona1 = 0;
            PeriodoRecargaZona2 = 0;
            PeriodoRecargaZona3 = 0;
            PeriodoRecargaZona4 = 0;
            PeriodoRecargaZona5 = 0;
        }

        [Key]
        [DisplayName("Jerarquía")]
        public Guid JerarquiaID { get; set; }

        [Required(ErrorMessage="Campo obligatorio")]
        [DisplayName("Nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        public decimal RecargaZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        public decimal RecargaZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        public decimal RecargaZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        public decimal RecargaZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        public decimal RecargaZona5 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        public decimal DescuentoPorcentualZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        public decimal DescuentoPorcentualZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        public decimal DescuentoPorcentualZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        public decimal DescuentoPorcentualZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        public decimal DescuentoPorcentualZona5 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        public decimal MontoRecorteZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        public decimal MontoRecorteZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        public decimal MontoRecorteZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        public decimal MontoRecorteZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        public decimal MontoRecorteZona5 { get; set; }

        [DisplayName("Período recarga")]
        public int PeriodoRecargaZona1 { get; set; }

        [DisplayName("Período recarga")]
        public int PeriodoRecargaZona2 { get; set; }

        [DisplayName("Período recarga")]
        public int PeriodoRecargaZona3 { get; set; }

        [DisplayName("Período recarga")]
        public int PeriodoRecargaZona4 { get; set; }

        [DisplayName("Período recarga")]
        public int PeriodoRecargaZona5 { get; set; }

        [DisplayName("Locación")]
        [Required(ErrorMessage="Campo obligatorio")]
        public Guid LocacionID { get; set; }
        public virtual Locacion Locacion { get; set; }

        public enum PeriodosRecarga
        {
            Ninguno,
            Diario,
            Semanal,
            Mensual
        }

        public virtual ICollection<Zona> Zonas { get; set; }
        public virtual ICollection<Usuario> Consumidores { get; set; }
    }
}