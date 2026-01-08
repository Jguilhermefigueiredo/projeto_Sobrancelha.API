using Microsoft.AspNetCore.Mvc;
using SombrancelhaApp.Api.Application.Imagem;
using SombrancelhaApp.Api.DTOs;
using System.IO;

namespace SombrancelhaApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulacaoController : ControllerBase
{
    private readonly IProcessamentoImagemService _processamentoService;
    private readonly ISubstituicaoSobrancelhaService _substituicaoService;

    public SimulacaoController(
        IProcessamentoImagemService processamentoService,
        ISubstituicaoSobrancelhaService substituicaoService)
    {
        _processamentoService = processamentoService;
        _substituicaoService = substituicaoService;
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

        try
        {
            // 1. Salva temporariamente o upload inicial
            var tempPath = Path.GetTempFileName() + ".jpg";
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            // 2. Executa o fluxo e recebe o caminho físico (C:\...\Storage\...)
            string caminhoFisicoFinal = _processamentoService.ProcessarFluxoCompleto(
                clienteId,
                tempPath,
                nomeMolde,
                corHex
            );

            // 3. Montagem da URL para o Front-end
            string nomeArquivo = Path.GetFileName(caminhoFisicoFinal);
            string dataPasta = DateTime.Now.ToString("yyyy-MM-dd");

            // RequestPath personalizado: /visualizar-imagens-atendimentos
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var urlRelativa = $"/visualizar-imagens-atendimentos/Atendimentos/{dataPasta}/{clienteId}/{nomeArquivo}".Replace("\\", "/");

            // 4. Retorno estruturado via DTO
            var resposta = new SimulacaoRespostaDto
            {
                ClienteId = clienteId,
                UrlImagemFinal = baseUrl + urlRelativa,
                DataProcessamento = DateTime.Now
            };

            return Ok(resposta);
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
        return Ok(moldes);
    }
}