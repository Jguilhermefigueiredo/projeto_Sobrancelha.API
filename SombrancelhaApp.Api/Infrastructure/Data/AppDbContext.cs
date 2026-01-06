using Microsoft.EntityFrameworkCore;
using SombrancelhaApp.Api.Domain;

namespace SombrancelhaApp.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

     //  NOVO DbSet (imagem do cliente)
    public DbSet<ClienteImagem> ClienteImagens => Set<ClienteImagem>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
