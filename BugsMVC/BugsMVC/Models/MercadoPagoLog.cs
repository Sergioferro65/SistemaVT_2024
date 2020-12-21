using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class MercadoPagoLog
    {
        [Key]
        public int MercadoPagoLogId { get; set; }
        public string Descripcion { get; set; }
    }
}