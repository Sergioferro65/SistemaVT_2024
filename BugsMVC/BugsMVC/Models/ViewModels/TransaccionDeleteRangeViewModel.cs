using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class TransaccionDeleteRangeViewModel
    {

       public TransaccionDeleteRangeViewModel()
        {
            FechaDesde = DateTime.Now;
            FechaHasta = DateTime.Now;
        }

        [DisplayName("Fecha Desde")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaDesde { get; set; }

        [DisplayName("Fecha Hasta")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime FechaHasta { get; set; }
    }
}