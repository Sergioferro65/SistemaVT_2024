using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Terminal
    {
        public Terminal()
        {
            NumeroSerie = 0;
            Version = 0;
        }

        [Key]
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

        //Campo que se usa en el sistema de Jonathan
        [Column("MaquinaID")]
        public Guid? MaquinaIDJonathan { get; set; }

        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

        [DisplayName("Modelo")]
        [Required(ErrorMessage ="Campo Obligatorio")]
        public Guid ModeloTerminalID { get; set; }
        public virtual ModeloTerminal ModeloTerminal { get; set; }

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

        public virtual ICollection<Maquina> Maquinas { get; set; }
        public virtual ICollection<Transaccion> Transacciones { get; set; }
        public virtual ICollection<TransaccionesMal> TransaccionesMal { get; set; }
    }
}