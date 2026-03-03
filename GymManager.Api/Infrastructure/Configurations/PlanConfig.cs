using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infrastructure.Configurations
{
    public class PlanConfig : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> b)
        {
            //Declaramos la clave primaria
            b.HasKey(b => b.Id);

            //Configuramos las propiedades de la entidad
            b.Property(b => b.Nombre).IsRequired().HasMaxLength(100);
            b.Property(b => b.DuracionDias).IsRequired();
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)").IsRequired();

        }

    }
}
