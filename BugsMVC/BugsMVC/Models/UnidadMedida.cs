using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public enum UnidadMedida
    {
        [Description("Unidades")]
        Unidades = 0,
        [Description("Gramos")]
        Gramos=1,
        [Description("Cm3")]
        cm3=2
    }
}