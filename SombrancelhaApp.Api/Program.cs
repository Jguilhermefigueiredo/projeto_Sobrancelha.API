using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SombrancelhaApp.Api.Infrastructure.Data;
using SombrancelhaApp.Api.Repositories;
using FluentValidation.AspNetCore;
using SombrancelhaApp.Api.Validators;
using SombrancelhaApp.Api.Application.Imagem;
using Microsoft.Extensions.FileProviders;
using SombrancelhaApp.Api.Infrastructure.BackgroundServices;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SombrancelhaApp.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES DE SEGURANÇA (JWT) ---
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var chaveJwt = builder.Configuration["Jwt:ChaveSecreta"] ?? "Chave_Temporaria_Para_Teste_32_Caracteres!";
var key = Encoding.ASCII.GetBytes(chaveJwt);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("Master", policy => policy.RequireRole("Master"));
});

// --- 2. SERVIÇOS DO SISTEMA E IA ---
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateClienteDtoValidator>());

// META 2.1: IA como SINGLETON (Performance Crítica)
// Serviços que carregam modelos pesados devem ser Singletons
builder.Services.AddSingleton<IDeteccaoFacialService, DeteccaoFacialService>();
builder.Services.AddSingleton<IDeteccaoSobrancelhaService, DeteccaoSobrancelhaService>();
builder.Services.AddSingleton<IIaService, IaService>(); 

// Serviços de fluxo podem ser Scoped ou Singleton
builder.Services.AddScoped<INormalizacaoService, NormalizacaoService>();
builder.Services.AddScoped<IProcessamentoImagemService, ProcessamentoImagemService>();
builder.Services.AddScoped<IRemocaoSobrancelhaService, RemocaoSobrancelhaService>();
builder.Services.AddScoped<ISubstituicaoSobrancelhaService, SubstituicaoSobrancelhaService>();

// Background Services e Repositórios
builder.Services.AddHostedService<LimpezaArquivosService>();
builder.Services.AddScoped<IClienteImagemRepository, ClienteImagemRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=SobrancelhaApp.db")
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// --- 3. CORS (Para o Front-end conseguir conectar) ---
builder.Services.AddCors(options => {
    options.AddPolicy("AppPolicy", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SobrancelhaApp API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- 4. TRATAMENTO DE ERRO GLOBAL (Deve ser o primeiro middleware) ---
app.UseExceptionHandler(errorApp => {
    errorApp.Run(async context => {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        await context.Response.WriteAsJsonAsync(new { 
            mensagem = "Erro interno no servidor de Simulação.",
            detalhe = feature?.Error.Message 
        });
    });
});

// --- 5. BANCO DE DADOS E SEEDING ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario {
            Nome = "Administrador Master",
            Email = "admin@sistema.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Perfil = PerfilUsuario.Master
        });
        db.SaveChanges();
    }
}

// --- 6. ARQUIVOS ESTÁTICOS E PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Infrastructure", "Images")),
    RequestPath = "/visualizar-imagens"
});

string storagePath = Path.Combine(builder.Environment.ContentRootPath, "Storage");
if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Storage")),
    RequestPath = "/visualizar-imagens-atendimentos"
});

app.UseCors("AppPolicy"); // Ativar antes da Autenticação
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();