﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        }
    }
}