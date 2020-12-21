using BugsMVC.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace BugsMVC.DAL
{
    public class BugsContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public BugsContext()
            : base("DefaultConnection")
        {
            this.Database.CommandTimeout = 1000;
        }

        public static BugsContext Create()
        {
            return new BugsContext();
        }

        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Jerarquia> Jerarquias { get; set; }
        public DbSet<Locacion> Locaciones { get; set; }
        public DbSet<Maquina> Maquinas { get; set; }
        public DbSet<Operador> Operadores { get; set; }
        public DbSet<Terminal> Terminales { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<TransaccionesMal> TransaccionesMal { get; set; }
        public DbSet<GrupoTransaccionesMal> GrupoTransaccionesMal { get; set; }
        public DbSet<Zona> Zonas { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<MarcaModelo> MarcasModelos { get; set; }
        public DbSet<ModeloTerminal> ModelosTerminal { get; set; }
        public DbSet<TransaccionTexto> TransaccionesTextos { get; set; }
        public DbSet<ArticuloAsignacion> ArticulosAsignaciones { get; set; }
        public DbSet<Auditoria> RegistrosAuditoria { get; set; }
        public DbSet<StockHistorico> StocksHistoricos { get; set; }
        public DbSet<TipoDeMovimiento> TipoDeMovimientos { get; set; }
        public DbSet<TipoDeAlarma> TipoDeAlarma { get; set; }
        public DbSet<AlarmaConfiguracion> AlarmaConfiguracion { get; set; }
        public DbSet<AlarmaConfiguracionDetalle> AlarmaConfiguracionDetalle { get; set; }
        public DbSet<MercadoPagoTable> MercadoPago { get; set; }
        public DbSet<MercadoPagoEstadoFinanciero> MercadoPagoEstadoFinanciero { get; set; }
        public DbSet<MercadoPagoEstadoTransmision> MercadoPagoEstadoTransmision { get; set; }
        public DbSet<MercadoPagoLog> MercadoPagoLog { get; set; }
        public DbSet<ViewTransaccion> ViewTransaccion { get; set; }
        public DbSet<TipoProducto> TipoProductos { get; set; }
        public DbSet<TablasOffline> TablasOfflines { get; set; }

        //Entidades para modelo de seguridad por funcion
        public DbSet<Funcion> Funciones { get; set; }
        public DbSet<FuncionRol> FuncionRoles { get; set; }
        public DbSet<FuncionOperador> FuncionOperador { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Funcion>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<FuncionRol>()
                .HasRequired(x => x.Rol)
                .WithMany(x => x.FuncionesRoles)
                .HasForeignKey(x => x.IdRol);

            modelBuilder.Entity<FuncionRol>()
                .HasRequired(x => x.Funcion)
                .WithMany(x => x.FuncionesRoles)
                .HasForeignKey(x => x.IdFuncion);


            base.OnModelCreating(modelBuilder);
        }
    }
}