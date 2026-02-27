using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infraestructure.Configurations
{
    public class AsistenciaConfig : IEntityTypeConfiguration<Asistencia>
    {
        public void Configure(EntityTypeBuilder<Asistencia> b)
        {
            //Declaramos Clave Primaria
            b.HasKey(b => b.Id);

            //Configuramos las propiedades de la entidad
            b.Property(b => b.FechaHora).IsRequired();
            b.Property(b => b.Resultado).HasMaxLength(50);
            b.Property(b => b.Motivo).HasMaxLength(100);
            b.Property(b => b.SocioId).IsRequired();

            //Declaramos la Clave Forania y su relacion
            b.HasOne(x => x.Socio)
                .WithMany(x => x.Asistencias)
                .HasForeignKey(x => x.SocioId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
