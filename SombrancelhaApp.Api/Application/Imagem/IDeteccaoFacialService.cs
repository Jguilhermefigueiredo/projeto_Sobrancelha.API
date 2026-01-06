using System.Drawing;

namespace SombrancelhaApp.Api.Application.Imagem;

// Objeto para transportar os pontos das duas sobrancelhas
public record ResultadoPontosFaciais(
    List<Point> SobrancelhaEsquerda,
    List<Point> SobrancelhaDireita
);

public interface IDeteccaoFacialService
{
    // O nome deve ser exatamente este para o ProcessamentoImagemService encontrar
    ResultadoPontosFaciais DetectarSobrancelhas(string caminhoImagem);
}