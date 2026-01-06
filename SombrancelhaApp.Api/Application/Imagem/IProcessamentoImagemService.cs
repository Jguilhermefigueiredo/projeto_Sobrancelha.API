namespace SombrancelhaApp.Api.Application.Imagem;

public interface IProcessamentoImagemService
{
    //ResultadoProcessamentoImagem Normalizar(string caminhoImagem);
    ResultadoProcessamentoImagem ProcessarFluxoCompleto(string caminhoImagem, string nomeMolde);
}
