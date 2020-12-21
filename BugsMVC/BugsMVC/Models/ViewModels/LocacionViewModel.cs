using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class LocacionViewModel
    {
        public LocacionViewModel()
        {
            EsSuperadmin = false;
            MostrarUsuario = false;
            SaludarUsuario = false;
            MostrarDatosOpcionales = false;
            NombreZona1 = "";
            NombreZona2 = "";
            NombreZona3 = "";
            NombreZona4 = "";
            NombreZona5 = "";
            //PeriodoRecargaZona1 = 0;
            //PeriodoRecargaZona2 = 0;
            //PeriodoRecargaZona3 = 0;
            //PeriodoRecargaZona4 = 0;
            //PeriodoRecargaZona5 = 0;
            MostrarDatosOpcionales = false;
        }

        [Key]
        [DisplayName("Locación")]
        public Guid LocacionID { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Nombre")]
        [StringLength(100, ErrorMessage = "El tamaño máximo del texto debe ser 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(20, ErrorMessage = "El tamaño máximo del texto debe ser 20 caracteres")]
        public string CUIT { get; set; }

        [StringLength(30, ErrorMessage = "El tamaño máximo del texto debe ser 30 caracteres")]
        [DisplayName("Dirección")]
        public string Direccion { get; set; }

        [StringLength(20, ErrorMessage = "El tamaño máximo del texto debe ser 20 caracteres")]
        public string Localidad { get; set; }

        [StringLength(10, ErrorMessage = "El tamaño máximo del texto debe ser 10 caracteres")]
        [DisplayName("Código Postal")]
        public string CodigoPostal { get; set; }

        [StringLength(20, ErrorMessage = "El tamaño máximo del texto debe ser 20 caracteres")]
        public string Provincia { get; set; }

        [StringLength(15, ErrorMessage = "El tamaño máximo del texto debe ser 15 caracteres")]
        [DisplayName("Nombre")]
        public string NombreZona1 { get; set; }

        [StringLength(15, ErrorMessage = "El tamaño máximo del texto debe ser 15 caracteres")]
        [DisplayName("Nombre")]
        [Required(ErrorMessage = "Debe completa el nombre de la Zona")]
        public string NombreZona2 { get; set; }

        [StringLength(15, ErrorMessage = "El tamaño máximo del texto debe ser 15 caracteres")]
        [DisplayName("Nombre")]
        [Required(ErrorMessage = "Debe completa el nombre de la Zona")]

        public string NombreZona3 { get; set; }

        [StringLength(15, ErrorMessage = "El tamaño máximo del texto debe ser 15 caracteres")]
        [DisplayName("Nombre")]
        [Required(ErrorMessage = "Debe completa el nombre de la Zona")]
        public string NombreZona4 { get; set; }

        [StringLength(15, ErrorMessage = "El tamaño máximo del texto debe ser 15 caracteres")]
        [DisplayName("Nombre")]
        [Required(ErrorMessage = "Debe completa el nombre de la Zona")]
        public string NombreZona5 { get; set; }

        [DisplayName("¿Muestra Usuario?")]
        public bool MostrarUsuario { get; set; }

        [DisplayName("¿Saluda Usuario?")]
        public bool SaludarUsuario { get; set; }

        //[DisplayName("Período recarga")]
        //public PeriodosRecarga PeriodoRecargaZona1 { get; set; }

        //[DisplayName("Período recarga")]
        //public PeriodosRecarga PeriodoRecargaZona2 { get; set; }

        //[DisplayName("Período recarga")]
        //public PeriodosRecarga PeriodoRecargaZona3 { get; set; }

        //[DisplayName("Período recarga")]
        //public PeriodosRecarga PeriodoRecargaZona4 { get; set; }

        //[DisplayName("Período recarga")]
        //public PeriodosRecarga PeriodoRecargaZona5 { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Número")]
        [Range(0,65535, ErrorMessage = "El número de Locación debe estar entre 0 y 65535")]
        public int? Numero { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Operador")]
        public Guid OperadorID { get; set; }
        public bool EsSuperadmin { get; set; }
        public bool MostrarDatosOpcionales { get; set; }

        //public enum PeriodosRecarga
        //{
        //    Ninguno,
        //    Diario,
        //    Semanal,
        //    Mensual
        //}

        public bool Zona1Activa { get; set; }
        public bool Zona2Activa { get; set; }
        public bool Zona3Activa { get; set; }
        public bool Zona4Activa { get; set; }
        public bool Zona5Activa { get; set; }

        public static LocacionViewModel From(Locacion entity)
        {
            LocacionViewModel viewModel = new LocacionViewModel();

            viewModel.LocacionID = entity.LocacionID;
            viewModel.Nombre = entity.Nombre;
            viewModel.CUIT = entity.CUIT;
            viewModel.Direccion = entity.Direccion;
            viewModel.Localidad = entity.Localidad;

            viewModel.CodigoPostal = entity.CodigoPostal;
            viewModel.Provincia = entity.Provincia;
            viewModel.NombreZona1 = entity.NombreZona1;
            viewModel.NombreZona2 = entity.NombreZona2;
            viewModel.NombreZona3 = entity.NombreZona3;
            viewModel.NombreZona4 = entity.NombreZona4;
            viewModel.NombreZona5 = entity.NombreZona5;

            if (!string.IsNullOrEmpty(entity.NombreZona5))
            {
                viewModel.Zona1Activa = true;
                viewModel.Zona2Activa = true;
                viewModel.Zona3Activa = true;
                viewModel.Zona4Activa = true;
                viewModel.Zona5Activa = true;
            }
            else if(!string.IsNullOrEmpty(entity.NombreZona4))
            {
                viewModel.Zona1Activa = true;
                viewModel.Zona2Activa = true;
                viewModel.Zona3Activa = true;
                viewModel.Zona4Activa = true;
            }
            else if (!string.IsNullOrEmpty(entity.NombreZona3))
            {
                viewModel.Zona1Activa = true;
                viewModel.Zona2Activa = true;
                viewModel.Zona3Activa = true;
            }
            else if (!string.IsNullOrEmpty(entity.NombreZona2))
            {
                viewModel.Zona1Activa = true;
                viewModel.Zona2Activa = true;
            }
            else if (!string.IsNullOrEmpty(entity.NombreZona1))
            {
                viewModel.Zona1Activa = true;
            }

            //viewModel.PeriodoRecargaZona1 = (PeriodosRecarga)entity.PeriodoRecargaZona1;
            //viewModel.PeriodoRecargaZona2 = (PeriodosRecarga)entity.PeriodoRecargaZona2;
            //viewModel.PeriodoRecargaZona3 = (PeriodosRecarga)entity.PeriodoRecargaZona3;
            //viewModel.PeriodoRecargaZona4 = (PeriodosRecarga)entity.PeriodoRecargaZona4;
            //viewModel.PeriodoRecargaZona5 = (PeriodosRecarga)entity.PeriodoRecargaZona5;

            viewModel.MostrarUsuario = entity.MostrarUsuario;
            viewModel.SaludarUsuario = entity.SaludarUsuario;
            viewModel.Numero = entity.Numero;
            viewModel.OperadorID = entity.OperadorID;
            return viewModel;
        }

        public Locacion ToEntity(Locacion entity)
        {
            entity.LocacionID = this.LocacionID;

            entity.Nombre = this.Nombre;
            entity.CUIT = this.CUIT;
            entity.Direccion = this.Direccion;
            entity.Localidad = this.Localidad;

            entity.CodigoPostal = this.CodigoPostal;
            entity.Provincia = this.Provincia;
            entity.NombreZona1 = this.NombreZona1;
            entity.NombreZona2 = this.NombreZona2;
            entity.NombreZona3 = this.NombreZona3;
            entity.NombreZona4 = this.NombreZona4;
            entity.NombreZona5 = this.NombreZona5;

            //entity.PeriodoRecargaZona1 = (int)this.PeriodoRecargaZona1;
            //entity.PeriodoRecargaZona2 = (int)this.PeriodoRecargaZona2;
            //entity.PeriodoRecargaZona3 = (int)this.PeriodoRecargaZona3;
            //entity.PeriodoRecargaZona4 = (int)this.PeriodoRecargaZona4;
            //entity.PeriodoRecargaZona5 = (int)this.PeriodoRecargaZona5;

            entity.MostrarUsuario = this.MostrarUsuario;
            entity.SaludarUsuario = this.SaludarUsuario;
            entity.Numero = this.Numero.Value;
            entity.OperadorID = this.OperadorID;

            return entity;
        }
    }
}