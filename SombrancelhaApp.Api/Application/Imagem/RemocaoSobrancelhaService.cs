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
        // Ajustado para receber caminhoSaida do orquestrador
        public string RemoverSobrancelha(string caminhoImagem, List<System.Drawing.Point> pontosSobrancelha, string caminhoSaida)
        {
            if (pontosSobrancelha == null || !pontosSobrancelha.Any())
                throw new ArgumentException("Nenhum ponto de detecção fornecido.");

            using var src = Cv2.ImRead(caminhoImagem);
            if (src.Empty()) throw new Exception("Não foi possível carregar a imagem.");

            // Máscara iniciada em preto
            using var mask = new Mat(src.Size(), MatType.CV_8UC1, Scalar.Black);

            // Converter pontos
            var cvPoints = pontosSobrancelha.Select(p => new OpenCvSharp.Point(p.X, p.Y)).ToArray();

            // evitar "afundar" a testa
            Cv2.Polylines(mask, new[] { cvPoints }, isClosed: false, color: Scalar.White, thickness: 8);

            // Dilatação elíptica para formas orgânicas
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
            Cv2.Dilate(mask, mask, kernel);

            // DESFOQUE AGRESSIVO cria o efeito de 'feather' (esfumaçado) nas bordas
            Cv2.GaussianBlur(mask, mask, new Size(5, 5), 0);

            // Inpainting (Remoção inteligente)
            using var result = new Mat();
            Cv2.Inpaint(src, mask, result, 7, InpaintMethod.Telea);

            // Persistência usando o caminho definido pelo orquestrador
            result.ImWrite(caminhoSaida);

            return caminhoSaida;
        }
    }
}