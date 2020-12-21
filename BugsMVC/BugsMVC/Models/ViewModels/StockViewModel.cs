using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugsMVC.Models
{
    public class StockViewModel
    {
        public StockViewModel()
        {
            Cantidad = 0;
        }

        public Guid StockID { get; set; }

        public Guid ArticuloAsignacionID { get; set; }

        [DisplayName("Artículo")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid ArticuloID { get; set; }
        public SelectList ArticuloList { get; set; }
        public string Articulo { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public int? AlarmaBajo { get; set; }
        public int? Capacidad { get; set; }

        [DisplayName("Máquina")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid? MaquinaID { get; set; }
        public SelectList MaquinaList { get; set; }
        public string Maquina { get; set; }

        [DisplayName("Locación")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid? LocacionID { get; set; }
        public SelectList LocacionList { get; set; }
        public string Locacion { get; set; }

        [DisplayName("Fecha aviso")]
        public DateTime? FechaAviso { get; set; }

        [DisplayName("Locación")]
        public DateTime? FechaEdicionWeb { get; set; }

        public Guid? UsuarioIDEdicionWeb { get; set; }
        public string UsuarioEdicionWeb { get; set; }

        [DisplayName("Zona")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Zona { get; set; }
        public int NroZona { get; set; }

        public SelectList ZonaList { get; set; }

        public string OperadorNombre { get; set; }

        [DisplayName("Fecha de edición VT")]
        public DateTime? FechaEdicionVT { get; set; }

        public static StockViewModel From(Stock entity)
        {
            StockViewModel viewModel = new StockViewModel();
            viewModel.ArticuloAsignacionID = entity.ArticuloAsignacionID;
            viewModel.OperadorNombre = entity.ArticuloAsignacion.Locacion.Operador.Nombre;
            //viewModel.ArticuloList = entity.ArticuloAsignacion.Articulo;
            viewModel.ArticuloID = entity.ArticuloAsignacion.ArticuloID;
            viewModel.Articulo = entity.ArticuloAsignacion.Articulo.Nombre;
            viewModel.MaquinaID = entity.ArticuloAsignacion.MaquinaID;//.HasValue? entity.ArticuloAsignacion.MaquinaID.Value : Guid.Empty;
            viewModel.Maquina = entity.ArticuloAsignacion.Maquina.NombreAlias != null ? entity.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + " - " + entity.ArticuloAsignacion.Maquina.NumeroSerie + '(' + entity.ArticuloAsignacion.Maquina.NombreAlias + ')' : entity.ArticuloAsignacion.Maquina.MarcaModelo.MarcaModeloNombre + " - " + entity.ArticuloAsignacion.Maquina.NumeroSerie;
            viewModel.StockID = entity.StockID;
            viewModel.LocacionID = entity.ArticuloAsignacion.LocacionID;
            viewModel.Locacion = entity.ArticuloAsignacion.Locacion.Nombre;
            viewModel.Cantidad = entity.Cantidad;
            viewModel.Capacidad = entity.ArticuloAsignacion.Capacidad.Value;
            viewModel.FechaAviso = entity.FechaAviso;
            viewModel.FechaEdicionVT = entity.FechaEdicionVT;
            viewModel.FechaEdicionWeb = entity.FechaEdicionWeb;
            viewModel.UsuarioIDEdicionWeb = entity.UsuarioIDEdicionWeb;
            viewModel.UsuarioEdicionWeb = ((string.IsNullOrEmpty(entity.UsuarioEdicionWeb.Apellido)) ? String.Empty : entity.UsuarioEdicionWeb.Apellido + ", ") + entity.UsuarioEdicionWeb.Nombre;
            viewModel.NroZona = entity.ArticuloAsignacion.NroZona.HasValue? entity.ArticuloAsignacion.NroZona.Value:0;
            viewModel.Zona = entity.ArticuloAsignacion.NroZona.HasValue ? (entity.ArticuloAsignacion.NroZona.Value == 1 ? entity.ArticuloAsignacion.Locacion.NombreZona1 : entity.ArticuloAsignacion.NroZona.Value == 2 ? entity.ArticuloAsignacion.Locacion.NombreZona2 :
                            entity.ArticuloAsignacion.NroZona.Value == 3 ? entity.ArticuloAsignacion.Locacion.NombreZona3 : entity.ArticuloAsignacion.NroZona.Value == 4 ? entity.ArticuloAsignacion.Locacion.NombreZona4 :
                            entity.ArticuloAsignacion.NroZona.Value == 5 ? entity.ArticuloAsignacion.Locacion.NombreZona5 : string.Empty) : string.Empty;
            viewModel.AlarmaBajo = entity.ArticuloAsignacion.AlarmaBajo.HasValue ? entity.ArticuloAsignacion.AlarmaBajo.Value : 0;

            return viewModel;
        }

        public Stock ToEntity(Stock entity)
        {
            entity.StockID = this.StockID;
            entity.Cantidad = this.Cantidad;
            entity.FechaAviso = this.FechaAviso;
            entity.FechaEdicionVT = this.FechaEdicionVT;
            entity.FechaEdicionWeb = this.FechaEdicionWeb;
            entity.UsuarioIDEdicionWeb = this.UsuarioIDEdicionWeb;
            
            return entity;
        }
    }
}