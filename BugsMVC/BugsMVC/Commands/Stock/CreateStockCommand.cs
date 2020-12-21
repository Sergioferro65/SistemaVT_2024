using BugsMVC.DAL;
using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugsMVC.Commands
{
    public class CreateStockCommand
    {
        Guid StockID,
             ArticuloAsignacionID;
        int Cantidad;
        BugsContext Db;
        string UserID;

        public CreateStockCommand Configure(Guid stockID,Guid articuloAsignacionID,int cantidad, BugsContext db,string userID)
        {
            StockID = stockID;
            ArticuloAsignacionID = articuloAsignacionID;
            Cantidad = cantidad;
            UserID = userID;
            Db = db;

            return this;
        }

        public void Execute()
        {
            Stock stock = null;
            StockHistorico historico = new StockHistorico();

            var currentUser = Db.Users.SingleOrDefault(x => x.Id == UserID);

            if (StockID == Guid.Empty)
            {
                stock = new Stock();
                stock.StockID = Guid.NewGuid();
                historico.TipoDeMovimientoID = Guid.Parse(Constantes.Nuevo);
                historico.Cantidad = Cantidad;
            }
            else
            {
                stock = Db.Stocks.Find(StockID);
                historico.TipoDeMovimientoID = Guid.Parse(Constantes.Reposicion);
                historico.Cantidad = Cantidad - stock.Cantidad;
            }

            stock.ArticuloAsignacionID = ArticuloAsignacionID;
            stock.Cantidad = Cantidad;
            stock.FechaEdicionWeb = DateTime.Now;
            stock.UsuarioIDEdicionWeb = currentUser.UsuarioID;
            stock.UsuarioEdicionWeb = currentUser.Usuario;     

            if (StockID == Guid.Empty)
            {
                Db.Stocks.Add(stock);
            }
            else
            {
                Db.Entry(stock).State = EntityState.Modified;
            }

            historico.StockHistoricoID = Guid.NewGuid();
            historico.UsuarioID = currentUser.UsuarioID;
            historico.StockID = stock.StockID;
            historico.Fecha = DateTime.Now;
            historico.FechaAviso = stock.FechaAviso;
            Db.StocksHistoricos.Add(historico);

            Db.SaveChanges();
        }
    }
}
