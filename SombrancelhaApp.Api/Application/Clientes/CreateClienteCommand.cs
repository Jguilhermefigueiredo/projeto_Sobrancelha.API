using MediatR;

namespace SombrancelhaApp.Api.Application.Clientes.CreateCliente;

public record CreateClienteCommand(
    string Nome,
    int Idade,
    string Telefone
) : IRequest<Guid>;
