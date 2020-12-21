using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class FuncionRol
    {
        [Key]
        public int Id { get; set; }
        public int IdFuncion { get; set; }
        public string IdRol { get; set; }
        public virtual ApplicationRole Rol { get; set; }
        public virtual Funcion Funcion { get; set; }

    }
}