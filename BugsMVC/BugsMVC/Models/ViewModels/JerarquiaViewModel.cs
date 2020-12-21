using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugsMVC.Models.ViewModels
{
    public class JerarquiaViewModel
    {
        public JerarquiaViewModel()
        {
            //PeriodoRecargaZona1 = 0;
            //PeriodoRecargaZona2 = 0;
            //PeriodoRecargaZona3 = 0;
            //PeriodoRecargaZona4 = 0;
            //PeriodoRecargaZona5 = 0;
            PeriodoRecargaZonaHtml = "";

            NombreZonas = new List<string>();
            ZonasActivas = new bool[5]  { false, false, false, false, false};
        }

        [Key]
        [DisplayName("Jerarquía")]
        public Guid JerarquiaID { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [DisplayName("Nombre")]
        [StringLength(100, ErrorMessage = "El tamaño máximo del texto debe ser 100 caracteres")]
        public string Nombre { get; set; }

        //Recarga de zona 1 a 5
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? RecargaZona1 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? RecargaZona2 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? RecargaZona3 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? RecargaZona4 { get; set; }

        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto de Recarga")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? RecargaZona5 { get; set; }

        //Descuento porcentual zona 1 a 10
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        [Range(0, 100, ErrorMessage = "El valor ingresado debe estar entre 0 y 100")]
        public decimal? DescuentoPorcentualZona1 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        [Range(0, 100, ErrorMessage = "El valor ingresado debe estar entre 0 y 100")]
        public decimal? DescuentoPorcentualZona2 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        [Range(0, 100, ErrorMessage = "El valor ingresado debe estar entre 0 y 100")]
        public decimal? DescuentoPorcentualZona3 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        [Range(0, 100, ErrorMessage = "El valor ingresado debe estar entre 0 y 100")]
        public decimal? DescuentoPorcentualZona4 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Descuento Porcentual")]
        [Range(0, 100, ErrorMessage = "El valor ingresado debe estar entre 0 y 100")]
        public decimal? DescuentoPorcentualZona5 { get; set; }
       
        //Monto recorte zona 1 a 5
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? MontoRecorteZona1 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? MontoRecorteZona2 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? MontoRecorteZona3 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? MontoRecorteZona4 { get; set; }
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [DisplayName("Monto Recorte")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Se superó el tamaño máximo de 9999999999999999,99")]
        public decimal? MontoRecorteZona5 { get; set; }

        [DisplayName("Período recarga")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public PeriodosRecarga? PeriodoRecargaZona1 { get; set; }

        [DisplayName("Período recarga")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public PeriodosRecarga? PeriodoRecargaZona2 { get; set; }

        [DisplayName("Período recarga")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public PeriodosRecarga? PeriodoRecargaZona3 { get; set; }

        [DisplayName("Período recarga")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public PeriodosRecarga? PeriodoRecargaZona4 { get; set; }

        [DisplayName("Período recarga")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public PeriodosRecarga? PeriodoRecargaZona5 { get; set; }



        public enum PeriodosRecarga
        {
            Ninguno,
            Diario,
            Semanal,
            Mensual
        }

        [DisplayName("Locación")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public Guid LocacionID { get; set; }

        //public SelectList LocacionIDList { get; set; }
        public List<string> NombreZonas { get; set; }
        public bool[] ZonasActivas { get; set; }
        public int NumeroZonasActivas { get; set; }

        public bool Zona1Activa { get; set; }
        public bool Zona2Activa { get; set; }
        public bool Zona3Activa { get; set; }
        public bool Zona4Activa { get; set; }
        public bool Zona5Activa { get; set; }

        public string NombreLocacion { get; set; }

        public string NombreZona1 { get; set; }
        public string NombreZona2 { get; set; }
        public string NombreZona3 { get; set; }
        public string NombreZona4 { get; set; }
        public string NombreZona5 { get; set; }

        public string OperadorNombre { get; set; }

        public string PeriodoRecargaZonaHtml { get; set; }
        public string RecargaZonaHtml { get; set; }
        public string RecargableZonaHtml { get; set; }
        public string MontoRecorteZonaHtml { get; set; }
        public string DescuentoPorcentualZonaHtml { get; set; }


        public static JerarquiaViewModel From(Jerarquia entity)
        {
            JerarquiaViewModel viewModel = new JerarquiaViewModel();
             
            viewModel.JerarquiaID = entity.JerarquiaID;

            viewModel.Nombre = entity.Nombre;
            viewModel.LocacionID = entity.LocacionID;
            //if (entity.RecargaZona1 != 0)
            //{
                viewModel.RecargaZona1 = entity.RecargaZona1;
                viewModel.MontoRecorteZona1 = entity.MontoRecorteZona1;
                viewModel.DescuentoPorcentualZona1 = entity.DescuentoPorcentualZona1;
                viewModel.PeriodoRecargaZona1 = (PeriodosRecarga)entity.PeriodoRecargaZona1;
            //}

            //if (entity.RecargaZona2 != 0)
            //{
                viewModel.RecargaZona2 = entity.RecargaZona2;
                viewModel.MontoRecorteZona2 = entity.MontoRecorteZona2;
                viewModel.DescuentoPorcentualZona2 = entity.DescuentoPorcentualZona2;
                viewModel.PeriodoRecargaZona2 = (PeriodosRecarga)entity.PeriodoRecargaZona2;
            //}
            //if (entity.RecargaZona3 != 0)
            //{
                viewModel.RecargaZona3 = entity.RecargaZona3;
                viewModel.MontoRecorteZona3 = entity.MontoRecorteZona3;
                viewModel.DescuentoPorcentualZona3 = entity.DescuentoPorcentualZona3;
                viewModel.PeriodoRecargaZona3 = (PeriodosRecarga)entity.PeriodoRecargaZona3;
            //}
            //if (entity.RecargaZona4 != 0)
            //{
                viewModel.RecargaZona4 = entity.RecargaZona4;
                viewModel.MontoRecorteZona4 = entity.MontoRecorteZona4;
                viewModel.DescuentoPorcentualZona4 = entity.DescuentoPorcentualZona4;
                viewModel.PeriodoRecargaZona4 = (PeriodosRecarga)entity.PeriodoRecargaZona4;
            //}
            //if (entity.RecargaZona5 != 0)
            //{
                viewModel.RecargaZona5 = entity.RecargaZona5;
                viewModel.MontoRecorteZona5 = entity.MontoRecorteZona5;
                viewModel.DescuentoPorcentualZona5 = entity.DescuentoPorcentualZona5;
                viewModel.PeriodoRecargaZona5 = (PeriodosRecarga)entity.PeriodoRecargaZona5;
            //}

            viewModel.NumeroZonasActivas = 0;

            if (entity.Locacion != null)
            {
                viewModel.NombreZona1 = entity.Locacion.NombreZona1;
                viewModel.NombreZona2 = entity.Locacion.NombreZona2;
                viewModel.NombreZona3 = entity.Locacion.NombreZona3;
                viewModel.NombreZona4 = entity.Locacion.NombreZona4;
                viewModel.NombreZona5 = entity.Locacion.NombreZona5;

                viewModel.NombreLocacion = entity.Locacion.Nombre;

                viewModel.Zona1Activa = !string.IsNullOrEmpty(entity.Locacion.NombreZona1);
                viewModel.Zona2Activa = !string.IsNullOrEmpty(entity.Locacion.NombreZona2);
                viewModel.Zona3Activa = !string.IsNullOrEmpty(entity.Locacion.NombreZona3);
                viewModel.Zona4Activa = !string.IsNullOrEmpty(entity.Locacion.NombreZona4);
                viewModel.Zona5Activa = !string.IsNullOrEmpty(entity.Locacion.NombreZona5);

                viewModel.NumeroZonasActivas = (viewModel.Zona1Activa ? 1 : 0) +
                                                (viewModel.Zona2Activa ? 1 : 0) +
                                                (viewModel.Zona3Activa ? 1 : 0) +
                                                (viewModel.Zona4Activa ? 1 : 0) +
                                                (viewModel.Zona5Activa ? 1 : 0);

                if (entity.Locacion.Operador != null)
                {
                    viewModel.OperadorNombre = entity.Locacion.Operador.Nombre;
                }
            }
            
            if (viewModel.Zona1Activa)
            {
                viewModel.PeriodoRecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona1, PeriodoRecargaToText(viewModel.PeriodoRecargaZona1.HasValue ? (int)viewModel.PeriodoRecargaZona1.Value : 0));
                viewModel.RecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona1, viewModel.RecargaZona1.HasValue ? Math.Round(viewModel.RecargaZona1.Value).ToString() : "");
                viewModel.MontoRecorteZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona1, viewModel.MontoRecorteZona1.HasValue ? Math.Round(viewModel.MontoRecorteZona1.Value).ToString(): "");
                viewModel.DescuentoPorcentualZonaHtml += String.Format("<div>Zona {0} = {1} %</div>", viewModel.NombreZona1, viewModel.DescuentoPorcentualZona1.HasValue ? Math.Round(viewModel.DescuentoPorcentualZona1.Value).ToString() : "");
            }


            if (viewModel.Zona2Activa)
            {
                viewModel.PeriodoRecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona2, PeriodoRecargaToText(viewModel.PeriodoRecargaZona2.HasValue ? (int)viewModel.PeriodoRecargaZona2.Value : 0));
                viewModel.RecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona2, viewModel.RecargaZona2.HasValue ? Math.Round(viewModel.RecargaZona2.Value).ToString() : "");
                viewModel.MontoRecorteZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona2, viewModel.MontoRecorteZona2.HasValue ? Math.Round(viewModel.MontoRecorteZona2.Value).ToString() : "");
                viewModel.DescuentoPorcentualZonaHtml += String.Format("<div>Zona {0} = {1} %</div>", viewModel.NombreZona2, viewModel.DescuentoPorcentualZona2.HasValue ? Math.Round(viewModel.DescuentoPorcentualZona2.Value).ToString() : "");
            }


            if (viewModel.Zona3Activa)
            {
                viewModel.PeriodoRecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona3, PeriodoRecargaToText(viewModel.PeriodoRecargaZona3.HasValue ? (int)viewModel.PeriodoRecargaZona3.Value : 0));
                viewModel.RecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona3, viewModel.RecargaZona3.HasValue ? Math.Round(viewModel.RecargaZona3.Value).ToString() : "");
                viewModel.MontoRecorteZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona3, viewModel.MontoRecorteZona3.HasValue ? Math.Round(viewModel.MontoRecorteZona3.Value).ToString() : "");
                viewModel.DescuentoPorcentualZonaHtml += String.Format("<div>Zona {0} = {1} %</div>", viewModel.NombreZona3, viewModel.DescuentoPorcentualZona3.HasValue ? Math.Round(viewModel.DescuentoPorcentualZona3.Value).ToString() : "");
            }


            if (viewModel.Zona4Activa)
            {
                viewModel.PeriodoRecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona4, PeriodoRecargaToText(viewModel.PeriodoRecargaZona4.HasValue ? (int)viewModel.PeriodoRecargaZona4.Value : 0));
                viewModel.RecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona4, viewModel.RecargaZona4.HasValue ? Math.Round(viewModel.RecargaZona4.Value).ToString() : "");
                viewModel.MontoRecorteZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona4, viewModel.MontoRecorteZona4.HasValue ? Math.Round(viewModel.MontoRecorteZona4.Value).ToString(): "");
                viewModel.DescuentoPorcentualZonaHtml += String.Format("<div>Zona {0} = {1} %</div>", viewModel.NombreZona4, viewModel.DescuentoPorcentualZona4.HasValue ? Math.Round(viewModel.DescuentoPorcentualZona4.Value).ToString() : "");
            }


            if (viewModel.Zona5Activa)
            {
                viewModel.PeriodoRecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona5, PeriodoRecargaToText(viewModel.PeriodoRecargaZona5.HasValue ? (int)viewModel.PeriodoRecargaZona5.Value : 0));
                viewModel.RecargaZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona5, viewModel.RecargaZona5.HasValue ? Math.Round(viewModel.RecargaZona5.Value).ToString() : "");
                viewModel.MontoRecorteZonaHtml += String.Format("<div>Zona {0} = {1} </div>", viewModel.NombreZona5, viewModel.MontoRecorteZona5.HasValue ? Math.Round(viewModel.MontoRecorteZona5.Value).ToString() : "");
                viewModel.DescuentoPorcentualZonaHtml += String.Format("<div>Zona {0} = {1} %</div>", viewModel.NombreZona5, viewModel.DescuentoPorcentualZona5.HasValue ? Math.Round(viewModel.DescuentoPorcentualZona5.Value).ToString() : "");
            }

            return viewModel;
        }

        public Jerarquia ToEntity(Jerarquia entity)
        {
            entity.JerarquiaID = this.JerarquiaID;

            entity.Nombre = this.Nombre;
            entity.LocacionID = this.LocacionID;

            entity.RecargaZona1 = this.RecargaZona1 ?? 0;
            entity.MontoRecorteZona1 = this.MontoRecorteZona1 ?? 0;
            entity.DescuentoPorcentualZona1 = this.DescuentoPorcentualZona1 ?? 0;

            entity.RecargaZona2 = this.RecargaZona2 ?? 0;
            entity.MontoRecorteZona2 = this.MontoRecorteZona2 ?? 0;
            entity.DescuentoPorcentualZona2 = this.DescuentoPorcentualZona2 ?? 0;

            entity.RecargaZona3 = this.RecargaZona3 ?? 0;
            entity.MontoRecorteZona3 = this.MontoRecorteZona3 ?? 0;
            entity.DescuentoPorcentualZona3 = this.DescuentoPorcentualZona3 ?? 0;

            entity.RecargaZona4 = this.RecargaZona4 ?? 0;
            entity.MontoRecorteZona4 = this.MontoRecorteZona4 ?? 0;
            entity.DescuentoPorcentualZona4 = this.DescuentoPorcentualZona4 ?? 0;

            entity.RecargaZona5 = this.RecargaZona5 ?? 0;
            entity.MontoRecorteZona5 = this.MontoRecorteZona5 ?? 0;
            entity.DescuentoPorcentualZona5 = this.DescuentoPorcentualZona5 ?? 0;

            entity.PeriodoRecargaZona1 = (int?)this.PeriodoRecargaZona1 ?? 0;
            entity.PeriodoRecargaZona2 = (int?)this.PeriodoRecargaZona2 ?? 0;
            entity.PeriodoRecargaZona3 = (int?)this.PeriodoRecargaZona3 ?? 0;
            entity.PeriodoRecargaZona4 = (int?)this.PeriodoRecargaZona4 ?? 0;
            entity.PeriodoRecargaZona5 = (int?)this.PeriodoRecargaZona5 ?? 0;
            return entity;
        }


        private static string PeriodoRecargaToText(int periodoRecarga)
        {
            string periodoRecargaTexto = "";

            switch (periodoRecarga)
            {
                case 0:
                    periodoRecargaTexto = "Ninguno";
                    break;
                case 1:
                    periodoRecargaTexto = "Diario";
                    break;
                case 2:
                    periodoRecargaTexto = "Semanal";
                    break;
                case 3:
                    periodoRecargaTexto = "Mensual";
                    break;
            }

            return periodoRecargaTexto;
        }
    }
}