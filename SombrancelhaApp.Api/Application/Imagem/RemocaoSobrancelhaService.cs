using OpenCvSharp;
using SombrancelhaApp.Api.Application.Imagem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SombrancelhaApp.Api.Application.Imagem
{
    public class RemocaoSobrancelhaService : IRemocaoSobrancelhaService
    {
        public string RemoverSobrancelha(string caminhoImagem, List<System.Drawing.Point> pontosSobrancelha)
        {
            if (pontosSobrancelha == null || !pontosSobrancelha.Any())
                throw new ArgumentException("Nenhum ponto de detecção fornecido.");

            using var src = Cv2.ImRead(caminhoImagem);
            if (src.Empty()) throw new Exception("Não foi possível carregar a imagem.");

            // Máscara iniciada em preto
            using var mask = new Mat(src.Size(), MatType.CV_8UC1, Scalar.Black);

            // Converter pontos
            var cvPoints = pontosSobrancelha.Select(p => new OpenCvSharp.Point(p.X, p.Y)).ToArray();

            // --- AJUSTES DE SUAVIZAÇÃO ---
            
            // 1. Espessura reduzida para evitar "afundar" a testa
            Cv2.Polylines(mask, new[] { cvPoints }, isClosed: false, color: Scalar.White, thickness: 8);
            // 2. Dilatação elíptica para formas orgânicas
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
            Cv2.Dilate(mask, mask, kernel);

            // 3. DESFOQUE AGRESSIVO: Aumentado para 31x31 para eliminar a "linha" divisória
            // Isso cria o efeito de 'feather' (esfumaçado) nas bordas
            Cv2.GaussianBlur(mask, mask, new Size(5, 5), 0);
            // 4. Inpainting (Remoção inteligente)
            using var result = new Mat();
            Cv2.Inpaint(src, mask, result, 2, InpaintMethod.Telea);

            // Persistência
            var diretorio = Path.GetDirectoryName(caminhoImagem);
            var nomeArquivo = $"limpa_{Guid.NewGuid()}.jpg";
            var caminhoDestino = Path.Combine(diretorio!, nomeArquivo);

            result.ImWrite(caminhoDestino);

            return caminhoDestino;
        }
    }
}