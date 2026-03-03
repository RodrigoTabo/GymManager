using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infrastructure.Configurations
{
    public class IntentosAccesoConfig : IEntityTypeConfiguration<IntentosAcceso>
    {
        public void Configure(EntityTypeBuilder<IntentosAcceso> b)
        {
            //Declaramos Clave Primaria
            b.HasKey(b => b.Id);

            //Configuramos las propiedades de la entidad
            b.Property(b => b.FechaRegistro).IsRequired();
            b.Property(x => x.DniIngresado).IsRequired().HasMaxLength(12);
            b.Property(b => b.Resultado).IsRequired();
            b.Property(b => b.Motivo).IsRequired();

            b.HasIndex(x => x.FechaRegistro);
            b.HasIndex(x => x.SocioId);
            b.HasIndex(x => x.DniIngresado);


            //Declaramos la Clave Forania y su relacion
            b.HasOne(x => x.Socio)
                .WithMany(x => x.IntentosAccesos)
                .HasForeignKey(x => x.SocioId)
                .OnDelete(DeleteBehavior.Restrict);


        }

    }
}

