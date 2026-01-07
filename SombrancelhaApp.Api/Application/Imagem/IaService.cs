using System.Collections.Generic;
using System.Drawing;

namespace SombrancelhaApp.Api.Application.Imagem;

public class IaService : IIaService
{
    public List<Point> DetectarPontos(string caminhoImagem)
    {
        // Mock temporário para teste: Retorna pontos fictícios 
        // para que o sistema não quebre até a IA estar integrada
        return new List<Point>
        {
            new Point(100, 200),
            new Point(150, 180),
            new Point(200, 200)
        };
    }
}