using SombrancelhaApp.Api.Domain;

namespace SombrancelhaApp.Api.Repositories;

public interface IClienteRepository
{
    void Add(Cliente cliente);
    void Update(Cliente cliente);
    void Delete(Cliente cliente);
    Cliente? GetById(Guid id);
    IEnumerable<Cliente> GetAll();
    IQueryable<Cliente> Query();
}
