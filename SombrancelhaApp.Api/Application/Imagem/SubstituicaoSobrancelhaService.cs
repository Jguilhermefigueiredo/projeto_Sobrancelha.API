using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Point = System.Drawing.Point;

namespace SombrancelhaApp.Api.Application.Imagem;

public class SubstituicaoSobrancelhaService : ISubstituicaoSobrancelhaService
{
    private readonly IWebHostEnvironment _env;

    public SubstituicaoSobrancelhaService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Lista todos os moldes (PNG) disponíveis na pasta de assets.
    /// Resolve o erro CS0535 de falta de implementação.
    /// </summary>
    public List<string> ListarMoldesDisponiveis()
    {
        var diretorioMoldes = Path.Combine(_env.ContentRootPath, "Infrastructure", "Assets", "Sobrancelhas");

        if (!Directory.Exists(diretorioMoldes))
            return new List<string>();

        var arquivos = Directory.GetFiles(diretorioMoldes, "*.png");
        return arquivos.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
    }

    public string AplicarMolde(string caminhoImagemBase, string nomeMolde, List<Point> pontos, string hexColor = "#3B2F2F")
    {
        var caminhoMolde = Path.Combine(_env.ContentRootPath, "Infrastructure", "Assets", "Sobrancelhas", $"{nomeMolde}.png");

        using (var imagemBase = Image.Load<Rgba32>(caminhoImagemBase))
        {
            using (var molde = Image.Load<Rgba32>(caminhoMolde))
            {
                // Identifica o lado
                bool ehLadoEsquerdo = pontos[0].X < (imagemBase.Width / 2);

                // Lógica de geometria
                var pInicio = pontos.First();
                var pFim = pontos.Last();

                double deltaX = pFim.X - pInicio.X;
                double deltaY = pFim.Y - pInicio.Y;

                float angulo = (float)(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
                int larguraDesejada = (int)(Math.Sqrt(deltaX * deltaX + deltaY * deltaY) * 1.2);

                // TRANSFORMAÇÃO E ESTÉTICA
                molde.Mutate(ctx =>
                {
                    // Redimensionamento proporcional
                    ctx.Resize(larguraDesejada, 0);

                    // Espelhamento se necessário
                    if (ehLadoEsquerdo) ctx.Flip(FlipMode.Horizontal);

                    // Rotação anatômica
                    ctx.Rotate(angulo);

                    // (Tingimento)
                    AplicarCor(ctx, hexColor);

                    // Feathering
                    ctx.GaussianBlur(0.7f);
                });

                // CÁLCULO DE POSIÇÃO (Ancoragem pela Base)
                int centroX = (pInicio.X + pFim.X) / 2;
                int centroY = (pInicio.Y + pFim.Y) / 2;

                int ajusteFinoY = 45;

                var posicaoDesenho = new SixLabors.ImageSharp.Point(
                    centroX - (molde.Width / 2),
                    centroY - molde.Height + ajusteFinoY
                );

                // Blending (Sobreposição com opacidade controlada)
                imagemBase.Mutate(ctx => ctx.DrawImage(molde, posicaoDesenho, 0.92f));

                // Salvamento
                var diretorio = Path.GetDirectoryName(caminhoImagemBase)!;
                var caminhoFinal = Path.Combine(diretorio, $"final_{Guid.NewGuid()}.jpg");

                imagemBase.Save(caminhoFinal);

                return caminhoFinal;
            }
        }
    }


    private void AplicarCor(IImageProcessingContext ctx, string hexColor)
{
    // Converte a string Hex para Color
    var color = Color.ParseHex(hexColor);
    // Converte a Color para Rgba32 para extrair os valores numéricos de 0 a 1
    Rgba32 rgba = color;
    float r = rgba.R / 255f;
    float g = rgba.G / 255f;
    float b = rgba.B / 255f;

    var matrix = new ColorMatrix(
        r, 0, 0, 0,
        0, g, 0, 0,
        0, 0, b, 0,
        0, 0, 0, 1,
        0, 0, 0, 0
    );

    ctx.Filter(matrix);
}


}