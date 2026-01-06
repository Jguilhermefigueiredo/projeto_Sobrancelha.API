
using FluentValidation;
using SombrancelhaApp.Api.DTOs;

namespace SombrancelhaApp.Api.Validators;

public class UpdateClienteDtoValidator : AbstractValidator<UpdateClienteDto>
{
    public UpdateClienteDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(x => x.Idade)
            .GreaterThan(0)
            .LessThan(120);

        RuleFor(x => x.Telefone)
            .NotEmpty()
            .MinimumLength(10);
    }
}
