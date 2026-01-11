using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SombrancelhaApp.Api.Infrastructure.Data;

namespace SombrancelhaApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("estatisticas")]
    public async Task<IActionResult> GetEstatisticas()
    {
        var hoje = DateTime.Today;

        // 1. Total de simulações hoje
        var totalHoje = await _context.AtendimentoSimulacoes
            .CountAsync(s => s.DataCriacao.Date == hoje);

        // 2. Molde mais usado (Top 3)
        var moldesMaisUsados = await _context.AtendimentoSimulacoes
            .GroupBy(s => s.NomeMolde)
            .Select(g => new 
            { 
                Nome = g.Key, 
                Quantidade = g.Count() 
            })
            .OrderByDescending(x => x.Quantidade)
            .Take(3)
            .ToListAsync();

        // 3. Cores mais pedidas (Top 3)
        var coresPopulares = await _context.AtendimentoSimulacoes
            .GroupBy(s => s.CorHex)
            .Select(g => new 
            { 
                Cor = g.Key, 
                Quantidade = g.Count() 
            })
            .OrderByDescending(x => x.Quantidade)
            .Take(3)
            .ToListAsync();

        return Ok(new
        {
            SimulacoesHoje = totalHoje,
            RankingMoldes = moldesMaisUsados,
            RankingCores = coresPopulares,
            TotalGeralNoBanco = await _context.AtendimentoSimulacoes.CountAsync()
        });
    }
}