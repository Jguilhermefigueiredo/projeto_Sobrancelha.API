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
        public async Task<IActionResult> Criar([FromBody] CreateClienteDto dto)
        {
            var cliente = new Cliente(
                dto.Nome,
                dto.Idade,
                dto.Telefone
            );

            // Correção: Adicionado await, modificado para AddAsync e adicionado ponto e vírgula
            await _repository.AddAsync(cliente);

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

            // Adicionando o filtro por nome para que apareça no Swagger
            if (!string.IsNullOrWhiteSpace(query.Nome))
            {
                clientesQuery = clientesQuery.Where(c => c.Nome.ToLower().Contains(query.Nome.ToLower()));
            }

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
        public async Task<IActionResult> ObterPorId(Guid id) // Adicionado async Task
        {
            var cliente = await _repository.GetByIdAsync(id);

            if (cliente == null)
                return NotFound();

            return Ok(cliente.ToDto());
        }

        // update
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateClienteDto dto) // Adicionado async Task
        {
            var cliente = await _repository.GetByIdAsync(id);

            if (cliente == null)
                return NotFound();

            cliente.Atualizar(
                dto.Nome,
                dto.Idade,
                dto.Telefone
            );

            await _repository.UpdateAsync(cliente);

            return Ok(cliente.ToDto());
        }

        // deletar
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Remover(Guid id) // Adicionado async Task
        {
            var cliente = await _repository.GetByIdAsync(id);

            if (cliente == null)
                return NotFound();

            await _repository.DeleteAsync(cliente);

            return NoContent();
        }
    }
}