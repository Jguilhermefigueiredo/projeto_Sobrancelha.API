using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SombrancelhaApp.Api.Infrastructure.Data;
using SombrancelhaApp.Api.Repositories;
using FluentValidation.AspNetCore;
using SombrancelhaApp.Api.Validators;
using SombrancelhaApp.Api.Application.Imagem;
using Microsoft.Extensions.FileProviders; // 1. Adicionado para suportar arquivos físicos

var builder = WebApplication.CreateBuilder(args);

// 🔹 Controllers + FluentValidation
builder.Services
    .AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateClienteDtoValidator>();
    });

builder.Services.AddScoped<IClienteImagemRepository, ClienteImagemRepository>();

builder.Services
    .AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly)
);

builder.Services.AddScoped<IDeteccaoSobrancelhaService, DeteccaoSobrancelhaService>();
builder.Services.AddScoped<IProcessamentoImagemService, ProcessamentoImagemService>();
builder.Services.AddScoped<IRemocaoSobrancelhaService, RemocaoSobrancelhaService>();
builder.Services.AddScoped<IDeteccaoFacialService, DeteccaoFacialService>();
builder.Services.AddScoped<ISubstituicaoSobrancelhaService, SubstituicaoSobrancelhaService>();

// 🔹 Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string DefaultConnection não encontrada")
    )
);

// 🔹 Repositórios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SobrancelhaApp API",
        Version = "v1"
    });
});

var app = builder.Build();

// 🔹 Migração automática
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 🔹 Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. CONFIGURAÇÃO DE ACESSO ÀS IMAGENS VIA HTTP
// Isso mapeia a pasta física para a URL: http://localhost:5000/visualizar-imagens
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Infrastructure", "Images")),
    RequestPath = "/visualizar-imagens"
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();