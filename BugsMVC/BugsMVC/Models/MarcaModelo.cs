using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class MarcaModelo
    {
        [Key]
        public Guid MarcaModeloID { get; set; }

        [DisplayName("Marca y Modelo")]
        [StringLength(50)]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public string MarcaModeloNombre { get; set; }
    }
}