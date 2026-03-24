using FluentValidation;
using GymManager.Shared.Contracts.Planes;

namespace GymManager.Api.Application.Validators.Plan
{
    public class CreatePlanRequestValidator : AbstractValidator<CreatePlanRequest>
    {

        public CreatePlanRequestValidator()
        {
            //No hace falta documentar que hace cada linea, literalmente lo esta diciendo.
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El Nombre es requerido.")
                .MaximumLength(20).WithMessage("El Nombre no puede superar los 20 caracteres.")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El Nombre solo puede contener letras.");

            RuleFor(x => x.DuracionDias)
                .GreaterThan(0).WithMessage("La Duración de Días debe ser mayor a 0.")
                .LessThanOrEqualTo(365).WithMessage("La Duración de Días no puede superar 365 días.");

            RuleFor(x => x.Precio)
                .GreaterThan(0).WithMessage("El Precio debe ser mayor a 0.")
                .LessThanOrEqualTo(250000).WithMessage("El Precio no puede superar 250.000.");

        }
    }
}
