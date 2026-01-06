namespace SombrancelhaApp.Api.Application.Imagem;

public class DeteccaoSobrancelhaService : IDeteccaoSobrancelhaService
{
    public ResultadoDeteccaoSobrancelha Detectar(string caminhoImagem)
    {
        if (!File.Exists(caminhoImagem))
        {
            return new ResultadoDeteccaoSobrancelha
            {
                Sucesso = false,
                Mensagem = "Imagem não encontrada"
            };
        }

        // 🔜 aqui entra OpenCV / MediaPipe / Dlib
        return new ResultadoDeteccaoSobrancelha
        {
            Sucesso = true,
            Mensagem = "Pipeline de detecção inicializado com sucesso",
            //CaminhoImagemProcessada = caminhoImagem
        };
    }
}
