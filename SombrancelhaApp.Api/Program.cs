using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SombrancelhaApp.Api.Infrastructure.Data;
using SombrancelhaApp.Api.Repositories;
using FluentValidation;
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

// --- 1. JWT ---
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

// --- 2. SERVIÇOS ---
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateClienteDtoValidator>();

builder.Services.AddSingleton<IDeteccaoFacialService, DeteccaoFacialService>();
builder.Services.AddSingleton<IDeteccaoSobrancelhaService, DeteccaoSobrancelhaService>();
builder.Services.AddSingleton<IIaService, IaService>(); 

builder.Services.AddScoped<INormalizacaoService, NormalizacaoService>();
builder.Services.AddScoped<IProcessamentoImagemService, ProcessamentoImagemService>();
builder.Services.AddScoped<IRemocaoSobrancelhaService, RemocaoSobrancelhaService>();
builder.Services.AddScoped<ISubstituicaoSobrancelhaService, SubstituicaoSobrancelhaService>();

builder.Services.AddHostedService<LimpezaArquivosService>();
builder.Services.AddScoped<IClienteImagemRepository, ClienteImagemRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=SobrancelhaApp.db")
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddCors(options => {
    options.AddPolicy("AppPolicy", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// --- 3. SWAGGER (Com correção do Botão de Upload) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SobrancelhaApp API", Version = "v1" });
    
    // FORÇA O BOTÃO DE UPLOAD A APARECER NO SWAGGER
    options.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

var app = builder.Build();

// --- 4. MIDDLEWARES E ARQUIVOS ESTÁTICOS ---
app.UseExceptionHandler("/error");

// Caminhos Físicos
string storagePath = Path.Combine(builder.Environment.ContentRootPath, "Storage");
if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

string assetPath = Path.Combine(builder.Environment.ContentRootPath, "Infrastructure", "Images");

// Servir arquivos estáticos (Antes da Autenticação para evitar 404/401 em imagens)
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(assetPath),
    RequestPath = "/visualizar-imagens"
});

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/visualizar-imagens-atendimentos"
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AppPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();