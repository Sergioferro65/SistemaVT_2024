using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class SeguridadDetalleViewModel
    {
        public int? IdFuncion { get; set; }

        public string TieneAcceso { get; set; }
        public string Operador { get; set; }
        public string Funcion { get; set; }
    }
}