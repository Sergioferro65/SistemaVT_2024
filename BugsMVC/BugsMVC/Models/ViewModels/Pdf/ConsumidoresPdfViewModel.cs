using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels.Pdf
{
    public class ConsumidoresPdfViewModel
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Legajo { get; set; }
        public string Jerarquia { get; set; }
        public int Numero { get; set; }
        public int ClaveTerminal { get; set; }
    }
}