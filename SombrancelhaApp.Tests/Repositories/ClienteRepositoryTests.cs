using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Repositories;
using SombrancelhaApp.Tests.Helpers;
using Xunit;

namespace SombrancelhaApp.Tests.Repositories;

public class ClienteRepositoryTests
{
    [Fact]
    public void Add_DeveSalvarClienteNoBanco()
    {
        // Arrange
        var context = DbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var cliente = new Cliente("João", 30, "11999999999");

        // Act
        repository.Add(cliente);

        // Assert
        var clienteSalvo = context.Clientes.FirstOrDefault();
        Assert.NotNull(clienteSalvo);
        Assert.Equal("João", clienteSalvo!.Nome);
    }

    [Fact]
    public void GetById_DeveRetornarClienteQuandoExistir()
    {
        // Arrange
        var context = DbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var cliente = new Cliente("Maria", 25, "11888888888");
        repository.Add(cliente);

        // Act
        var resultado = repository.GetById(cliente.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(cliente.Id, resultado!.Id);
    }

    [Fact]
    public void GetAll_DeveRetornarListaDeClientes()
    {
        // Arrange
        var context = DbContextFactory.Create();
        var repository = new ClienteRepository(context);

        repository.Add(new Cliente("Ana", 22, "11777777777"));
        repository.Add(new Cliente("Carlos", 40, "11666666666"));

        // Act
        var clientes = repository.GetAll();

        // Assert
        Assert.Equal(2, clientes.Count());
    }

    [Fact]
    public void Delete_DeveRemoverCliente()
    {
        // Arrange
        var context = DbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var cliente = new Cliente("Pedro", 35, "11555555555");
        repository.Add(cliente);

        // Act
        repository.Delete(cliente);

        // Assert
        Assert.Empty(context.Clientes);
    }

    [Fact]
    public void Update_DeveAlterarDadosDoCliente()
    {
        // Arrange
        var context = DbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var cliente = new Cliente("Lucas", 28, "11444444444");
        repository.Add(cliente);

        cliente.Atualizar("Lucas Silva", 29, "11333333333");

        // Act
        repository.Update(cliente);

        // Assert
        var atualizado = repository.GetById(cliente.Id);
        Assert.Equal("Lucas Silva", atualizado!.Nome);
        Assert.Equal(29, atualizado.Idade);
    }
}
