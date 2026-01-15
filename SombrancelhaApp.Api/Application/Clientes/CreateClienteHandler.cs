using MediatR;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Repositories;

namespace SombrancelhaApp.Api.Application.Clientes.CreateCliente;

public class CreateClienteHandler
    : IRequestHandler<CreateClienteCommand, Guid>
{
    private readonly IClienteRepository _repository;

    public CreateClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    // Adicionado o modificador 'async'
    public async Task<Guid> Handle(
        CreateClienteCommand request,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente(
            request.Nome,
            request.Idade,
            request.Telefone
        );

        // Agora aguardamos a operação assíncrona do repositório
        await _repository.AddAsync(cliente);

        // Retornamos o ID diretamente, o C# empacota em uma Task automaticamente por causa do 'async'
        return cliente.Id;
    }
}