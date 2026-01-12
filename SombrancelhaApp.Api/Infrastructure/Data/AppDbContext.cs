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
    public DbSet<AtendimentoSimulacao> AtendimentoSimulacoes { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    modelBuilder.Entity<AtendimentoSimulacao>()
        .HasOne(s => s.Usuario)
        .WithMany()
        .HasForeignKey(s => s.UsuarioId)
        .OnDelete(DeleteBehavior.Restrict);
}
}
