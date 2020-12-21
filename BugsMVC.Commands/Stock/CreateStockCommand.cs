using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugsMVC.Commands
{
    public class CreateStockCommand
    {
        public CreateStockCommand Configure()
        {
            return this;
        }

        public void Execute()
        {


            Stock stock = null;
            StockHistorico historico = new StockHistorico();

            string userId = User.Identity.GetUserId();
            var currentUser = db.Users.SingleOrDefault(x => x.Id == userId);

            //No habria que preguntar por el id de asignacion?
            if (stockViewModel.StockID == Guid.Empty)
            {
                stock = new Stock();
                stock.StockID = Guid.NewGuid();
                historico.TipoDeMovimientoID = Guid.Parse(Constantes.Nuevo);
            }
            else
            {//Ver, creo que nunca entraria acá
                stock = db.Stocks.Find(stockViewModel.StockID);
                historico.TipoDeMovimientoID = Guid.Parse(Constantes.Reposicion);
            }

            //stock.ArticuloID = stockViewModel.ArticuloID;
            //stock.MaquinaID = stockViewModel.MaquinaID;
            //stock.AlarmaBajo = stockViewModel.AlarmaBajo;
            //stock.AlarmaMuyBajo = stockViewModel.AlarmaMuyBajo;
            //stock.Capacidad = stockViewModel.Capacidad;
            //stock.AlarmaActiva = stockViewModel.AlarmaActiva;
            stock.Cantidad = stockViewModel.Cantidad;
            stock.FechaAviso = null;
            stock.FechaEdicionWeb = DateTime.Now;
            stock.UsuarioIDEdicionWeb = currentUser.UsuarioID;
            stock.UsuarioEdicionWeb = currentUser.Usuario;

            //stock.ArticuloAsignacionID=
            //stock = stockViewModel.ToEntity(stock);              

            if (stockViewModel.StockID == Guid.Empty)
            {
                db.Stocks.Add(stock);
            }
            else
            {
                db.Entry(stock).State = EntityState.Modified;
            }

            historico.StockHistoricoID = Guid.NewGuid();
            historico.UsuarioID = currentUser.UsuarioID;
            historico.StockID = stock.StockID;
            historico.Fecha = DateTime.Now;
            historico.Cantidad = stock.Cantidad;
            db.StockHistorico.Add(historico);

            db.SaveChanges();


        }
    }
}
