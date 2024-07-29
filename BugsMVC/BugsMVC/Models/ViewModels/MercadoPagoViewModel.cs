using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;

namespace BugsMVC.Models.ViewModels
{
    public class MercadoPagoViewModel
    {
        public int MercadoPagoId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public Guid MaquinaId { get; set; }
        public string MaquinaDescripcion { get; set; }
        public int MercadoPagoEstadoFinancieroId { get; set; }
        public int MercadoPagoEstadoTransmisionId { get; set; }
        public string MercadoPagoEstadoFinancieroDescripcion { get; set; }
        public string MercadoPagoEstadoTransmisionDescripcion { get; set; }        
        public bool MostrarDevolverDinero { get; set; }
        public string Comprobante { get; set; }
        public string OperadorNombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaModificacionEstadoTransmision { get; set; }
        public string Entidad { get; set; }

        public string UrlDevolucion { get; set; }

        public string NroSerieTerminal { get; set; }

        public string Locacion { get; set; }

        public string NotaService { get; set; }

        public static MercadoPagoViewModel From(MercadoPagoTable entity)
        {
            MercadoPagoViewModel viewModel = new MercadoPagoViewModel();
            viewModel.MercadoPagoId = entity.MercadoPagoId;
            viewModel.Fecha = entity.Fecha;
            viewModel.Monto = entity.Monto;
            viewModel.MaquinaId = entity.MaquinaId;
            viewModel.MercadoPagoEstadoFinancieroId = entity.MercadoPagoEstadoFinancieroId;
            viewModel.MercadoPagoEstadoTransmisionId = entity.MercadoPagoEstadoTransmisionId;
            viewModel.Comprobante = entity.Comprobante;
            viewModel.MercadoPagoEstadoFinancieroDescripcion = entity.MercadoPagoEstadoFinanciero.Descripcion;
            viewModel.MercadoPagoEstadoTransmisionDescripcion = entity.MercadoPagoEstadoTransmision.Descripcion;
            viewModel.Descripcion = entity.Descripcion;
            viewModel.FechaModificacionEstadoTransmision = entity.FechaModificacionEstadoTransmision;
            viewModel.MaquinaDescripcion = entity.Maquina.getDescripcionMaquina();
            viewModel.MostrarDevolverDinero = entity.MercadoPagoEstadoFinancieroId == (int)MercadoPagoEstadoFinanciero.States.ACREDITADO ||
                                              entity.MercadoPagoEstadoTransmisionId == (int)MercadoPagoEstadoTransmision.States.TERMINADO_MAL;
            viewModel.OperadorNombre = entity.Maquina.Operador.Nombre;
            viewModel.Entidad = entity.Entidad;
            viewModel.UrlDevolucion = entity.UrlDevolucion;

            return viewModel;
        }

        public static MercadoPagoViewModel From(MercadoPagoTable entity, string nroserieterminal, string locacion)
        {
            MercadoPagoViewModel viewModel = new MercadoPagoViewModel();
            viewModel.MercadoPagoId = entity.MercadoPagoId;
            viewModel.Fecha = entity.Fecha;
            viewModel.Monto = entity.Monto;
            viewModel.MaquinaId = entity.MaquinaId;
            viewModel.MercadoPagoEstadoFinancieroId = entity.MercadoPagoEstadoFinancieroId;
            viewModel.MercadoPagoEstadoTransmisionId = entity.MercadoPagoEstadoTransmisionId;
            viewModel.Comprobante = entity.Comprobante;
            viewModel.MercadoPagoEstadoFinancieroDescripcion = entity.MercadoPagoEstadoFinanciero.Descripcion;
            viewModel.MercadoPagoEstadoTransmisionDescripcion = entity.MercadoPagoEstadoTransmision.Descripcion;
            viewModel.Descripcion = entity.Descripcion;
            viewModel.FechaModificacionEstadoTransmision = entity.FechaModificacionEstadoTransmision;
            viewModel.MaquinaDescripcion = entity.Maquina.NumeroSerie;// entity.Maquina.getDescripcionMaquina();
            viewModel.MostrarDevolverDinero = entity.MercadoPagoEstadoFinancieroId == (int)MercadoPagoEstadoFinanciero.States.ACREDITADO ||
                                              entity.MercadoPagoEstadoTransmisionId == (int)MercadoPagoEstadoTransmision.States.TERMINADO_MAL;
            viewModel.OperadorNombre = entity.Maquina.Operador.Nombre;
            viewModel.Entidad = entity.Entidad;
            viewModel.UrlDevolucion = entity.UrlDevolucion;
            viewModel.NroSerieTerminal = nroserieterminal;
            viewModel.Locacion = locacion;
            viewModel.NotaService = entity.Maquina.NotasService;

            return viewModel;
        }



        public void ToEntity(MercadoPagoTable entity)
        {
            entity.MercadoPagoId = this.MercadoPagoId;
            entity.Fecha = this.Fecha;
            entity.Monto = this.Monto;
            entity.MaquinaId = this.MaquinaId;
            entity.MercadoPagoEstadoFinancieroId = this.MercadoPagoEstadoFinancieroId;
            entity.MercadoPagoEstadoTransmisionId = this.MercadoPagoEstadoTransmisionId;
            entity.Comprobante = this.Comprobante;
            entity.Descripcion = this.Descripcion;
            entity.FechaModificacionEstadoTransmision = this.FechaModificacionEstadoTransmision;
            entity.Entidad = this.Entidad;
            entity.UrlDevolucion = this.UrlDevolucion;
        }
    }
}