using System.Collections.Generic;
using System.Drawing;
namespace SombrancelhaApp.Api.Application.Imagem;

public interface IRemocaoSobrancelhaService
{   string RemoverSobrancelha(string caminhoImagem, List<System.Drawing.Point> pontosSobrancelha, string caminhoSaida);
    }