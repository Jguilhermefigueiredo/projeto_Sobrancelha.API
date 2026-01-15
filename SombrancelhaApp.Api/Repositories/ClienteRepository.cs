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

    // 1. Métodos Assíncronos evitam que a API "trave" sob carga
    public async Task AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<Cliente?> GetByIdAsync(Guid id)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
    }

    // 2. A "Mágica" para o Swagger: Busca com Filtro e Paginação
    public async Task<(IEnumerable<Cliente> Items, int TotalCount)> GetPagedAsync(string? nome, int page, int pageSize)
    {
        var query = _context.Clientes.AsQueryable();

        // Filtro por nome (ignore maiúsculas/minúsculas)
        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(c => c.Nome.ToLower().Contains(nome.ToLower()));
        }

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Mantemos o Query para casos de uso específicos
    public IQueryable<Cliente> Query()
    {
        return _context.Clientes.AsNoTracking().AsQueryable();
    }
}