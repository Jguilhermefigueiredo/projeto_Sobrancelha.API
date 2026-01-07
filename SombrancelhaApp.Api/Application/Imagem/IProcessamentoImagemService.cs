using System.Collections.Generic;

namespace SombrancelhaApp.Api.Application.Imagem;

public interface IProcessamentoImagemService
{
    /// <summary>
    /// Orquestra todo o fluxo: Criação de pastas por cliente, normalização,
    /// detecção, remoção e aplicação do molde com cor personalizada.
    /// </summary>
    /// <param name="clienteId">Identificador único do cliente para organização de pastas.</param>
    /// <param name="caminhoImagem">Caminho da imagem original enviada.</param>
    /// <param name="nomeMolde">Nome do arquivo de asset (sem .png).</param>
    /// <param name="corHex">Código hexadecimal da cor para tingir a sobrancelha. Padrão: #3B2F2F</param>
    string ProcessarFluxoCompleto(string clienteId, string caminhoImagem, string nomeMolde, string corHex = "#3B2F2F");
}