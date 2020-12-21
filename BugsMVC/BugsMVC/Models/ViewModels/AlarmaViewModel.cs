using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class AlarmaViewModel
    {
        [DisplayName("Tipos De Alarma")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public int TipoDeAlarmaID { get; set; }

        [DisplayName("Locaciones")]
        [UIHint("Dropdown")]
        public Guid? LocacionID { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid OperadorID { get; set; }

        [DisplayName("Usuarios")]
        public string[] Usuarios { get; set; }

    }
}