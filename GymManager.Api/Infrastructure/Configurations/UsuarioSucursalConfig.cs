using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infrastructure.Configurations
{
    public class UsuarioSucursalConfig : IEntityTypeConfiguration<UsuarioSucursal>
    {
        public void Configure(EntityTypeBuilder<UsuarioSucursal> b)
        {
            b.ToTable("UsuarioSucursales");

            b.HasKey(x => new { x.UsuarioId, x.SucursalId });

            b.Property(x => x.EsPrincipal)
                .IsRequired();

            b.HasOne(x => x.Usuario)
                .WithMany(u => u.UsuarioSucursales)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Sucursal)
                .WithMany()
                .HasForeignKey(x => x.SucursalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
