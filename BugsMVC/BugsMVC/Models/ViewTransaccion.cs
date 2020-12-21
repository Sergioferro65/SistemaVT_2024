using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ViewTransaccion
    {
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

        [DisplayName("Descuento")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        public decimal DescuentoAplicado { get; set; }

        [DisplayName("Usuario Service")]
        public bool UsuarioService { get; set; }

        [DisplayName("Transacción Texto")]
        public string TextoTransaccion { get; set; }

        [DisplayName("Artículo")]
        public string Articulo { get; set; }

        [DisplayName("Modelo Terminal")]
        public string ModeloTerminal { get; set; }

        [DisplayName("Terminal")]
        public int? Terminal { get; set; }

        [DisplayName("Máquina")]
        public int? Maquina { get; set; }

        [DisplayName("Locación")]
        public string Locacion { get; set; }

        [DisplayName("Operador")]
        public string Operador { get; set; }
        public Guid? OperadorID { get; set; }

        [DisplayName("Usuario")]
        public Guid? UsuarioID { get; set; }

        [DisplayName("Jerarquia")]
        public string Jerarquia { get; set; }

        public string UsuarioNombre { get; set; }

        public string UsuarioApellido { get; set; }

        [DisplayName("Nombre Operador")]
        public string OperadorNombre { get; set; }

        [DisplayName("Valor Recorte")]
        public decimal? ValorRecorte { get; set; }

        [DisplayName("Nombre Zona1")]
        public string NombreZona1 { get; set; }
        [DisplayName("Nombre Zona2")]
        public string NombreZona2 { get; set; }
        [DisplayName("Nombre Zona3")]
        public string NombreZona3 { get; set; }
        [DisplayName("Nombre Zona4")]
        public string NombreZona4 { get; set; }
        [DisplayName("Nombre Zona5")]
        public string NombreZona5 { get; set; }

        [DisplayName("Número Usuario")]
        public int? NumeroUsuario { get; set; }
    }
}