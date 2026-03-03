using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Infrastructure.Data
{
    public class GymManagerDbContext : DbContext
    {

        public GymManagerDbContext(DbContextOptions<GymManagerDbContext> options)
                : base(options) { }

        // DbSets
        public DbSet<Socio> Socios => Set<Socio>();
        public DbSet<Plan> Planes => Set<Plan>();
        public DbSet<Pago> Pagos => Set<Pago>();
        public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
        public DbSet<DocumentoSocio> DocumentosSocio => Set<DocumentoSocio>();
        public DbSet<Asistencia> Asistencias => Set<Asistencia>();
        public DbSet<IntentosAcceso> IntentosAccesos => Set<IntentosAcceso>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymManagerDbContext).Assembly);
        }


    }
}
