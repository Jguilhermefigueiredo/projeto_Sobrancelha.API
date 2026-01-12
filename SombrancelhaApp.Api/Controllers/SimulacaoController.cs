using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Necessário para [Authorize]
using SombrancelhaApp.Api.Application.Imagem;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.DTOs;
using SombrancelhaApp.Api.Infrastructure.Data;
using System.Security.Claims; // Necessário para Claims

namespace SombrancelhaApp.Api.Controllers;

[Authorize] // Bloqueia acesso anônimo para toda a classe
[ApiController]
[Route("api/[controller]")]
public class SimulacaoController : ControllerBase
{
    private readonly IProcessamentoImagemService _processamentoService;
    private readonly ISubstituicaoSobrancelhaService _substituicaoService;
    private readonly AppDbContext _context;

    public SimulacaoController(
        IProcessamentoImagemService processamentoService,
        ISubstituicaoSobrancelhaService substituicaoService,
        AppDbContext context)
    {
        _processamentoService = processamentoService;
        _substituicaoService = substituicaoService;
        _context = context;
    }

    [HttpPost("processar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ProcessarFoto(
        IFormFile foto, 
        [FromForm] string clienteId, 
        [FromForm] string nomeMolde, 
        [FromForm] string corHex = "#3B2F2F")
    {
        if (foto == null || foto.Length == 0)
            return BadRequest("Nenhuma foto foi enviada.");

        // 🔹 Captura o ID do Usuário Logado (Master ou Funcionário) do Token JWT
        var claimUsuarioId = User.FindFirst("UsuarioId")?.Value;
        if (string.IsNullOrEmpty(claimUsuarioId))
            return Unauthorized("Usuário não identificado no token.");

        var usuarioIdLogado = Guid.Parse(claimUsuarioId);

        try
        {
            var tempPath = Path.GetTempFileName() + ".jpg";
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            string caminhoFisicoFinal = _processamentoService.ProcessarFluxoCompleto(
                clienteId,
                tempPath,
                nomeMolde,
                corHex
            );

            string nomeArquivo = Path.GetFileName(caminhoFisicoFinal);
            string dataPasta = DateTime.Now.ToString("yyyy-MM-dd");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var urlRelativa = $"/visualizar-imagens-atendimentos/Atendimentos/{dataPasta}/{clienteId}/{nomeArquivo}".Replace("\\", "/");
            var urlCompleta = baseUrl + urlRelativa;

            // 🔹 Salva registro no banco de dados incluindo o Usuário autor da ação
            var atendimento = new AtendimentoSimulacao
            {
                ClienteId = clienteId,
                NomeMolde = nomeMolde,
                CorHex = corHex,
                CaminhoImagemFinal = caminhoFisicoFinal,
                UrlImagemFinal = urlCompleta,
                UsuarioId = usuarioIdLogado, // Vinculação com o RBAC
                DataCriacao = DateTime.Now
            };

            _context.AtendimentoSimulacoes.Add(atendimento);
            await _context.SaveChangesAsync();

            return Ok(new SimulacaoRespostaDto
            {
                ClienteId = clienteId,
                UrlImagemFinal = urlCompleta,
                DataProcessamento = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpGet("moldes")]
    public IActionResult GetMoldes()
    {
        var moldes = _substituicaoService.ListarMoldesDisponiveis();
        
        if (moldes == null || !moldes.Any())
            return NotFound("Nenhum molde de sobrancelha encontrado na pasta Assets.");

        return Ok(moldes);
    }

    [HttpGet("historico/{clienteId}")]
    public async Task<IActionResult> GetHistorico(string clienteId)
    {
        var historico = await _context.AtendimentoSimulacoes
            .Where(x => x.ClienteId == clienteId)
            .OrderByDescending(x => x.DataCriacao)
            .ToListAsync();

        return Ok(historico);
    }

    [HttpPatch("confirmar-limpeza/{id}")]
    public async Task<IActionResult> ConfirmarLimpeza(Guid id)
    {
        var simulacao = await _context.AtendimentoSimulacoes.FindAsync(id);

        if (simulacao == null)
            return NotFound("Simulação não encontrada.");

        simulacao.ConfirmadoParaDeletar = true;

        await _context.SaveChangesAsync();

        return Ok(new {
            mensagem = "Exclusão autorizada. O arquivo será removido fisicamente na próxima rotina de limpeza.",
            id = id,
            autorizadoEm = DateTime.Now
        });
    }
}