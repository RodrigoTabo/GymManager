using FluentValidation;
using GymManager.Shared.Contracts.Socios;

namespace GymManager.Api.Application.Validators.Socio
{
    public class SocioQueryValidator : AbstractValidator<SocioQuery>
    {
        public SocioQueryValidator()
        {
            //No hace falta documentar que hace cada linea, literalmente lo esta diciendo.
            RuleFor(x => x.BuscarPor)
                .Must(x => string.IsNullOrEmpty(x) || new[] { "DNI", "NombreCompleto", "Plan" }.Contains(x))
                .WithMessage("BuscarPor debe ser 'DNI', 'NombreCompleto' o 'Plan'.");

            RuleFor(x => x.Texto)
                .MaximumLength(50)
                .WithMessage("El texto de búsqueda no puede superar los 50 caracteres.");

        }
    }
}
