using System.Collections.Generic;
using System.Drawing;

namespace SombrancelhaApp.Api.Application.Imagem
{
    public interface ISubstituicaoSobrancelhaService
    {
        // Adiciona o hexColor
        string AplicarMolde(string caminhoImagemBase, string nomeMolde, List<Point> pontos, string hexColor = "#3B2F2F");


        List<string> ListarMoldesDisponiveis();
    }
}