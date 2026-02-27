using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace GymManager.Api.Infraestructure.Configurations
{
    public class DocumentoSocioConfig : IEntityTypeConfiguration<DocumentoSocio>
    {
        public void Configure(EntityTypeBuilder<DocumentoSocio> b)
        {
            //Declaramos la clave primaria
            b.HasKey(b => b.Id);

            //Configuramos las propiedades de la entidad
            b.Property(b => b.Tipo).HasMaxLength(50);
            b.Property(b => b.UrlArchivo).IsRequired().HasMaxLength(500);
            b.Property(b => b.SocioId).IsRequired();

            //Indicamos que el ID del socio va a ser unico.
            b.HasIndex(b => b.SocioId).IsUnique();

            //Declaramos la clave foranea
            b.HasOne(x => x.Socio)
                .WithOne(x => x.Documento)
                .HasForeignKey<DocumentoSocio>(x => x.SocioId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
