using Microsoft.AspNetCore.Mvc;
using SombrancelhaApp.Api.DTOs;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Repositories;
using SombrancelhaApp.Api.Mappers;
using Microsoft.EntityFrameworkCore;


namespace SombrancelhaApp.Api.Controllers
{
    [ApiController]
    [Route("clientes")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteRepository _repository;

        public ClienteController(IClienteRepository repository)
        {
            _repository = repository;
        }

        // ===============================
        // POST /clientes
        // ===============================
         [HttpPost]
    public IActionResult Criar([FromBody] CreateClienteDto dto)
    {
        var cliente = new Cliente(
            dto.Nome,
            dto.Idade,
            dto.Telefone
        );

        _repository.Add(cliente);

        return CreatedAtRoute(
            "ObterClientePorId",
            new { id = cliente.Id },
            cliente.ToDto()
        );
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromQuery] ClienteQueryDto query)
    {
        var clientesQuery = _repository.Query();

        clientesQuery = query.OrderBy?.ToLower() switch
        {
            "nome" => clientesQuery.OrderBy(c => c.Nome),
            "idade" => clientesQuery.OrderBy(c => c.Idade),
            "criadoem" => clientesQuery.OrderBy(c => c.CriadoEm),
            _ => clientesQuery.OrderBy(c => c.Nome)
        };

        var totalItems = await clientesQuery.CountAsync();

        var clientes = await clientesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return Ok(new PagedResultDto<ClienteResponseDto>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize),
            Items = clientes.Select(c => c.ToDto())
        });
    }

    [HttpGet("{id:guid}", Name = "ObterClientePorId")]
    public IActionResult ObterPorId(Guid id)
    {
        var cliente = _repository.GetById(id);

        if (cliente == null)
            return NotFound();

        return Ok(cliente.ToDto());
    }


    //update
    [HttpPut("{id:guid}")]
    public IActionResult Atualizar(Guid id, [FromBody] UpdateClienteDto dto)
    {
        var cliente = _repository.GetById(id);

    if (cliente == null)
        return NotFound();

    cliente.Atualizar(
        dto.Nome,
        dto.Idade,
        dto.Telefone
    );

    _repository.Update(cliente);

    return Ok(cliente.ToDto());
    }

    //deletar
    [HttpDelete("{id:guid}")]
public IActionResult Remover(Guid id)
{
    var cliente = _repository.GetById(id);

    if (cliente == null)
        return NotFound();

    _repository.Delete(cliente);

    return NoContent();
}

}
}
