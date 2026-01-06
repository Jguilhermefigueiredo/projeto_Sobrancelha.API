using FluentValidation;
using SombrancelhaApp.Api.DTOs;

namespace SombrancelhaApp.Api.Validators;

public class CreateClienteDtoValidator : AbstractValidator<CreateClienteDto>
{
    public CreateClienteDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres");

        RuleFor(x => x.Idade)
            .GreaterThan(0).WithMessage("A idade deve ser maior que zero");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("O telefone é obrigatório")
            .MinimumLength(10).WithMessage("O telefone deve ter ao menos 10 dígitos");
    }
}
