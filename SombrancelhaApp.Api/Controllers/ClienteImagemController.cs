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
    public IActionResult DetectarSobrancelha(Guid clienteId, Guid imagemId, [FromBody] ProcessarSimulacaoDto dto)
    {
        var cliente = _clienteRepository.GetById(clienteId);
        if (cliente == null) return NotFound("Cliente não encontrado");

        var imagem = _clienteImagemRepository.GetById(imagemId);
        if (imagem == null) return NotFound("Imagem não encontrada");

        try 
        {
            // O serviço agora retorna uma STRING (caminho do arquivo final)
            // Passamos: clienteId.ToString(), caminhoOriginal, nomeMolde e corHex (se existir no DTO)
            var corParaAplicar = "#3B2F2F"; // Valor padrão ou venha de dto.CorHex  DTO
            
            var caminhoFinal = _processamentoImagemService.ProcessarFluxoCompleto(
                clienteId.ToString(), 
                imagem.Caminho, 
                dto.NomeMolde, 
                corParaAplicar
            );

            if (string.IsNullOrEmpty(caminhoFinal))
                return BadRequest("Não foi possível gerar a simulação.");

            // --- LÓGICA DE URL ACESSÍVEL ---
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var nomeArquivoFinal = Path.GetFileName(caminhoFinal);

            // Importante: Como o ProcessarFluxoCompleto agora salva na pasta Storage/Atendimentos,
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