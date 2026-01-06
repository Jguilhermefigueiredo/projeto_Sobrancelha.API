using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Infrastructure.Data;

namespace SombrancelhaApp.Api.Repositories;

public class ClienteImagemRepository : IClienteImagemRepository
{
    private readonly AppDbContext _context;

    public ClienteImagemRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(ClienteImagem imagem)
    {
        _context.ClienteImagens.Add(imagem);
        _context.SaveChanges();
    }

    public IEnumerable<ClienteImagem> GetByClienteId(Guid clienteId)
    {
        return _context.ClienteImagens
            .Where(i => i.ClienteId == clienteId)
            .OrderByDescending(i => i.CriadoEm)
            .ToList();
    }

    public ClienteImagem? GetById(Guid id)
{
    return _context.ClienteImagens
        .FirstOrDefault(i => i.Id == id);
}

}
