using Microsoft.AspNetCore.Mvc;
using SombrancelhaApp.Api.DTOs;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Repositories;
using SombrancelhaApp.Api.Application.Imagem;
using System.IO;

namespace SombrancelhaApp.Api.Controllers;

[ApiController]
[Route("clientes/{clienteId:guid}/imagem")]
public class ClienteImagemController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IClienteImagemRepository _clienteImagemRepository;
    private readonly IProcessamentoImagemService _processamentoImagemService;
    private readonly IWebHostEnvironment _env;

    public ClienteImagemController(
        IClienteRepository clienteRepository,
        IClienteImagemRepository clienteImagemRepository,
        IProcessamentoImagemService processamentoImagemService,
        IWebHostEnvironment env)
    {
        _clienteRepository = clienteRepository;
        _clienteImagemRepository = clienteImagemRepository;
        _processamentoImagemService = processamentoImagemService;
        _env = env;
    }

    [HttpPost("{imagemId:guid}/detectar-sobrancelha")]
    // CORREÇÃO 1: Adicionado 'async Task<'
    public async Task<IActionResult> DetectarSobrancelha(Guid clienteId, Guid imagemId, [FromBody] ProcessarSimulacaoDto dto)
    {
        // CORREÇÃO 2: Alterado de _repository para _clienteRepository e adicionado await
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null) return NotFound("Cliente não encontrado");

        // Assumindo que o repositório de imagens ainda é síncrono. 
        // Se der erro, mude para: await _clienteImagemRepository.GetByIdAsync(imagemId);
        var imagem = _clienteImagemRepository.GetById(imagemId);
        if (imagem == null) return NotFound("Imagem não encontrada");

        try 
        {
            var corParaAplicar = "#3B2F2F"; 
            
            // Este serviço parece ser síncrono (processamento de CPU), então não precisa de await
            var caminhoFinal = _processamentoImagemService.ProcessarFluxoCompleto(
                clienteId.ToString(), 
                imagem.Caminho, 
                dto.NomeMolde, 
                corParaAplicar
            );

            if (string.IsNullOrEmpty(caminhoFinal))
                return BadRequest("Não foi possível gerar a simulação.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var nomeArquivoFinal = Path.GetFileName(caminhoFinal);

            var urlSimulacao = $"{baseUrl}/visualizar-imagens/atendimentos/{clienteId}/{nomeArquivoFinal}";

            return Ok(new
            {
                imagemOriginalId = imagem.Id,
                moldeAplicado = dto.NomeMolde,
                mensagem = "Simulação gerada com sucesso",
                urlSimulacao = urlSimulacao
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro no processamento: {ex.Message}");
        }
    }
}