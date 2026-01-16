using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SombrancelhaApp.Api.Application.Imagem;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.DTOs;
using SombrancelhaApp.Api.Infrastructure.Data;
using System.Security.Claims;
using System.IO;

namespace SombrancelhaApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SimulacaoController : ControllerBase
{
    private readonly IProcessamentoImagemService _processamentoService;
    private readonly ISubstituicaoSobrancelhaService _substituicaoService;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public SimulacaoController(
        IProcessamentoImagemService processamentoService,
        ISubstituicaoSobrancelhaService substituicaoService,
        AppDbContext context,
        IWebHostEnvironment env)
    {
        _processamentoService = processamentoService;
        _substituicaoService = substituicaoService;
        _context = context;
        _env = env;
    }

    [HttpPost("processar")]
public async Task<IActionResult> Processar(
    IFormFile foto,
    [FromForm] Guid clienteId,
    [FromForm] string nomeMolde,
    [FromForm] string corHex = "#3B2F2F")
{
    if (foto == null || foto.Length == 0)
        return BadRequest("Nenhuma foto foi enviada.");

    var claimUsuarioId = User.FindFirst("UsuarioId")?.Value;
    if (string.IsNullOrEmpty(claimUsuarioId))
        return Unauthorized("Usuário não identificado no token.");

    var usuarioIdLogado = Guid.Parse(claimUsuarioId);

    try
    {
        // 1. Define o ponto de partida (Raiz da Storage)
        string storageRoot = Path.Combine(_env.ContentRootPath, "Storage");
        if (!Directory.Exists(storageRoot)) Directory.CreateDirectory(storageRoot);

        // 2. Salva a foto original na raiz (onde o original funciona)
        var nomeArquivoOriginal = $"{Guid.NewGuid()}_original{Path.GetExtension(foto.FileName)}";
        var caminhoOriginal = Path.Combine(storageRoot, nomeArquivoOriginal);

        using (var stream = new FileStream(caminhoOriginal, FileMode.Create))
        {
            await foto.CopyToAsync(stream);
        }

        // 3. Executa o processamento (que cria as subpastas Atendimentos/Data/Cliente)
        string caminhoFisicoFinal = _processamentoService.ProcessarFluxoCompleto(
            clienteId.ToString(),
            caminhoOriginal,
            nomeMolde,
            corHex
        );

        if (string.IsNullOrEmpty(caminhoFisicoFinal))
            return BadRequest("Falha ao gerar a simulação.");

        // 4. LÓGICA INTELIGENTE PARA URLs
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        // URL Original (Raiz)
        var urlOriginal = $"{baseUrl}/visualizar-imagens-atendimentos/{nomeArquivoOriginal}";

        // URL Simulação (DINÂMICA: Atendimentos/Data/Cliente/Arquivo.jpg)
        // Isso transforma o caminho do C:\ no caminho da Web
        string relativoFinal = Path.GetRelativePath(storageRoot, caminhoFisicoFinal)
                                   .Replace("\\", "/"); 

        var urlSimulacao = $"{baseUrl}/visualizar-imagens-atendimentos/{relativoFinal}";

        // 5. Salva no banco com a URL completa e correta
        var atendimento = new AtendimentoSimulacao
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId.ToString(),
            NomeMolde = nomeMolde,
            CorHex = corHex,
            CaminhoImagemFinal = caminhoFisicoFinal,
            UrlImagemFinal = urlSimulacao, // Agora com as subpastas no link!
            UsuarioId = usuarioIdLogado,
            DataCriacao = DateTime.Now,
            ConfirmadoParaDeletar = false
        };

        _context.AtendimentoSimulacoes.Add(atendimento);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            clienteId = clienteId,
            urlOriginal = urlOriginal,
            urlSimulacao = urlSimulacao,
            dataProcessamento = atendimento.DataCriacao,
            mensagem = "Simulação concluída com sucesso!"
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Erro: {ex.Message}");
    }
    }

    [HttpGet("moldes")]
    public IActionResult GetMoldes()
    {
        return Ok(_substituicaoService.ListarMoldesDisponiveis());
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
        if (simulacao == null) return NotFound();

        simulacao.ConfirmadoParaDeletar = true;
        await _context.SaveChangesAsync();
        return Ok(new { mensagem = "Exclusão autorizada." });
    }
}