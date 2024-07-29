using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Maquina
    {
        public Maquina()
        {
            Zona = 0;
            ContadorVentasParcial = 0;
            MontoVentasParcial = 0;
            ContadorVentasHistorico = 0;
            MontoVentasHistorico = 0;
            ValorVenta = 0;
            Decimales = 0;
            FactorEscala = 1;
            TiempoSesion = 100;
            CreditoMaximoCash = 0;
            ValorChannelA = 0;
            ValorChannelB = 0;
            ValorChannelC = 0;
            ValorChannelD = 0;
            ValorChannelE = 0;
            ValorChannelF = 0;
            ValorBillete1 = 0;
            ValorBillete2 = 0;
            ValorBillete3 = 0;
            ValorBillete4 = 0;
            ValorBillete5 = 0;
            ValorBillete6 = 0;



            AlarmaActiva = false;
            MostrarDatosOpcionales = false;
            MostrarPanelDescuentos = false;
        }

        [Key]
        [DisplayName("Maquina")]
        public Guid MaquinaID { get; set; }

        [NotMapped]
        public bool MostrarDatosOpcionales { get; set; }
        [NotMapped]
        public bool MostrarPanelDescuentos { get; set; }

        [NotMapped]
        [DisplayName("Asignar Máquina")]
        public bool CheckAsignarMaquina { get; set; }

        [DisplayName("Fecha Aviso")]
        public DateTime? FechaAviso { get; set; }

        [DisplayName("Fecha Estado")]
        public DateTime? FechaEstado { get; set; }

        [DisplayName("Alarma Activa")]
        [UIHint("Checkbox")]
        public bool? AlarmaActiva { get; set; }

        //Del uno al 10, cargar con las zonas del cliente
        public int? Zona { get; set; }

        [DisplayName("N° Serie Máquina")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        [StringLength(20)]
        public string NumeroSerie { get; set; }

        [DisplayName("Nombre Alias")]
        [StringLength(20)]
        public string NombreAlias { get; set; }

        [StringLength(40)]
        [DisplayName("Ubicación")]
        public string Ubicacion { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(20)]
        public string Estado { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(20)]
        [DisplayName("Estado Conexión")]
        public string EstadoConexion { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(16)]
        public string Mensaje { get; set; }

        [ScaffoldColumn(false)]
        [DisplayName("Notas Service")]
        [StringLength(200)]
        public string NotasService { get; set; }

        [DisplayName("Contador Ventas Parcial")]
        [ScaffoldColumn(false)]
        public int ContadorVentasParcial { get; set; }

        [DisplayName("Monto Ventas Parcial")]
        [ScaffoldColumn(false)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoVentasParcial { get; set; }

        [DisplayName("Contador Ventas Histórico")]
        [ScaffoldColumn(false)]
        public int ContadorVentasHistorico { get; set; }

        [DisplayName("Monto Ventas Histórico")]
        [ScaffoldColumn(false)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoVentasHistorico { get; set; }

        [DisplayName("Fecha Último Service")]
        [ScaffoldColumn(false)]
        public DateTime? FechaUltimoService { get; set; }

        [DisplayName("Fecha Última Recaudación")]
        [ScaffoldColumn(false)]
        public DateTime? FechaUltimaRecaudacion { get; set; }

        [DisplayName("Fecha Última Reposición")]
        [ScaffoldColumn(false)]
        public DateTime? FechaUltimaReposicion { get; set; }

        [DisplayName("Fecha Última Reposición")]
        [ScaffoldColumn(false)]
        public DateTime? FechaUltimoOk { get; set; }

        [DisplayName("Fecha Última Reposición")]
        [ScaffoldColumn(false)]
        public DateTime? FechaUltimaConexion { get; set; }

        [DisplayName("Efectivo Recaudado")]
        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal TotalRecaudado { get; set; }

        [DisplayName("Solo Venta Efectivo")]
        public bool SoloVentaEfectivo { get; set; }

        [DisplayName("Valor Venta")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorVenta { get; set; }

        //Dos o cero (mostrar en combo)
        public int Decimales { get; set; }

        //combo: 1, 5, 10, 255
        [DisplayName("Factor Escala")]
        public int FactorEscala { get; set; }

        [DisplayName("Tiempo de Sesión")]
        public int TiempoSesion { get; set; }

        [DisplayName("Crédito Máximo Cash")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoMaximoCash { get; set; }

        [DisplayName("Valor Channel A")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorChannelA { get; set; }

        [DisplayName("Valor Channel B")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorChannelB { get; set; }

        [DisplayName("Valor Channel C")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorChannelC { get; set; }

        [DisplayName("Valor Channel D")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorChannelD { get; set; }

        [DisplayName("Valor Channel E")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorChannelE { get; set; }

        [DisplayName("Valor Channel F")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorChannelF { get; set; } 

        [DisplayName("Valor Billete 1")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorBillete1 { get; set; }

        [DisplayName("Valor Billete 2")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorBillete2 { get; set; }

        [DisplayName("Valor Billete 3")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorBillete3 { get; set; }

        [DisplayName("Valor Billete 4")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorBillete4 { get; set; }

        [DisplayName("Valor Billete 5")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorBillete5 { get; set; }

        [DisplayName("Valor Billete 6")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal ValorBillete6 { get; set; }

        [DisplayName("Precio Venta 1")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio1 { get; set; }

        [DisplayName("Precio Venta 2")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio2 { get; set; }

        [DisplayName("Precio Venta 3")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio3 { get; set; }

        [DisplayName("Precio Venta 4")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio4 { get; set; }

        [DisplayName("Precio Venta 5")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio5 { get; set; }


        [DisplayName("Precio Venta 6")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio6 { get; set; }


        [DisplayName("Precio Venta 7")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio7 { get; set; }


        [DisplayName("Precio Venta 8")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio8 { get; set; }


        [DisplayName("Precio Venta 9")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio9 { get; set; }


        [DisplayName("Precio Venta 10")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoPrecio10 { get; set; }



        [DisplayName("Valor Descuento 1")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor1 { get; set; }

        [DisplayName("Valor Descuento 2")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor2 { get; set; }

        [DisplayName("Valor Descuento 3")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor3 { get; set; }

        [DisplayName("Valor Descuento 4")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor4 { get; set; }

        [DisplayName("Valor Descuento 5")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor5 { get; set; }


        [DisplayName("Valor Descuento 6")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor6 { get; set; }

        [DisplayName("Valor Descuento 7")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor7 { get; set; }

        [DisplayName("Valor Descuento 8")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor8 { get; set; }

        [DisplayName("Valor Descuento 9")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor9 { get; set; }

        [DisplayName("Valor Descuento 10")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? DescuentoValor10 { get; set; }


        [DisplayName("Descuento Porcentual")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [Range(0, 100, ErrorMessage = "El valor ingresado debe estar entre 0 y 100")]
        public decimal? DescuentoPorcentual { get; set; }

        [DisplayName("Locación")]
        public Guid? LocacionID { get; set; }
        public virtual Locacion Locacion { get; set; }

        [DisplayName("Marca Modelo")]
        [Required(ErrorMessage ="Campo Obligatorio")]
        public Guid MarcaModeloID { get; set; }
        public virtual MarcaModelo MarcaModelo { get; set; }

        [DisplayName("Terminal")]
        public Guid? TerminalID { get; set; }
        public virtual Terminal Terminal { get; set; }

        [Required]
        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

        [DisplayName("Tipo Producto")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public Guid TipoProductoID { get; set; }
        public virtual TipoProducto TipoProducto { get; set; }

        public virtual ICollection<ArticuloAsignacion> ArticulosAsignaciones { get; set; }
        public virtual ICollection<MercadoPagoTable> MercadoPagos { get; set; }
        public virtual ICollection<TransaccionesMal> TransaccionesMal { get; set; }

        public string getDescripcionMaquina()
        {
            return NombreAlias != null ? MarcaModelo.MarcaModeloNombre + " - " + NumeroSerie + '(' + NombreAlias + ')' : MarcaModelo.MarcaModeloNombre + '-' + NumeroSerie;
        }

        [ScaffoldColumn(false)]
        [DisplayName("Precio Venta/Valor Descuento")]
        public string PrecioVentaValorDescuento
        {
            get
            {
                return ("Precio venta 1: $" + (DescuentoPrecio1.HasValue ? DescuentoPrecio1.ToString() : "0") + ", Valor descuento 1: $" + (DescuentoValor1.HasValue ? DescuentoValor1.ToString() : "0") + Environment.NewLine +
                "Precio venta 2: $" + (DescuentoPrecio2.HasValue ? DescuentoPrecio2.ToString() : "0") + ", Valor descuento 2: $" + (DescuentoValor2.HasValue ? DescuentoValor2.ToString() : "0") + Environment.NewLine +
                "Precio venta 3: $" + (DescuentoPrecio3.HasValue ? DescuentoPrecio3.ToString() : "0") + ", Valor descuento 3: $" + (DescuentoValor3.HasValue ? DescuentoValor3.ToString() : "0") + Environment.NewLine +
                "Precio venta 4: $" + (DescuentoPrecio4.HasValue ? DescuentoPrecio4.ToString() : "0") + ", Valor descuento 4: $" + (DescuentoValor4.HasValue ? DescuentoValor4.ToString() : "0") + Environment.NewLine +
                "Precio venta 5: $" + (DescuentoPrecio5.HasValue ? DescuentoPrecio5.ToString() : "0") + ", Valor descuento 5: $" + (DescuentoValor5.HasValue ? DescuentoValor5.ToString() : "0") + Environment.NewLine +
                "Precio venta 6: $" + (DescuentoPrecio6.HasValue ? DescuentoPrecio6.ToString() : "0") + ", Valor descuento 6: $" + (DescuentoValor6.HasValue ? DescuentoValor6.ToString() : "0") + Environment.NewLine +
                "Precio venta 7: $" + (DescuentoPrecio7.HasValue ? DescuentoPrecio7.ToString() : "0") + ", Valor descuento 7: $" + (DescuentoValor7.HasValue ? DescuentoValor7.ToString() : "0") + Environment.NewLine +
                "Precio venta 8: $" + (DescuentoPrecio8.HasValue ? DescuentoPrecio8.ToString() : "0") + ", Valor descuento 8: $" + (DescuentoValor8.HasValue ? DescuentoValor8.ToString() : "0") + Environment.NewLine +
                "Precio venta 9: $" + (DescuentoPrecio9.HasValue ? DescuentoPrecio9.ToString() : "0") + ", Valor descuento 9: $" + (DescuentoValor9.HasValue ? DescuentoValor9.ToString() : "0") + Environment.NewLine +
                "Precio venta 10: $" + (DescuentoPrecio10.HasValue ? DescuentoPrecio10.ToString() : "0") + ", Valor descuento 10: $" + (DescuentoValor10.HasValue ? DescuentoValor10.ToString() : "0"));

            }
        }
        public void DesasignarMaquina()
        {
            LocacionID = null;
            Zona = 0;
            Ubicacion = string.Empty;
            NombreAlias = string.Empty;
            TerminalID = null;

            Estado = "Creada";
            Mensaje = string.Empty;
            NotasService = string.Empty;
            SoloVentaEfectivo = false;
            ValorVenta = 0;
            TiempoSesion = 100;
            FactorEscala = 1;
            Decimales = 0;

            DescuentoPorcentual = null;
            DescuentoPrecio1 = null;
            DescuentoPrecio2 = null;
            DescuentoPrecio3 = null;
            DescuentoPrecio4 = null;
            DescuentoPrecio5 = null;
            DescuentoPrecio6 = null;
            DescuentoPrecio7 = null;
            DescuentoPrecio8 = null;
            DescuentoPrecio9 = null;
            DescuentoPrecio10 = null;
            DescuentoValor1 = null;
            DescuentoValor2 = null;
            DescuentoValor3 = null;
            DescuentoValor4 = null;
            DescuentoValor5 = null;
            DescuentoValor6 = null;
            DescuentoValor7 = null;
            DescuentoValor8 = null;
            DescuentoValor9 = null;
            DescuentoValor10 = null;

            ContadorVentasParcial = 0;
            MontoVentasParcial = 0;
            ContadorVentasHistorico = 0;
            MontoVentasHistorico = 0;
            CreditoMaximoCash = 0;
            ValorChannelA = 0;
            ValorChannelB = 0;
            ValorChannelC = 0;
            ValorChannelD = 0;
            ValorChannelE = 0;
            ValorChannelF = 0;
            ValorBillete1 = 0;
            ValorBillete2 = 0;
            ValorBillete3 = 0;
            ValorBillete4 = 0;
            ValorBillete5 = 0;
            ValorBillete6 = 0;
            AlarmaActiva = false;
            CheckAsignarMaquina = false;
            EstadoConexion = "";
        }
    }
}