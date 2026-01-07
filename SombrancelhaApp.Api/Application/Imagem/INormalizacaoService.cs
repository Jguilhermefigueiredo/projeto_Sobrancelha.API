using System.Drawing;

namespace SombrancelhaApp.Api.Application.Imagem;

public interface INormalizacaoService
{
    // Define o contrato para padronizar o tamanho da imagem para a IA
    void Normalizar(string caminhoOrigem, string caminhoDestino);
}