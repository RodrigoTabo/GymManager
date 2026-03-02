using GymManager.Api.Infrastructure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infraestructure.Configurations
{
    public class SocioConfig : IEntityTypeConfiguration<Socio>
    {
        public void Configure(EntityTypeBuilder<Socio> b)
        {
            //Declaramos la clave primaria
            b.HasKey(b => b.Id);

            //Configuramos las propiedades de la entidad
            b.Property(b => b.DNI).IsRequired();
            b.Property(b => b.Nombre).IsRequired().HasMaxLength(50);
            b.Property(b => b.Apellido).IsRequired().HasMaxLength(30);
            b.Property(b => b.FechaAlta).IsRequired();

            //Configuramos los datos unicos
            b.HasIndex(x => x.DNI).IsUnique();

            //Declaramos las claves foraneas.
            b.HasOne(x => x.Plan)
                 .WithMany(x => x.Socios)
                 .HasForeignKey(x => x.PlanId)
                 .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
