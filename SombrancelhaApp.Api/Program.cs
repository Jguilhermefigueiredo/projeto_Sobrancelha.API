using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SombrancelhaApp.Api.Infrastructure.Data;
using SombrancelhaApp.Api.Repositories;
using FluentValidation.AspNetCore;
using SombrancelhaApp.Api.Validators;
using SombrancelhaApp.Api.Application.Imagem;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Controllers + FluentValidation
builder.Services
    .AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateClienteDtoValidator>();
    });

// 🔹 Injeção de Dependências - Serviços de Imagem

builder.Services.AddScoped<INormalizacaoService, NormalizacaoService>(); // Adicionado
builder.Services.AddScoped<IIaService, IaService>(); // Adicionado (ou o nome da sua classe de IA)

builder.Services.AddScoped<IProcessamentoImagemService, ProcessamentoImagemService>();
builder.Services.AddScoped<IRemocaoSobrancelhaService, RemocaoSobrancelhaService>();
builder.Services.AddScoped<ISubstituicaoSobrancelhaService, SubstituicaoSobrancelhaService>();

// serviços de detecção se ainda estiver usando separadamente
builder.Services.AddScoped<IDeteccaoSobrancelhaService, DeteccaoSobrancelhaService>();
builder.Services.AddScoped<IDeteccaoFacialService, DeteccaoFacialService>();

// 🔹 Banco de Dados e Repositórios
builder.Services.AddScoped<IClienteImagemRepository, ClienteImagemRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string DefaultConnection não encontrada")
    )
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SobrancelhaApp API", Version = "v1" });
});

var app = builder.Build();

// 🔹 Migração automática
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 2. CONFIGURAÇÃO DE ACESSO ÀS IMAGENS (MAPEAMENTOS)

// Mapeamento 1: Imagens de Cadastro (Clientes)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Infrastructure", "Images")),
    RequestPath = "/visualizar-imagens"
});

// --- MAPEAMENTO 2 ---
string storagePath = Path.Combine(builder.Environment.ContentRootPath, "Storage");
if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/visualizar-imagens-atendimentos",
    OnPrepareResponse = ctx =>
    {
        // Adiciona o cabeçalho de Cache para que o navegador não precise baixar
        // a mesma imagem várias vezes enquanto o usuário testa moldes.
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});



app.UseHttpsRedirection();
app.MapControllers();

app.Run();