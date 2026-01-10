using Microsoft.EntityFrameworkCore;
using SombrancelhaApp.Api.Infrastructure.Data;

namespace SombrancelhaApp.Api.Infrastructure.BackgroundServices;

public class LimpezaArquivosService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LimpezaArquivosService> _logger;
    
    // Define que uma simulação é considerada "antiga" após 48 horas
    private readonly TimeSpan _tempoDeVida = TimeSpan.FromHours(48); 

    public LimpezaArquivosService(IServiceProvider serviceProvider, ILogger<LimpezaArquivosService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Serviço de Limpeza Híbrida iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dataCorte = DateTime.Now.Subtract(_tempoDeVida);

                // Busca apenas o que expirou E já foi autorizado pelo usuário
                var prontosParaDeletar = await context.AtendimentoSimulacoes
                    .Where(s => s.DataCriacao < dataCorte && s.ConfirmadoParaDeletar)
                    .ToListAsync(stoppingToken);

                if (prontosParaDeletar.Any())
                {
                    _logger.LogInformation($"🧹 Faxina iniciada: {prontosParaDeletar.Count} itens para remover.");

                    foreach (var simulacao in prontosParaDeletar)
                    {
                        try
                        {
                            // 1. Remove o arquivo físico no disco
                            if (System.IO.File.Exists(simulacao.CaminhoImagemFinal))
                            {
                                System.IO.File.Delete(simulacao.CaminhoImagemFinal);
                                
                                // Tenta limpar a pasta do cliente se ficar vazia
                                var diretorio = Path.GetDirectoryName(simulacao.CaminhoImagemFinal);
                                if (Directory.Exists(diretorio) && !Directory.EnumerateFileSystemEntries(diretorio).Any())
                                {
                                    Directory.Delete(diretorio);
                                }
                            }

                            // 2. Remove o registro do Banco de Dados
                            context.AtendimentoSimulacoes.Remove(simulacao);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"❌ Falha ao excluir item {simulacao.Id}: {ex.Message}");
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("✅ Faxina concluída com sucesso.");
                }
            }

            // O serviço "dorme" por 6 horas antes de verificar novamente
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}