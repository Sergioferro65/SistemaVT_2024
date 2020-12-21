using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugsMVC.Models.ViewModels
{
    public class MercadoPagoConfiguracionViewModel
    {
        [StringLength(200)]
        [DisplayName("CLIENT_ID")]
        public string ClientId { get; set; }

        [StringLength(200)]
        [DisplayName("CLIENT_SECRET")]
        public string SecretToken { get; set; }

        public static MercadoPagoConfiguracionViewModel From(Operador entity)
        {
            MercadoPagoConfiguracionViewModel viewModel = new MercadoPagoConfiguracionViewModel();
            viewModel.ClientId = entity.ClientId;
            viewModel.SecretToken = entity.SecretToken;

            return viewModel;
        }

        public Operador ToEntity(Operador entity)
        {
            entity.ClientId = this.ClientId;
            entity.SecretToken = this.SecretToken;

            return entity;
        }
    }
}