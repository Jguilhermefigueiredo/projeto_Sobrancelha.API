using System.Collections.Generic;
using System.Drawing;
namespace SombrancelhaApp.Api.Application.Imagem;

public interface IRemocaoSobrancelhaService
{
    // Recebe o caminho da imagem e uma lista de pontos (polígono da sobrancelha)
    string RemoverSobrancelha(string caminhoImagem, List<Point> pontosSobrancelha);
}