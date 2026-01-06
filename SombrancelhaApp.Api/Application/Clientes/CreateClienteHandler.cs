using MediatR;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SombrancelhaApp.Api.Application.Clientes.CreateCliente;

public class CreateClienteHandler
    : IRequestHandler<CreateClienteCommand, Guid>
{
    private readonly IClienteRepository _repository;

    public CreateClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    public Task<Guid> Handle(
        CreateClienteCommand request,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente(
            request.Nome,
            request.Idade,
            request.Telefone
        );

        _repository.Add(cliente);

        return Task.FromResult(cliente.Id);
    }
}
