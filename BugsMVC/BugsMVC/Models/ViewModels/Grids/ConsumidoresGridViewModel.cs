using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels.Grids
{
    public class ConsumidoresGridViewModel
    {
        public Guid UsuarioID { get; set; }
        public string OperadorNombre { get; set; }
        public string Jerarquia { get; set; }
        public string Locacion { get; set; }
        public string Operador { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Legajo { get; set; }
        public int Dni { get; set; }
        public int Numero { get; set; }
        public int ClaveTerminal { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool Inhibido { get; set; }
        public DateTime? FechaInhibido { get; set; }
        public decimal Efectivo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public decimal CreditoZona1 { get; set; }
        public decimal CreditoZona2 { get; set; }
        public decimal CreditoZona3 { get; set; }
        public decimal CreditoZona4 { get; set; }
        public decimal CreditoZona5 { get; set; }
        public string Email { get; set; }
    }
}