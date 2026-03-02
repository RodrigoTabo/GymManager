using GymManager.Api.Infrastructure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infraestructure.Configurations
{
    public class MetodoPagoConfig : IEntityTypeConfiguration<MetodoPago>
    {
        public void Configure(EntityTypeBuilder<MetodoPago> b)
        {
            //Declaramos la Clave Primaria
            b.HasKey(b => b.Id);

            //Configuramos la propiedad de la entidad
            b.Property(b => b.Nombre).IsRequired().HasMaxLength(50);

            //Indicamos que necesitamos que sea unico
            b.HasIndex(b => b.Nombre).IsUnique();


        }

    }
}
