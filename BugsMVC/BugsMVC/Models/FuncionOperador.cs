using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class FuncionOperador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DisplayName("Operador")]
        public Guid OperadorId { get; set; }

        [DisplayName("Funcion")]
        public int FuncionId { get; set; }

        public virtual Operador Operador { get; set; }
        public virtual Funcion Funcion { get; set; }
    }
}