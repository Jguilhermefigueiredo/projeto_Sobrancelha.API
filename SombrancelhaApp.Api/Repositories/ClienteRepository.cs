using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace SombrancelhaApp.Api.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Cliente cliente)
    {
        //Console.WriteLine("DB EM USO: " + _context.Database.GetDbConnection().DataSource);
        _context.Clientes.Add(cliente);
        _context.SaveChanges();
    }

    // Update persistido no banco
    public void Update(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        _context.SaveChanges();
    }
    //delete
    public void Delete(Cliente cliente)
    {
    _context.Clientes.Remove(cliente);
    _context.SaveChanges();
    }

    

    public Cliente? GetById(Guid id)
    {
        return _context.Clientes.FirstOrDefault(c => c.Id == id);
    }

    public IEnumerable<Cliente> GetAll()
    {
        return _context.Clientes.ToList();
    }

    public IQueryable<Cliente> Query()
    {
        return _context.Clientes.AsQueryable();
    }
}
