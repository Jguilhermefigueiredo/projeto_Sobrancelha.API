using DlibDotNet;
using System.Drawing;
using Point = System.Drawing.Point;

namespace SombrancelhaApp.Api.Application.Imagem;

public class DeteccaoFacialService : IDeteccaoFacialService
{
    private readonly string _modelPath;

    public DeteccaoFacialService()
    {
        // Define o caminho do modelo
        _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "shape_predictor_68_face_landmarks.dat");

        if (!File.Exists(_modelPath))
        {
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "shape_predictor_68_face_landmarks.dat");
        }
    }

    public List<Point> ObterPontosSobrancelhaEsquerda(string caminhoImagem)
    {
        var pontosResultados = new List<Point>();

        if (!File.Exists(_modelPath))
            throw new FileNotFoundException($"Modelo da IA não encontrado em: {_modelPath}");

        // Detector de faces e preditor de pontos
        using var fd = Dlib.GetFrontalFaceDetector();
        using var sp = ShapePredictor.Deserialize(_modelPath);

        // Carrega a imagem
        using var img = Dlib.LoadImage<RgbPixel>(caminhoImagem);

        // Detecta faces
        var faces = fd.Operator(img);

        if (faces.Length > 0)
        {
            // Pega os pontos do primeiro rosto detectado
            using var shape = sp.Detect(img, faces[0]);

            // Extrai pontos 17 a 21 (Sobrancelha Esquerda)
            for (uint i = 17; i <= 21; i++)
            {
                var p = shape.GetPart(i);
                pontosResultados.Add(new Point((int)p.X, (int)p.Y));
            }
        }

        return pontosResultados;
    }
        public ResultadoPontosFaciais DetectarSobrancelhas(string caminhoImagem)
{
    var pontosEsquerda = new List<Point>();
    var pontosDireita = new List<Point>();

    using var fd = Dlib.GetFrontalFaceDetector();
    using var sp = ShapePredictor.Deserialize(_modelPath);
    using var img = Dlib.LoadImage<RgbPixel>(caminhoImagem);

    var faces = fd.Operator(img);

    if (faces.Length > 0)
    {
        using var shape = sp.Detect(img, faces[0]);

        // Pontos 17-21: Sobrancelha Esquerda
        for (uint i = 17; i <= 21; i++)
        {
            var p = shape.GetPart(i);
            pontosEsquerda.Add(new Point((int)p.X, (int)p.Y));
        }

        // Pontos 22-26: Sobrancelha Direita
        for (uint i = 22; i <= 26; i++)
        {
            var p = shape.GetPart(i);
            pontosDireita.Add(new Point((int)p.X, (int)p.Y));
        }
    }

    return new ResultadoPontosFaciais(pontosEsquerda, pontosDireita);
    }
}