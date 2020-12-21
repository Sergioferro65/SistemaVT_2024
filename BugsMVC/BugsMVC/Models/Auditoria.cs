using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Auditoria
    {
        // Audit Properties
        public Guid AuditoriaID { get; set; }

        [DisplayName("Usuario")]
        public string UserName { get; set; }

        [DisplayName("Dirección IP")]
        public string IPAddress { get; set; }

        [DisplayName("Área Accedida")]
        public string AreaAccessed { get; set; }

        [DisplayName("Fecha Acceso")]
        public DateTime TimeAccessed { get; set; }

        // Default Constructor
        public Auditoria() { }
    }
}