using BugsMVC.DAL;
using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Commands.Seguridad
{
    public class SeguridadPorInformeCommand
    {
        int FuncionID;
        List<Guid> Operadores;
        BugsContext Db;

        public SeguridadPorInformeCommand Configure(int funcionID, List<Guid> operadores, BugsContext db)
        {
            FuncionID = funcionID;
            Operadores = operadores;
            Db = db;
            return this;
        }

        public void Execute()
        {
            if (Operadores.Count() == 0)
            {
                IQueryable<FuncionOperador> borrarTodos = Db.FuncionOperador.Where(x => x.FuncionId == FuncionID);
                Db.FuncionOperador.RemoveRange(borrarTodos);
            }
            else
            {
                IQueryable<Guid> operadoresActuales = Db.FuncionOperador.Where(x => x.FuncionId == FuncionID).Select(x => x.OperadorId);

                //FuncionOperador a borrar
                IQueryable<Guid> operadoresQuitar = operadoresActuales.Except(Operadores);

                //FuncionOperador a agregar
                IQueryable<Guid> operadoresAgregar = Operadores.Except(operadoresActuales).AsQueryable();

                //borrar
                IQueryable<FuncionOperador> quitar = Db.FuncionOperador.Where(x => x.FuncionId == FuncionID && operadoresQuitar.Any(y => y == x.OperadorId));
                Db.FuncionOperador.RemoveRange(quitar);

                //agregar
                Funcion funcion = Db.Funciones.Where(x => x.Id == FuncionID).First();

                foreach (Guid item in operadoresAgregar)
                {
                    Operador operador = Db.Operadores.Where(x => x.OperadorID == item).First();

                    FuncionOperador entity = new FuncionOperador();
                    entity.Funcion = funcion;
                    entity.Operador = operador;
                    Db.FuncionOperador.Add(entity);
                }
            }

            Db.SaveChanges();
        }
    }
}