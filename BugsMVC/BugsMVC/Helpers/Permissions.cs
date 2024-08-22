using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Helpers
{
    public class Permissions
    {
        public enum Locacion :int 
        {
            Index=1,
            Create = 30,
            Update = 31,
            Delete = 32
        }
        public enum Usuario : int
        {
            Index = 2,
            Create = 33,
            Update = 34,
            Delete = 35,
            CargaMasiva = 36
        }
        public enum Maquina : int
        {
            Index = 3,
            Create = 37,
            Update = 38,
            Delete = 39
        }
        public enum Terminal : int
        {
            Index = 4,
            Create = 22,
            Update = 23,
            Delete = 24

        }
        public enum Transaccion : int
        {
            Index = 5,
            Delete = 54,
            DeleteRange = 55
        }
        public enum UsuarioWeb : int
        {
            Index = 6,
            Create = 40,
            Update = 41,
            Delete = 42     
        }
        public enum Articulo : int
        {
            Listado = 7,
            Create = 43,
            Update = 44,
            Delete = 45,
            Asignacion = 20,
            AsignacionCreate = 46,
            AsignacionUpdate = 47,
            AsignacionDelete = 48
        }
        public enum Stock : int
        {
            Index = 8,
            Reposiciones = 21,
            Update = 52,
            Delete = 53,
        }
        public enum Informes : int
        {
            Consumo = 9,
            Ventas = 10,
            CertificadoEntregaEpp = 11,
            EntregaEPPTrabajador = 12,
            EntregaTotalEPP = 13
        }
        public enum Operadores : int
        {
            Index = 14,
        }
        public enum ModelosMaquina : int
        {
            Index = 15,
        }
        public enum ModeloTerminal : int
        {
            Index = 16,
        }
        public enum TransaccionTexto : int
        {
            Index = 17,
        }
        public enum Auditoria : int
        {
            Index = 18
        }
        public enum Jerarquia : int
        {
            Index = 19,
            Create = 49,
            Update = 50,
            Delete = 51
        }

        public enum Consumidor : int
        {
            Index = 25,
            MiCuenta = 29,
        }

        public enum Seguridad : int
        {
            Seguridad = 27
        }

        public enum Alarma : int
        {
            Configuracion = 28
        }

        public enum MercadoPago : int
        {
            Index = 56
        }

        public enum ConfiguracionPagosExternos : int
        {
            Index = 57
        }

        public enum PagosExternos : int
        {
            Index = 58,
            DeleteRange = 59,
            Delete = 60
        }

    }
}