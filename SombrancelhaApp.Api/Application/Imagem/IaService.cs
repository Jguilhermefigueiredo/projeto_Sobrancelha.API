using DlibDotNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Point = System.Drawing.Point;

namespace SombrancelhaApp.Api.Application.Imagem;

public class IaService : IIaService
{
    private readonly string _caminhoModelo;

    public IaService(IWebHostEnvironment env)
    {
        // Ajustado para a pasta "Models" conforme sua localização atual
        _caminhoModelo = Path.Combine(env.ContentRootPath, "Models", "shape_predictor_68_face_landmarks.dat");
        
        // Log preventivo para você ver no terminal onde ele está procurando se falhar
        if (!File.Exists(_caminhoModelo))
        {
            Console.WriteLine($"[IA] ALERTA: Arquivo não encontrado em: {_caminhoModelo}");
        }
    }

    public List<Point> DetectarPontos(string caminhoImagem)
    {
        if (!File.Exists(_caminhoModelo))
            throw new FileNotFoundException($"Modelo da IA não encontrado no caminho: {_caminhoModelo}");

        using var fd = Dlib.GetFrontalFaceDetector();
        using var sp = ShapePredictor.Deserialize(_caminhoModelo);

        Console.WriteLine($"[IA] Analisando imagem: {Path.GetFileName(caminhoImagem)}...");
        
        using var img = Dlib.LoadImage<RgbPixel>(caminhoImagem);

        var faces = fd.Operator(img);
        var face = faces.FirstOrDefault();

        if (face == null) 
        {
            Console.WriteLine("[IA] ERRO: Nenhum rosto detectado!");
            throw new System.Exception("Nenhum rosto detectado na imagem.");
        }

        Console.WriteLine($"[IA] Rosto encontrado na posição: L:{face.Left} T:{face.Top} R:{face.Right} B:{face.Bottom}");

        using var shape = sp.Detect(img, face);
        var pontosSobrancelha = new List<Point>();

        // Mapeamento IBUG: 17 a 21 (Esquerda), 22 a 26 (Direita)
        for (uint i = 17; i <= 26; i++)
        {
            var p = shape.GetPart(i);
            var pontoConvertido = new Point((int)p.X, (int)p.Y);
            pontosSobrancelha.Add(pontoConvertido);

            if (i == 17) Console.WriteLine($"[IA] Sobrancelha Esquerda (Início): {pontoConvertido.X}, {pontoConvertido.Y}");
            if (i == 22) Console.WriteLine($"[IA] Sobrancelha Direita (Início): {pontoConvertido.X}, {pontoConvertido.Y}");
        }

        return pontosSobrancelha;
    }
}