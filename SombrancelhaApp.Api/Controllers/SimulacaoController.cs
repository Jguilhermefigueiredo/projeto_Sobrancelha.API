using Microsoft.AspNetCore.Mvc;
using SombrancelhaApp.Api.Application.Imagem;

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

    // Endpoint principal: Recebe a foto da câmera e retorna a simulação pronta.

    [HttpPost("processar")]
[Consumes("multipart/form-data")] //  Swagger mostra o botão de upload
public async Task<IActionResult> ProcessarFoto(
    IFormFile foto, // Sem o [FromForm] aqui evita confusão de mapeamento
    [FromForm] string clienteId, 
    [FromForm] string nomeMolde, 
    [FromForm] string corHex = "#3B2F2F")
    {
        if (foto == null || foto.Length == 0)
            return BadRequest("Nenhuma foto foi enviada.");

        try
        {
            // Salva temporariamente o upload inicial para processamento
            var tempPath = Path.GetTempFileName() + ".jpg";
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            // cuida das pastas Storage/Atendimentos/{Data}/{Id}
            string caminhoResultado = _processamentoService.ProcessarFluxoCompleto(
            clienteId,
            tempPath,
            nomeMolde,
            corHex
);

            // Retorna o arquivo final para o frontend
            var bytes = await System.IO.File.ReadAllBytesAsync(caminhoResultado);
            return File(bytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    //Retorna a lista de nomes de sobrancelhas disponíveis para o carrossel do front.

    [HttpGet("moldes")]
    public IActionResult GetMoldes()
    {
        var moldes = _substituicaoService.ListarMoldesDisponiveis();
        return Ok(moldes);
    }
}