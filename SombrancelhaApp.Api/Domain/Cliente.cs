namespace SombrancelhaApp.Api.Domain;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public int Idade { get; private set; }
    public string Telefone { get; private set; } = null!;
    public DateTime CriadoEm { get; private set; }

    protected Cliente() { }

    public Cliente(string nome, int idade, string telefone)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Idade = idade;
        Telefone = telefone;
        CriadoEm = DateTime.UtcNow;
    }

      // regra de atualização controlada pelo domínio
    public void Atualizar(string nome, int idade, string telefone)
    {
        Nome = nome;
        Idade = idade;
        Telefone = telefone;
    }
}
