namespace SombrancelhaApp.Api.Application.Imagem;

public class ResultadoProcessamentoImagem
{
    public bool Sucesso { get; private set; }
    public string? Mensagem { get; private set; }

    // Alteramos/Adicionamos este nome para ser usado tanto na normalização quanto na remoção
    public string? CaminhoProcessado { get; private set; }

    public static ResultadoProcessamentoImagem Ok(string caminho)
    {
        return new ResultadoProcessamentoImagem
        {
            Sucesso = true,
            CaminhoProcessado = caminho,
            Mensagem = "Processamento concluído com sucesso"
        };
    }

    public static ResultadoProcessamentoImagem Falha(string mensagem)
    {
        return new ResultadoProcessamentoImagem
        {
            Sucesso = false,
            Mensagem = mensagem
        };
    }
}