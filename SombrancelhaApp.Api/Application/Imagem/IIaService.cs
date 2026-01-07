using System.Collections.Generic;
using System.Drawing;

namespace SombrancelhaApp.Api.Application.Imagem;

public interface IIaService
{
    // Define o contrato para detecção dos pontos da sobrancelha
    List<Point> DetectarPontos(string caminhoImagem);
}