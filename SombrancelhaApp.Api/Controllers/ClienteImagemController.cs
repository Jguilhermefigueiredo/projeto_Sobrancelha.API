using Microsoft.AspNetCore.Mvc;
using SombrancelhaApp.Api.DTOs;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Repositories;
using SombrancelhaApp.Api.Application.Imagem;

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

    [HttpPost]
    public IActionResult UploadImagem(Guid clienteId, [FromForm] UploadImagemClienteDto dto)
    {
        var cliente = _clienteRepository.GetById(clienteId);
        if (cliente == null) return NotFound("Cliente não encontrado");

        if (dto.Imagem == null || dto.Imagem.Length == 0)
            return BadRequest("Imagem inválida");

        var extensao = Path.GetExtension(dto.Imagem.FileName).ToLowerInvariant();
        if (extensao != ".jpg" && extensao != ".jpeg" && extensao != ".png")
            return BadRequest("Formato de imagem não suportado");

        var pasta = Path.Combine(_env.ContentRootPath, "Infrastructure", "Images", "clientes", clienteId.ToString());
        Directory.CreateDirectory(pasta);

        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
        var caminhoCompleto = Path.Combine(pasta, nomeArquivo);

        using (var stream = new FileStream(caminhoCompleto, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            dto.Imagem.CopyTo(stream);
            stream.Flush();
        }

        var imagem = new ClienteImagem(clienteId, caminhoCompleto);
        _clienteImagemRepository.Add(imagem);

        // Gera a URL acessível para a imagem original
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var urlAcessivel = $"{baseUrl}/visualizar-imagens/clientes/{clienteId}/{nomeArquivo}";

        return Created(string.Empty, new
        {
            imagem.Id,
            urlOriginal = urlAcessivel,
            imagem.CriadoEm
        });
    }

    [HttpGet]
    public IActionResult ListarImagens(Guid clienteId)
    {
        var cliente = _clienteRepository.GetById(clienteId);
        if (cliente == null) return NotFound("Cliente não encontrado");

        var imagens = _clienteImagemRepository.GetByClienteId(clienteId);
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var response = imagens.Select(i => new
        {
            i.Id,
            url = $"{baseUrl}/visualizar-imagens/clientes/{clienteId}/{Path.GetFileName(i.Caminho)}",
            i.CriadoEm
        });

        return Ok(response);
    }

    [HttpPost("{imagemId:guid}/detectar-sobrancelha")]
    public IActionResult DetectarSobrancelha(Guid clienteId, Guid imagemId, [FromBody] ProcessarSimulacaoDto dto)
    {
        var cliente = _clienteRepository.GetById(clienteId);
        if (cliente == null) return NotFound("Cliente não encontrado");

        var imagem = _clienteImagemRepository.GetById(imagemId);
        if (imagem == null) return NotFound("Imagem não encontrada");

        var resultado = _processamentoImagemService.ProcessarFluxoCompleto(imagem.Caminho, dto.NomeMolde);

        if (!resultado.Sucesso)
            return BadRequest(resultado.Mensagem);

        // --- LÓGICA DE URL ACESSÍVEL ---
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var nomeArquivoFinal = Path.GetFileName(resultado.CaminhoProcessado!);

        // Monta a URL que aponta para o middleware StaticFiles configurado no Program.cs
        var urlSimulacao = $"{baseUrl}/visualizar-imagens/clientes/{clienteId}/{nomeArquivoFinal}";

        return Ok(new
        {
            imagemOriginalId = imagem.Id,
            moldeAplicado = dto.NomeMolde,
            mensagem = "Simulação gerada com sucesso",
            urlSimulacao = urlSimulacao
        });
    }
}