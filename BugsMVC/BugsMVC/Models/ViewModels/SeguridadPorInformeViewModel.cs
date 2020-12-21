using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class SeguridadPorInformeViewModel
    {
        [DisplayName("Menú")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public int FuncionID { get; set; }

        [DisplayName("Operadores")]
        public string[] Operadores { get; set; }
    }
}