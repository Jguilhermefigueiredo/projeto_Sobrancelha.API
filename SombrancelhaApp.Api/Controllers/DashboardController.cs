using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SombrancelhaApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace SombrancelhaApp.Api.Controllers;

[Authorize(Roles = "Master")]
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

        // Total de simulações hoje
        var totalHoje = await _context.AtendimentoSimulacoes
            .CountAsync(s => s.DataCriacao.Date == hoje);

        // Molde mais usado (Top 3)
        var moldesMaisUsados = await _context.AtendimentoSimulacoes
            .GroupBy(s => s.NomeMolde)
            .Select(g => new { Nome = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(3)
            .ToListAsync();

        // Cores mais pedidas (Top 3)
        var coresPopulares = await _context.AtendimentoSimulacoes
            .GroupBy(s => s.CorHex)
            .Select(g => new { Cor = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(3)
            .ToListAsync();

        // Produção por Equipe
        // Precisamos do .Include(s => s.Usuario) para acessar o Nome do funcionário
        var producaoEquipe = await _context.AtendimentoSimulacoes
            .Include(s => s.Usuario)
            .GroupBy(s => s.Usuario.Nome)
            .Select(g => new 
            { 
                Funcionario = g.Key ?? "Usuário Removido", 
                Quantidade = g.Count() 
            })
            .ToListAsync();

        return Ok(new
        {
            SimulacoesHoje = totalHoje,
            RankingMoldes = moldesMaisUsados,
            RankingCores = coresPopulares,
            ProducaoEquipe = producaoEquipe, // Exibe quanto cada um trabalhou
            TotalGeralNoBanco = await _context.AtendimentoSimulacoes.CountAsync()
        });
    }
}