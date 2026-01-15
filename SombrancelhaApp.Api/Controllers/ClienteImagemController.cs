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

    /// <summary>
    /// Realiza o upload de uma foto para um cliente específico.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")] //formulário de arquivo
    public async Task<IActionResult> Upload(Guid clienteId, IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0) 
            return BadRequest("O arquivo de imagem é obrigatório.");

        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null) 
            return NotFound("Cliente não encontrado.");

        // Define e garante a existência do diretório de armazenamento
        string storagePath = Path.Combine(_env.ContentRootPath, "Storage");
        if (!Directory.Exists(storagePath)) 
            Directory.CreateDirectory(storagePath);

        // Gera um nome único para evitar conflitos de arquivos
        var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(arquivo.FileName)}";
        var caminhoCompleto = Path.Combine(storagePath, nomeArquivo);

        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        // Persiste a referência da imagem no banco de dados
        var novaImagem = new ClienteImagem 
{ 
        Id = Guid.NewGuid(),
        ClienteId = clienteId,
        Caminho = caminhoCompleto,
        CriadoEm = DateTime.UtcNow
};

        // Nota: Certifique-se que seu repositório possui o método AddAsync
        await _clienteImagemRepository.AddAsync(novaImagem);

        return Ok(new 
        { 
            imagemId = novaImagem.Id, 
            mensagem = "Upload realizado com sucesso.",
            nomeArquivo = nomeArquivo
        });
    }

    /// <summary>
    /// Processa a simulação de sobrancelha em uma imagem já existente.
    /// </summary>
    [HttpPost("{imagemId:guid}/detectar-sobrancelha")]
    public async Task<IActionResult> DetectarSobrancelha(Guid clienteId, Guid imagemId, [FromBody] ProcessarSimulacaoDto dto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null) return NotFound("Cliente não encontrado.");

        var imagem = await _clienteImagemRepository.GetByIdAsync(imagemId);
        if (imagem == null) return NotFound("Imagem não encontrada.");

        try 
        {
            // Cor padrão para a aplicação (pode ser customizada via DTO futuramente)
            var corParaAplicar = "#3B2F2F"; 
            
            var caminhoFinal = _processamentoImagemService.ProcessarFluxoCompleto(
                clienteId.ToString(), 
                imagem.Caminho, 
                dto.NomeMolde, 
                corParaAplicar
            );

            if (string.IsNullOrEmpty(caminhoFinal))
                return BadRequest("Falha ao gerar a simulação da sobrancelha.");

            // Constrói a URL pública para acesso à imagem processada
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var nomeArquivoFinal = Path.GetFileName(caminhoFinal);
            var urlSimulacao = $"{baseUrl}/visualizar-imagens-atendimentos/{nomeArquivoFinal}";

            return Ok(new
            {
                imagemOriginalId = imagem.Id,
                moldeAplicado = dto.NomeMolde,
                urlSimulacao = urlSimulacao,
                mensagem = "Simulação concluída com sucesso."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno durante o processamento: {ex.Message}");
        }
    }
}