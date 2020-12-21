using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class ConsumidorViewModel
    {
        public int CantidadTransacciones { get; set; }

        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoTotalTransacciones { get; set; }

        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal Efectivo { get; set; }

        public Locacion Locacion { get; set; }

        [DisplayName("Credito Zona 1")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona1 { get; set; }
        [DisplayName("Credito Zona 2")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona2 { get; set; }
        [DisplayName("Credito Zona 3")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona3 { get; set; }
        [DisplayName("Credito Zona 4")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona4 { get; set; }
        [DisplayName("Credito Zona 5")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CreditoZona5 { get; set; }

        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoRecargaZona1 { get; set; }
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoRecargaZona2 { get; set; }
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoRecargaZona3 { get; set; }
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoRecargaZona4 { get; set; }
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal MontoRecargaZona5 { get; set; }

        public static ConsumidorViewModel From(Usuario entity,IEnumerable<Transaccion> transacciones)
        {
            ConsumidorViewModel viewModel = new ConsumidorViewModel();
            viewModel.CreditoZona1 = entity.CreditoZona1;
            viewModel.CreditoZona2 = entity.CreditoZona2;
            viewModel.CreditoZona3 = entity.CreditoZona3;
            viewModel.CreditoZona4 = entity.CreditoZona4;
            viewModel.CreditoZona5 = entity.CreditoZona5;

            viewModel.MontoRecargaZona1 = entity.Jerarquia.RecargaZona1;
            viewModel.MontoRecargaZona2 = entity.Jerarquia.RecargaZona2;
            viewModel.MontoRecargaZona3 = entity.Jerarquia.RecargaZona3;
            viewModel.MontoRecargaZona4 = entity.Jerarquia.RecargaZona4;
            viewModel.MontoRecargaZona5 = entity.Jerarquia.RecargaZona5;

            viewModel.Efectivo = entity.Efectivo;
            viewModel.CantidadTransacciones = transacciones.Where(x=> x.FechaTransaccion.HasValue && x.FechaTransaccion.Value.Month == DateTime.Now.Month).Count();
            viewModel.MontoTotalTransacciones = transacciones.Where(x => x.FechaTransaccion.HasValue && x.FechaTransaccion.Value.Month == DateTime.Now.Month).Sum(x => x.ValorVenta);

            return viewModel;
        }

        public ConsumidorViewModel WithLocacion(Locacion locacion)
        {
            this.Locacion = locacion;
            return this;
        }

        public decimal CalcularPorcentaje(decimal total,decimal creditoZona) {      
            return total != (decimal)0 ? ( (creditoZona * 100 / total) > 100 ? 100 : (creditoZona * 100 / total)) : 0;
        }
    }
}