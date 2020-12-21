using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Funcion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

        //[DisplayName("Operador")]
        //public Guid? OperadorId { get; set; }
        [DisplayName("Por Operador")]
        public bool PorOperador { get; set; }

        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        //public virtual Operador Operador {get;set;}
        public virtual ICollection<FuncionRol> FuncionesRoles { get; set; }
        public virtual ICollection<FuncionOperador> FuncionOperador { get; set; }
    }
}