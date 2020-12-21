using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace BugsMVC.Models
{
    public class Transaccion
    {
        public Transaccion()
        {
            EfectivoInicial = 0;
            EfectivoFinal = 0;
            CreditoInicialZona1 = 0;
            CreditoFinalZona1 = 0;
            CreditoInicialZona2 = 0;
            CreditoFinalZona2 = 0;
            CreditoInicialZona3 = 0;
            CreditoFinalZona3 = 0;
            CreditoInicialZona4 = 0;
            CreditoFinalZona4 = 0;
            CreditoInicialZona5 = 0;
            CreditoFinalZona5 = 0;
            ValorVenta = 0;        
            ValorRecarga = 0;
            DescuentoAplicado = 0;
        }

        [Key]
        [DisplayName("Transacción")]
        public Guid TransaccionID { get; set; }

        [DisplayName("Fecha Alta Base")]
        public DateTime? FechaAltaBase { get; set; }

        [DisplayName("Fecha Transacción")]
        public DateTime? FechaTransaccion { get; set; }

        [StringLength(3)]
        [DisplayName("Código Transaccion")]
        public string CodigoTransaccion { get; set; }

        [DisplayName("Efectivo Inicial")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal EfectivoInicial { get; set; }

        [DisplayName("Efectivo Final")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal EfectivoFinal { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoInicialZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoFinalZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoInicialZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoFinalZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoInicialZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoFinalZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoInicialZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoFinalZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoInicialZona5 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal CreditoFinalZona5 { get; set; }

        [DisplayName("Valor Venta")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal ValorVenta { get; set; }

        [DisplayName("Valor Recarga")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal ValorRecarga { get; set; }

        [DisplayName("Descuento Aplicado")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal DescuentoAplicado { get; set; }

        [DisplayName("Usuario Service")]
        public bool UsuarioService { get; set; }

        [StringLength(30)]
        [DisplayName("Transacción IDVT")]
        public string TransaccionOriginal { get; set; }

        [DisplayName("Valor Recorte")]
        public decimal? ValorRecorte { get; set; }

        [DisplayName("Tipo Transacción")]
        public Guid TransaccionTextoID { get; set; }
        public virtual TransaccionTexto TransaccionTexto { get; set; }

        [DisplayName("Artículo")]
        public Guid? ArticuloID { get; set; }
        public virtual Articulo Articulo { get; set; }

        [DisplayName("Modelo Terminal")]
        public Guid? ModeloTerminalID { get; set; }
        public virtual ModeloTerminal ModeloTerminal { get; set; }

        [DisplayName("Terminal")]
        public Guid? TerminalID { get; set; }
        public virtual Terminal Terminal { get; set; }

        [DisplayName("Máquina")]
        public Guid? MaquinaID { get; set; }
        public virtual Maquina Maquina { get; set; }

        [DisplayName("Locación")]
        public Guid? LocacionID { get; set; }
        public virtual Locacion Locacion { get; set; }

        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

        [DisplayName("Usuario")]
        public Guid? UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }

        [DisplayName("Jerarquia")]
        public Guid? JerarquiaID { get; set; }
        public virtual Jerarquia Jerarquia { get; set; }
    }
}