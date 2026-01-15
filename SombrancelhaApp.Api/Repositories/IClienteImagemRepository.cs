using SombrancelhaApp.Api.Domain;

namespace SombrancelhaApp.Api.Repositories;

public interface IClienteImagemRepository
{
    void Add(ClienteImagem imagem);
    IEnumerable<ClienteImagem> GetByClienteId(Guid clienteId);

    ClienteImagem? GetById(Guid id);// pipeline de processamento/detecção

    Task AddAsync(ClienteImagem imagem);
    Task<ClienteImagem?> GetByIdAsync(Guid id);
}
