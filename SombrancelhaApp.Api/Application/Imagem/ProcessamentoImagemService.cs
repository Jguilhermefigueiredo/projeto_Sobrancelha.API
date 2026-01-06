using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace SombrancelhaApp.Api.Application.Imagem;

public class ProcessamentoImagemService : IProcessamentoImagemService
{
    private readonly IRemocaoSobrancelhaService _remocaoService;
    private readonly ISubstituicaoSobrancelhaService _substituicaoService;
    private readonly IDeteccaoFacialService _deteccaoFacialService;

    public ProcessamentoImagemService(
        IRemocaoSobrancelhaService remocaoService,
        ISubstituicaoSobrancelhaService substituicaoService,
        IDeteccaoFacialService deteccaoFacialService)
    {
        _remocaoService = remocaoService;
        _substituicaoService = substituicaoService;
        _deteccaoFacialService = deteccaoFacialService;
    }

    public ResultadoProcessamentoImagem ProcessarFluxoCompleto(string caminhoImagem, string nomeMolde)
    {
        if (!AguardarArquivoFicarDisponivel(caminhoImagem))
        {
            return ResultadoProcessamentoImagem.Falha("O arquivo original está sendo usado por outro processo.");
        }

        var resultadoNormalizacao = Normalizar(caminhoImagem);
        if (!resultadoNormalizacao.Sucesso) return resultadoNormalizacao;

        try
        {
            // 3. OBTENÇÃO DOS PONTOS (IA)
            var pontosDeteccao = _deteccaoFacialService.DetectarSobrancelhas(resultadoNormalizacao.CaminhoProcessado!);

            if (pontosDeteccao.SobrancelhaEsquerda.Count == 0 && pontosDeteccao.SobrancelhaDireita.Count == 0)
            {
                return ResultadoProcessamentoImagem.Falha("Não foi possível detectar as sobrancelhas.");
            }

            // pontos para o OpenCV limpar ambos os lados
            var todosOsPontos = pontosDeteccao.SobrancelhaEsquerda.Concat(pontosDeteccao.SobrancelhaDireita).ToList();

            // 4. REMOÇÃO
            var caminhoImagemSemSobrancelha = _remocaoService.RemoverSobrancelha(
                resultadoNormalizacao.CaminhoProcessado!, 
                todosOsPontos
            );

            if (!File.Exists(caminhoImagemSemSobrancelha))
                return ResultadoProcessamentoImagem.Falha("Erro: O OpenCV não gerou a imagem limpa.");

            // 5. SUBSTITUIÇÃO (Aplicação dos moldes)
            // Lado Esquerdo (será espelhado pelo service pois o asset é direito)
            var imgComEsquerda = _substituicaoService.AplicarMolde(
                caminhoImagemSemSobrancelha, 
                nomeMolde, 
                pontosDeteccao.SobrancelhaEsquerda
            );

            // Lado Direito (usará asset original)
            var caminhoImagemFinal = _substituicaoService.AplicarMolde(
                imgComEsquerda, 
                nomeMolde, 
                pontosDeteccao.SobrancelhaDireita
            );

            return ResultadoProcessamentoImagem.Ok(caminhoImagemFinal);
        }
        catch (Exception ex)
        {
            // O fechamento do bloco try e o catch
            return ResultadoProcessamentoImagem.Falha($"Erro no processamento: {ex.Message}");
        }
    }

    public ResultadoProcessamentoImagem Normalizar(string caminhoImagem)
    {
        try
        {
            using var image = Image.Load(caminhoImagem);
            image.Mutate(x =>
            {
                x.AutoOrient();
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(512, 512)
                });
            });

            var pasta = Path.GetDirectoryName(caminhoImagem)!;
            var nomeArquivo = $"normalizada_{Guid.NewGuid()}.jpg";
            var caminhoFinal = Path.Combine(pasta, nomeArquivo);

            image.Save(caminhoFinal, new JpegEncoder { Quality = 90 });
            return ResultadoProcessamentoImagem.Ok(caminhoFinal);
        }
        catch (Exception ex)
        {
            return ResultadoProcessamentoImagem.Falha($"Erro ao normalizar: {ex.Message}");
        }
    }

    private bool AguardarArquivoFicarDisponivel(string caminho)
    {
        int tentativas = 0;
        while (tentativas < 10)
        {
            try
            {
                using var fs = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                tentativas++;
                Thread.Sleep(200);
            }
        }
        return false;
    }
}