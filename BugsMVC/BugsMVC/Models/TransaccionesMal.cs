using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugsMVC.Models
{
    public class TransaccionesMal
    {
        public TransaccionesMal()
        {
        }

        [Key]
        [DisplayName("Transacción Mal")]
        public Guid IdTransaccionMal { get; set; }

        [DisplayName("Número Terminal")]
        //[ForeignKey("IdTerminal")]
        public Guid? TerminalID { get; set; }

        [DisplayName("Transacción")]
        public string Transaccion { get; set; }

        [DisplayName("Fecha Descarga")]
        public DateTime? FechaDescarga { get; set; }

        [DisplayName("Motivo")]
        public string Motivo { get; set; }

        [DisplayName("Máquina")]
        public Guid? MaquinaID { get; set; }
     
        [DisplayName("Locación")]
        public Guid? LocacionID { get; set; }

        [DisplayName("Operador")]
        public Guid? OperadorID { get; set; }

        [ForeignKey("TerminalID")]
        public virtual Terminal Terminal { get; set; }
        public virtual Operador Operador { get; set; }
        public virtual Maquina Maquina { get; set; }
        public virtual Locacion Locacion { get; set; }

    }
}