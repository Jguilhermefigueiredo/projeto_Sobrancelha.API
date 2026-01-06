namespace SombrancelhaApp.Api.Application.Imagem;

public class ResultadoDeteccaoSobrancelha
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }

    // Mock: regiões onde a sobrancelha estaria
    public bool SobrancelhaDetectada { get; set; }

    public static ResultadoDeteccaoSobrancelha Ok()
    {
        return new ResultadoDeteccaoSobrancelha
        {
            Sucesso = true,
            SobrancelhaDetectada = true,
            Mensagem = "Sobrancelha detectada com sucesso (mock)"
        };
    }

    public static ResultadoDeteccaoSobrancelha Falha(string mensagem)
    {
        return new ResultadoDeteccaoSobrancelha
        {
            Sucesso = false,
            SobrancelhaDetectada = false,
            Mensagem = mensagem
        };
    }
}