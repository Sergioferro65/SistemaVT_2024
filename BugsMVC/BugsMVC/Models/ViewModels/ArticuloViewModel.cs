using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class ArticuloViewModel
    {
        [DisplayName("Unidad de medida")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public UnidadMedida UnidadMedida { get; set; }

        [Key]
        [DisplayName("Artículo")]
        public Guid ArticuloID { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [StringLength(50)]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Costo Real")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal CostoReal { get; set; }

        [StringLength(100)]
        public string Marca { get; set; }

        [StringLength(100)]
        public string Modelo { get; set; }

        public string OperadorNombre { get; set; }

        [StringLength(15)]
        [DisplayName("Certificación")]
        public string Certificacion { get; set; }

        [DisplayName("Operador")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid OperadorID { get; set; }

        public static ArticuloViewModel From(Articulo entity)
        {
            ArticuloViewModel viewModel = new ArticuloViewModel();
            viewModel.OperadorID = entity.OperadorID;
            viewModel.OperadorNombre = entity.Operador.Nombre;
            viewModel.ArticuloID = entity.ArticuloID;
            viewModel.UnidadMedida = (UnidadMedida)entity.UnidadMedida;
            viewModel.Nombre = entity.Nombre;
            viewModel.Modelo = entity.Modelo;
            viewModel.Marca = entity.Marca;
            viewModel.CostoReal = entity.CostoReal;
            viewModel.Certificacion = entity.Certificacion;

            return viewModel;
        }

        public Articulo ToEntity(Articulo entity)
        {
            entity.OperadorID = this.OperadorID;
            entity.ArticuloID = this.ArticuloID;
            entity.UnidadMedida = (int)this.UnidadMedida;
            entity.Nombre = this.Nombre;
            entity.Modelo = this.Modelo;
            entity.Marca = this.Marca;
            entity.CostoReal = this.CostoReal;
            entity.Certificacion = this.Certificacion;

            return entity;
        }

    }
}