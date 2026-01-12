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

// Força o carregamento dos segredos em modo de desenvolvimento
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// se o valor for o placeholder ou nulo, usa a chave de teste
var chaveJwt = builder.Configuration["Jwt:ChaveSecreta"];

if (string.IsNullOrEmpty(chaveJwt) || chaveJwt == "ConfigurarNoServidor")
{
    chaveJwt = "Chave_Temporaria_Para_Teste_32_Caracteres!";
}

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

// --- 2. SERVIÇOS DO SISTEMA ---
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateClienteDtoValidator>();
    });

// Injeção de Dependências - Imagem e IA
builder.Services.AddScoped<INormalizacaoService, NormalizacaoService>();
builder.Services.AddScoped<IIaService, IaService>();
builder.Services.AddScoped<IProcessamentoImagemService, ProcessamentoImagemService>();
builder.Services.AddScoped<IRemocaoSobrancelhaService, RemocaoSobrancelhaService>();
builder.Services.AddScoped<ISubstituicaoSobrancelhaService, SubstituicaoSobrancelhaService>();
builder.Services.AddScoped<IDeteccaoSobrancelhaService, DeteccaoSobrancelhaService>();
builder.Services.AddScoped<IDeteccaoFacialService, DeteccaoFacialService>();

// Background Services
builder.Services.AddHostedService<LimpezaArquivosService>();

// Banco de Dados e Repositórios
builder.Services.AddScoped<IClienteImagemRepository, ClienteImagemRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=SobrancelhaApp.db")
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// --- 3. SWAGGER COM SUPORTE A JWT ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SobrancelhaApp API", Version = "v1" });

    // Adiciona o campo de "Authorize" no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- 4. CICLO DE VIDA E DATA SEEDING ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    
    // Roda Migrações
    db.Database.Migrate();

    // SCRIPT DE PRIMEIRO ACESSO: Cria o Master se o banco estiver vazio
    if (!db.Usuarios.Any())
    {
        var senhaHashed = BCrypt.Net.BCrypt.HashPassword("Admin123!"); // Mude ao subir
        db.Usuarios.Add(new Usuario
        {
            Nome = "Administrador Master",
            Email = "admin@sistema.com",
            SenhaHash = senhaHashed,
            Perfil = PerfilUsuario.Master
        });
        db.SaveChanges();
    }
}

// --- 5. MIDDLEWARES E ARQUIVOS ESTÁTICOS ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Imagens de Cadastro
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Infrastructure", "Images")),
    RequestPath = "/visualizar-imagens"
});

// Imagens de Atendimentos (Storage)
string storagePath = Path.Combine(builder.Environment.ContentRootPath, "Storage");
if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/visualizar-imagens-atendimentos",
    OnPrepareResponse = ctx => {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

app.UseHttpsRedirection();

// A ordem aqui importa: Autenticação antes de Autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();