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

    public string AplicarMolde(string caminhoImagemBase, string nomeMolde, List<Point> pontos)
    {
        var caminhoMolde = Path.Combine(_env.ContentRootPath, "Infrastructure", "Assets", "Sobrancelhas", $"{nomeMolde}.png");

        using (var imagemBase = Image.Load<Rgba32>(caminhoImagemBase))
        {
            using (var molde = Image.Load<Rgba32>(caminhoMolde))
            {
                // 1. Identifica o lado 
                bool ehLadoEsquerdo = pontos[0].X < (imagemBase.Width / 2);

                // 2. Lógica de geometria
                var pInicio = pontos.First(); 
                var pFim = pontos.Last();    

                double deltaX = pFim.X - pInicio.X;
                double deltaY = pFim.Y - pInicio.Y;

                float angulo = (float)(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
                int larguraDesejada = (int)(Math.Sqrt(deltaX * deltaX + deltaY * deltaY) * 1.2);

                // 3. TRANSFORMAÇÃO DO MOLDE (Deve vir ANTES do cálculo de posição)
                molde.Mutate(ctx =>
                {
                    ctx.Resize(larguraDesejada, 0);

                    if (ehLadoEsquerdo)
                    {
                        ctx.Flip(FlipMode.Horizontal);
                    }

                    ctx.Rotate(angulo);
                });

                // 4. CÁLCULO DE POSIÇÃO (Agora com o molde já transformado)
                int centroX = (pInicio.X + pFim.X) / 2;
                int centroY = (pInicio.Y + pFim.Y) / 2;

                // --- ESTRATÉGIA DE ANCORAGEM PELA BASE ---
                // Usamos molde.Height (altura total) para encostar a base do asset na linha da IA.
                // Aumente este valor (ex: 15, 20, 30) para fazer a sobrancelha DESCER.
                int ajusteFinoY = 45; 

                var posicaoDesenho = new SixLabors.ImageSharp.Point(
                    centroX - (molde.Width / 2),
                    centroY - molde.Height + ajusteFinoY
                );

                // 5. Blending
                imagemBase.Mutate(ctx => ctx.DrawImage(molde, posicaoDesenho, 0.8f));

                // 6. Salvamento
                var diretorio = Path.GetDirectoryName(caminhoImagemBase)!;
                var caminhoFinal = Path.Combine(diretorio, $"final_{Guid.NewGuid()}.jpg");

                imagemBase.Save(caminhoFinal);

                return caminhoFinal;
            }
        }
    }
}