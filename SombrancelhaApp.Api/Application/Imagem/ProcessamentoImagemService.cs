using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using Point = System.Drawing.Point;
using SombrancelhaApp.Api.Application.Imagem;

namespace SombrancelhaApp.Api.Application.Imagem;

public class ProcessamentoImagemService : IProcessamentoImagemService
{
    private readonly INormalizacaoService _normalizacaoService;
    private readonly IIaService _iaService;
    private readonly IRemocaoSobrancelhaService _remocaoService;
    private readonly ISubstituicaoSobrancelhaService _substituicaoService;
    private readonly IWebHostEnvironment _env;

    public ProcessamentoImagemService(
        INormalizacaoService normalizacaoService,
        IIaService iaService,
        IRemocaoSobrancelhaService remocaoService,
        ISubstituicaoSobrancelhaService substituicaoService,
        IWebHostEnvironment env)
    {
        _normalizacaoService = normalizacaoService;
        _iaService = iaService;
        _remocaoService = remocaoService;
        _substituicaoService = substituicaoService;
        _env = env;
    }

    public string ProcessarFluxoCompleto(string clienteId, string caminhoImagem, string nomeMolde, string corHex)
    {
        // ESTRUTURA DE PASTAS
        string dataHoje = DateTime.Now.ToString("yyyy-MM-dd");
        string pastaDestino = Path.Combine(_env.ContentRootPath, "Storage", "Atendimentos", dataHoje, clienteId);

        if (!Directory.Exists(pastaDestino))
            Directory.CreateDirectory(pastaDestino);

        //  NORMALIZAÇÃO
        string caminhoNormalizada = Path.Combine(pastaDestino, "1_normalizada.jpg");
        _normalizacaoService.Normalizar(caminhoImagem, caminhoNormalizada);

        // DETECÇÃO IA
        var todosPontos = _iaService.DetectarPontos(caminhoNormalizada);

// Separa os pontos (Lógica IBUG)
var pontosEsquerda = todosPontos.Take(5).ToList(); // 17-21
var pontosDireita = todosPontos.Skip(5).Take(5).ToList(); // 22-26

// REMOÇÃO (Passa todos os pontos para limpar ambos os lados)
string caminhoLimpa = Path.Combine(pastaDestino, "2_limpa.jpg");
_remocaoService.RemoverSobrancelha(caminhoNormalizada, todosPontos, caminhoLimpa);

// SUBSTITUIÇÃO (Exemplo aplicando no lado que o usuário escolher ou ambos)
string caminhoFinal = _substituicaoService.AplicarMolde(caminhoLimpa, nomeMolde, pontosEsquerda, corHex);
caminhoFinal = _substituicaoService.AplicarMolde(caminhoFinal, nomeMolde, pontosDireita, corHex);

return caminhoFinal;
    }
}