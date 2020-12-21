using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class TerminalViewModel
    {
        public TerminalViewModel()
        {
            NumeroSerie = 0;
            Version = 0;
        }

        [DisplayName("Terminal")]
        public Guid TerminalID { get; set; }

        [Required]
        [DisplayName("N° Serie")]
        public int NumeroSerie { get; set; }

        [Required(ErrorMessage = "Campo Obligatorio")]
        [StringLength(40, ErrorMessage = "El tamaño máximo del texto debe ser 40 caracteres")]
        public string Interfaz { get; set; }

        [DisplayName("Versión")]
        public int Version { get; set; }

        [DisplayName("Fecha Fabricación")]
        public DateTime? FechaFabricacion { get; set; }

        [DisplayName("Fecha Estado Seteos Escritura")]
        public DateTime? FechaEstadoSeteosEscritura { get; set; }

        [DisplayName("TipoLector_out")]
        public int? TipoLector_out { get; set; }

        [DisplayName("Fecha Alta")]
        public DateTime? FechaAlta { get; set; }

        public Guid? MaquinaIDJonathan { get; set; }

        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }

        [DisplayName("Modelo")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public Guid ModeloTerminalID { get; set; }

        [DisplayName("Periféricos")]
        public int? Perifericos { get; set; }

        [DisplayName("Modulo Comunicación")]
        public string ModuloComunicacion { get; set; }

        [DisplayName("Sim Card")]
        public string SimCard { get; set; }

        [DisplayName("Nivel Señal 1")]
        public int? NivelSenal1 { get; set; }

        [DisplayName("Nivel Señal 2")]
        public int? NivelSenal2 { get; set; }

        [DisplayName("Nivel Señal 3")]
        public int? NivelSenal3 { get; set; }

        [DisplayName("Fecha Nivel 1")]
        public DateTime? FechaNivel1 { get; set; }

        [DisplayName("Fecha Nivel 2")]
        public DateTime? FechaNivel2 { get; set; }

        [DisplayName("Fecha Nivel 3")]
        public DateTime? FechaNivel3 { get; set; }

        [DisplayName("Periféricos Habilitados")]
        public string PerifericoDescripcion { get; set; }
        
        [DisplayName("Máquina asignada")]
        public string MaquinaAsignada { get; set; }

        public bool BackToMaquina { get; set; }

        [DisplayName("Maquina")]
        public string NumeroSerieMaquinaJonathan { get; set; }

        [DisplayName("Modelo")]
        public string ModeloTerminal { get; set; }

        public static TerminalViewModel From(Terminal entity)
        {
            TerminalViewModel viewModel = new TerminalViewModel();

            viewModel.TerminalID = entity.TerminalID;
            viewModel.NumeroSerie = entity.NumeroSerie;
            viewModel.Interfaz = entity.Interfaz;
            viewModel.Version = entity.Version;
            viewModel.FechaFabricacion = entity.FechaFabricacion;
            viewModel.FechaEstadoSeteosEscritura = entity.FechaEstadoSeteosEscritura;
            viewModel.TipoLector_out = entity.TipoLector_out;
            viewModel.FechaAlta = entity.FechaAlta;
            viewModel.MaquinaIDJonathan = entity.MaquinaIDJonathan;
            viewModel.OperadorID = entity.OperadorID;
            viewModel.ModeloTerminalID = entity.ModeloTerminalID;
            viewModel.Perifericos = entity.Perifericos;
            viewModel.ModuloComunicacion = entity.ModuloComunicacion;
            viewModel.SimCard = entity.SimCard;
            viewModel.NivelSenal1 = entity.NivelSenal1;
            viewModel.NivelSenal2 = entity.NivelSenal2;
            viewModel.NivelSenal3 = entity.NivelSenal3;
            viewModel.FechaNivel1 = entity.FechaNivel1;
            viewModel.FechaNivel2 = entity.FechaNivel2;
            viewModel.FechaNivel3 = entity.FechaNivel3;

            viewModel.PerifericoDescripcion = GetPerifericoDescripcion(entity.Perifericos ?? 0);
            viewModel.PerifericoDescripcion = viewModel.PerifericoDescripcion.Replace("\r\n", "<br />");
            viewModel.MaquinaAsignada = entity.Maquinas.Any() ? "SI" : "NO";
            viewModel.BackToMaquina = false;
            var maquinaIdJonathan = entity.Maquinas.SingleOrDefault(x => x.MaquinaID == entity.MaquinaIDJonathan);
            if (maquinaIdJonathan != null)
            {
                viewModel.NumeroSerieMaquinaJonathan = maquinaIdJonathan.NumeroSerie;
            }
            viewModel.ModeloTerminal = entity.ModeloTerminal.Modelo;
            return viewModel;
        }

        public Terminal ToEntity(Terminal entity)
        {
            entity.TerminalID = this.TerminalID;
            entity.NumeroSerie = this.NumeroSerie;
            entity.Interfaz = this.Interfaz;
            entity.Version = this.Version;
            entity.FechaFabricacion = this.FechaFabricacion;
            entity.FechaEstadoSeteosEscritura = this.FechaEstadoSeteosEscritura;
            entity.TipoLector_out = this.TipoLector_out;
            entity.FechaAlta = this.FechaAlta;
            entity.MaquinaIDJonathan = this.MaquinaIDJonathan;
            entity.OperadorID = this.OperadorID;
            entity.ModeloTerminalID = this.ModeloTerminalID;
            entity.Perifericos = this.Perifericos;
            entity.ModuloComunicacion = this.ModuloComunicacion;
            entity.SimCard = this.SimCard;
            entity.NivelSenal1 = this.NivelSenal1;
            entity.NivelSenal2 = this.NivelSenal2;
            entity.NivelSenal3 = this.NivelSenal3;
            entity.FechaNivel1 = this.FechaNivel1;
            entity.FechaNivel2 = this.FechaNivel2;
            entity.FechaNivel3 = this.FechaNivel3;

            return entity;
        }

        public static string GetPerifericoDescripcion(int perifericos)
        {
            int aux = perifericos;
            string result = string.Empty;
            if ((aux -= 64) >= 0)
            {
                result = "Recarga Efectivo" + Environment.NewLine;
            }
            else
            {
                aux += 64;
            }

            if ((aux -= 32) >= 0)
            {
                result += ("Monedero ejecutivo" + Environment.NewLine);
            }
            else
            {
                aux += 32;
            }

            if ((aux -= 16) >= 0)
            {
                result += ("POS" + Environment.NewLine);
            }
            else
            {
                aux += 16;
            }

            if ((aux -= 8) >= 0)
            {
                result += ("Billetero Paralelo" + Environment.NewLine);
            }
            else
            {
                aux += 8;
            }
            if ((aux -= 4) >= 0)
            {
                result += ("Validador 212" + Environment.NewLine);
            }
            else
            {
                aux += 4;
            }
            if ((aux -= 2) >= 0)
            {
                result += ("WebCoin" + Environment.NewLine);
            }
            else
            {
                aux += 2;
            }
            if ((aux -= 1) >= 0)
            {
                result += ("Lector" + Environment.NewLine);
            }
            else
            {
                aux += 1;
            }
            return result;
        }
    }
}