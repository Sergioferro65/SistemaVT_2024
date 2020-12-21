using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class TablasOffline
    {
        public TablasOffline()
        {
        }

        [Key]
        public Guid TablasOfflineID { get; set; }

        public int FechaUnix { get; set; }

        [Range(0, 99999999)]
        public int NumeroBloque { get; set; }

        [StringLength(4096)]
        public string Bloque { get; set; }

        [Range(1, 5)]
        public int Zona { get; set; }

        [DisplayName("Locación")]
        public Guid? LocacionID { get; set; }
        public virtual Locacion Locacion { get; set; }
        
    }
}