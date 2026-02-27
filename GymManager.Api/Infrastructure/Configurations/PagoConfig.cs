using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Infraestructure.Configurations
{
    public class PagoConfig : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> b)
        {

            //Declaramos Clave Primaria
            b.HasKey(b => b.Id);

            //Configuramos las propiedades de la entidad
            b.Property(b => b.Importe).IsRequired();
            b.Property(b => b.MetodoPagoId).IsRequired();
            b.Property(b => b.SocioId).IsRequired();

            b.HasIndex(x => x.SocioId);

            //Configuramos las claves foraneas.
            b.HasOne(x=> x.MetodoPago)
                .WithMany(x => x.Pagos)
                .HasForeignKey(x => x.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Socio)
                .WithMany(x => x.Pagos)
                .HasForeignKey(x => x.SocioId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
