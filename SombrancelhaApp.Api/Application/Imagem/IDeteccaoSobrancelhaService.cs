namespace SombrancelhaApp.Api.Application.Imagem;

public interface IDeteccaoSobrancelhaService
{
    ResultadoDeteccaoSobrancelha Detectar(string caminhoImagem);
}
