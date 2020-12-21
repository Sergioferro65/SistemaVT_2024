using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class GrupoTransaccionesMal
    {
        [Key]
        [DisplayName("Grupo Mal")]
        public Guid IDGrupoTransaccionesMal { get; set; }

        [DisplayName("Fecha")]
        public DateTime? Fecha{ get; set; }

        [DisplayName("Texto In")]
        public string TextoIn { get; set; }

        [DisplayName("Motivo")]
        public string Motivo { get; set; }
        [DisplayName("Nserie")]
        public int? NSerie { get; set; }
    }
}