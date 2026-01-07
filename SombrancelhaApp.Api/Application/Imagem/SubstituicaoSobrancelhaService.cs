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
                // Aumentamos levemente a escala para cobrir a área removida
                int larguraDesejada = (int)(Math.Sqrt(deltaX * deltaX + deltaY * deltaY) * 1.25);

                // TRANSFORMAÇÃO E ESTÉTICA
                molde.Mutate(ctx =>
                {
                    ctx.Resize(larguraDesejada, 0);

                    if (ehLadoEsquerdo) ctx.Flip(FlipMode.Horizontal);

                    ctx.Rotate(angulo);
                    AplicarCor(ctx, hexColor);
                    ctx.GaussianBlur(0.7f);
                });

                // CÁLCULO DE POSIÇÃO CALIBRADO (Evita o "efeito ombro")
                // Usamos o ponto inicial como âncora e centralizamos a altura do molde nele
                int xFinal, yFinal;

                if (ehLadoEsquerdo)
                {
                    // No lado esquerdo, o pInicio é o canto interno (perto do nariz)
                    xFinal = pInicio.X; 
                    yFinal = pInicio.Y - (molde.Height / 2);
                }
                else
                {
                    // No lado direito, subtraímos a largura para o molde crescer para a direita
                    xFinal = pInicio.X - molde.Width; 
                    yFinal = pInicio.Y - (molde.Height / 2);
                }

                // SEGURANÇA: Impede que o desenho saia da imagem (causa do erro de "não alteração")
                xFinal = Math.Clamp(xFinal, 0, imagemBase.Width - molde.Width);
                yFinal = Math.Clamp(yFinal, 0, imagemBase.Height - molde.Height);

                var posicaoDesenho = new SixLabors.ImageSharp.Point(xFinal, yFinal);

                // Blending
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