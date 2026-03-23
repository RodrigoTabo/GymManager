using FluentValidation;
using GymManager.Shared.Contracts.Planes;

namespace GymManager.Api.Application.Validators.Plan
{
    public class UpdatePlanRequestValidator : AbstractValidator<UpdatePlanRequest>
    {

        public UpdatePlanRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El Nombre es requerido.")
                .MaximumLength(20).WithMessage("El Nombre no puede superar los 20 caracteres.");

            RuleFor(x => x.DuracionDias)
                .GreaterThan(0).WithMessage("La Duración de Días debe ser mayor a 0.")
                .LessThanOrEqualTo(365).WithMessage("La Duración de Días no puede superar 365 días.");

            RuleFor(x => x.Precio)
                .GreaterThan(0).WithMessage("El Precio debe ser mayor a 0.")
                .LessThanOrEqualTo(250000).WithMessage("El Precio no puede superar 250.000.");
        }

    }
}
