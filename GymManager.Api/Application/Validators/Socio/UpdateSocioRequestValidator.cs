using FluentValidation;
using GymManager.Shared.Contracts.Socios;

namespace GymManager.Api.Application.Validators.Socio
{
    public class UpdateSocioRequestValidator : AbstractValidator<UpdateSocioRequest>
    {
        public UpdateSocioRequestValidator()
        {
            //No hace falta documentar que hace cada linea, literalmente lo esta diciendo.
            RuleFor(x => x.DNI)
                .NotEmpty().WithMessage("El DNI es obligatorio.")
                .Length(8, 10).WithMessage("El DNI debe tener entre 7 y 10 caracteres.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El Nombre es obligatorio.")
                .MaximumLength(15).WithMessage("El Nombre no puede superar los 15 caracteres.");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El Apellido es obligatorio.")
                .MaximumLength(30).WithMessage("El Apellido no puede superar los 30 caracteres.");

            RuleFor(x => x.PlanId)
                .GreaterThan(0).WithMessage("Debes seleccionar un Plan válido");

            RuleFor(x => x.Telefono)
                .Matches(@"^\+?\d{7,15}$").WithMessage("El Teléfono no es válido")
                .When(x => !string.IsNullOrEmpty(x.Telefono));

            RuleFor(x => x.FechaNacimiento)
                .NotEmpty().WithMessage("La fecha es obligatoria")
                .LessThan(DateTime.Now).WithMessage("La fecha no puede ser futura");

        }
    }
}
