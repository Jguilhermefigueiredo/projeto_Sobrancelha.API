using System.Collections.Generic;
using System.Drawing;

namespace SombrancelhaApp.Api.Application.Imagem;

public interface ISubstituicaoSobrancelhaService
{
    string AplicarMolde(string caminhoImagemBase, string nomeMolde, List<Point> pontos);
}