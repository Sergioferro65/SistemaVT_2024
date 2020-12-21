using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class ModeloTerminal
    {
        [Key]
        public Guid ModeloTerminalID { get; set; }

        [DisplayName("Modelo")]
        [Required(ErrorMessage ="Campo Obligatorio")]
        [StringLength(100, ErrorMessage = "El {0} debe tener como máximo {1} caracteres.")]
        public string Modelo { get; set; }
    }
}