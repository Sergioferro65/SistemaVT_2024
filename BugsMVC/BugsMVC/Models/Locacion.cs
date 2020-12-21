using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugsMVC.Models
{
    public class Locacion
    {
        public Locacion()
        {
            MostrarUsuario = false;
            SaludarUsuario = false;
            NombreZona1 = "";
            NombreZona2 = "";
            NombreZona3 = "";
            NombreZona4 = "";
            NombreZona5 = "";
            Numero = 0;
        }

        [Key]
        [DisplayName("Locación")]
        public Guid LocacionID { get; set; }

        [Required(ErrorMessage="Campo obligatorio")]
        [DisplayName("Nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(20)]
        public string CUIT { get; set; }

        [StringLength(30)]
        [DisplayName("Dirección")]
        public string Direccion { get; set; }

        [StringLength(20)]
        public string Localidad { get; set; }

        [StringLength(10)]
        [DisplayName("Código Postal")]
        public string CodigoPostal { get; set; }

        [StringLength(20)]
        public string Provincia { get; set; }

        [StringLength(15)]
        [DisplayName("Nombre")]
        public string NombreZona1 { get; set; }

        [StringLength(15)]
        [DisplayName("Nombre")]
        public string NombreZona2 { get; set; }

        [StringLength(15)]
        [DisplayName("Nombre")]
        public string NombreZona3 { get; set; }

        [StringLength(15)]
        [DisplayName("Nombre")]
        public string NombreZona4 { get; set; }

        [StringLength(15)]
        [DisplayName("Nombre")]
        public string NombreZona5 { get; set; }

        [DisplayName("¿Muestra Usuario?")]
        public bool MostrarUsuario { get; set; }

        [DisplayName("¿Saluda Usuario?")]
        public bool SaludarUsuario { get; set; }

        [Required(ErrorMessage="Campo obligatorio")]
        [DisplayName("Número")]
        [Range(0, 65535, ErrorMessage = "El número de Locación debe estar entre 0 y 65535")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Operador")]
        public Guid OperadorID { get; set; }
        public virtual Operador Operador { get; set; }

        public virtual ICollection<Jerarquia> Jerarquias { get; set; }
        public virtual ICollection<Maquina> Maquinas { get; set; }
        public virtual ICollection<ArticuloAsignacion> ArticulosAsignaciones { get; set; }
        public virtual ICollection<AlarmaConfiguracion> AlarmaConfiguraciones { get; set; }
        public virtual ICollection<TransaccionesMal> TransaccionesMal { get; set; }
        public virtual ICollection<TablasOffline> TablaOfflines { get; set; }
    }
}