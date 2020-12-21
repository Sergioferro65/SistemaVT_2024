using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Rol : IdentityRole
    {
        public Rol() : base() {}
        
        public virtual ICollection<FuncionRol> FuncionesRoles { get; set; }
    }
}