using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugsMVC.Models
{
    public class Operador
    {
        public Operador()
        {
            Numero = 0;
        }

        [Key]
        [DisplayName("Operador")]
        public Guid OperadorID { get; set; }               

        [StringLength(100)]
        [DisplayName("Operador")]
        public string Nombre { get; set; }

        [DisplayName("Número")]
        [Range(0, 65535, ErrorMessage = "El número de Operador debe estar entre 0 y 65535")]
        public int Numero { get; set; }

        [DisplayName("Tiempo Aviso Inhibición (Minutos)")]
        [Range(1, 65535, ErrorMessage = "El Tiempo de Aviso Inhibición debe estar entre 1 y 65535")]
        public int TiempoAvisoInhibicion { get; set; }

        [DisplayName("Tiempo Aviso Conexión (Minutos)")]
        [Range(1, 65535, ErrorMessage = "El Tiempo de Aviso Conexión debe estar entre 1 y 65535")]
        public int TiempoAvisoConexion { get; set; }

        [StringLength(200)]
        [DisplayName("ClientId")]
        public string ClientId { get; set; }

        [StringLength(200)]
        [DisplayName("Token")]
        public string SecretToken { get; set; }

        public virtual ICollection<Locacion> Locaciones { get; set; }
        public virtual ICollection<Articulo> Articulos { get; set; }
        public virtual ICollection<Terminal> Terminales { get; set; }
        //public virtual ICollection<Funcion> Funciones { get; set; }
        public virtual ICollection<FuncionOperador> FuncionOperadores { get; set; }
        public virtual ICollection<Maquina> Maquinas { get; set; }
        public virtual ICollection<AlarmaConfiguracion> AlarmaConfiguraciones { get; set; }
        public virtual ICollection<TransaccionesMal> TransaccionesMal { get; set; }
    }
}