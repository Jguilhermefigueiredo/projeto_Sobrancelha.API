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

    public List<string> ListarMoldesDisponiveis()
    {
        var diretorioMoldes = Path.Combine(_env.ContentRootPath, "Infrastructure", "Assets", "Sobrancelhas");
        if (!Directory.Exists(diretorioMoldes)) return new List<string>();

        var arquivos = Directory.GetFiles(diretorioMoldes, "*.png");
        return arquivos.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
    }

    public string AplicarMolde(string caminhoImagemBase, string nomeMolde, List<Point> pontos, string hexColor = "#3B2F2F")
    {
        var caminhoMolde = Path.Combine(_env.ContentRootPath, "Infrastructure", "Assets", "Sobrancelhas", $"{nomeMolde}.png");

        using (var imagemBase = Image.Load<Rgba32>(caminhoImagemBase))
        using (var molde = Image.Load<Rgba32>(caminhoMolde))
        {
            // 1. Encontrar limites e centro (Bounding Box)
            int minX = pontos.Min(p => p.X);
            int maxX = pontos.Max(p => p.X);
            int minY = pontos.Min(p => p.Y);
            int maxY = pontos.Max(p => p.Y);

            int centroX = (minX + maxX) / 2;
            int centroY = (minY + maxY) / 2;
            int larguraRealIA = Math.Abs(maxX - minX);

            // 2. Identificação de lado baseada na FOTO
            var pInicio = pontos.First();
            var pFim = pontos.Last();
            bool estaNoLadoEsquerdoDaFoto = pInicio.X < (imagemBase.Width / 2);

            // 3. Geometria de inclinação
            double deltaX = pFim.X - pInicio.X;
            double deltaY = pFim.Y - pInicio.Y;
            float angulo = (float)(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));

            // 4. Transformação do Molde
            molde.Mutate(ctx =>
            {
                // Redimensiona baseado na detecção real
                ctx.Resize((int)(larguraRealIA * 1.15), 0);

                // LÓGICA DE ESPELHAMENTO CORRIGIDA
                // Se o asset original for o lado direito, espelhamos quando estiver na esquerda da foto
                if (estaNoLadoEsquerdoDaFoto) 
                {
                    ctx.Flip(FlipMode.Horizontal);
                }

                ctx.Rotate(angulo);
                AplicarCor(ctx, hexColor);
                ctx.GaussianBlur(0.5f);
            });

            // 5. POSICIONAMENTO CENTRALIZADO (Bounding Box)
            // Centraliza o molde no "meio" de onde a sobrancelha original estava
            int xFinal = centroX - (molde.Width / 2);
            int yFinal = centroY - (molde.Height / 2);

            // Segurança: Clamping
            xFinal = Math.Clamp(xFinal, 0, imagemBase.Width - molde.Width);
            yFinal = Math.Clamp(yFinal, 0, imagemBase.Height - molde.Height);

            // 6. Desenho e Salvamento
            imagemBase.Mutate(ctx => ctx.DrawImage(molde, new SixLabors.ImageSharp.Point(xFinal, yFinal), 0.90f));

            var diretorio = Path.GetDirectoryName(caminhoImagemBase)!;
            var caminhoFinal = Path.Combine(diretorio, $"final_{Guid.NewGuid()}.jpg");
            imagemBase.Save(caminhoFinal);

            return caminhoFinal;
        }
    }

    private void AplicarCor(IImageProcessingContext ctx, string hexColor)
    {
        var color = Color.ParseHex(hexColor);
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