using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class SeguridadViewModel
    {
        [DisplayName("Rol")]
        public string IdRol { get; set; }
        public IEnumerable<SeguridadDetalleViewModel> SeguridadDetalle { get; set; }
    }
}