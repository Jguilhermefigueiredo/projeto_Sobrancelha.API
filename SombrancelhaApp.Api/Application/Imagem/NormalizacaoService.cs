using System.Drawing;
using System.IO;

namespace SombrancelhaApp.Api.Application.Imagem;

public class NormalizacaoService : INormalizacaoService
{
    public void Normalizar(string caminhoOrigem, string caminhoDestino)
    {
        // Por enquanto, apenas copia o arquivo para não travar o fluxo
        // No futuro, aqui entrará o redimensionamento/ajuste de brilho
        File.Copy(caminhoOrigem, caminhoDestino, true);
    }
}