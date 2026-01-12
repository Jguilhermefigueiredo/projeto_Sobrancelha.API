using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace SombrancelhaApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Busca o usuário pelo e-mail
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // 2. Verifica se existe e se a senha (hash) é válida usando BCrypt
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
        }

        // 3. Gera o Token JWT
        var token = GerarTokenJwt(usuario);

        return Ok(new
        {
            usuario = new { usuario.Nome, usuario.Email, usuario.Perfil },
            token = token
        });
    }

    private string GerarTokenJwt(Usuario usuario)
{
    var tokenHandler = new JwtSecurityTokenHandler();

    // BUSCA A CHAVE DA CONFIGURAÇÃO (Program.cs)
    var chaveStr = _config["Jwt:ChaveSecreta"];
    
    // Fallback de segurança para o seu teste local agora
    if (string.IsNullOrEmpty(chaveStr) || chaveStr == "ConfigurarNoServidor")
    {
        chaveStr = "Chave_Temporaria_Para_Teste_32_Caracteres!";
    }
    
    var chave = Encoding.ASCII.GetBytes(chaveStr);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("UsuarioId", usuario.Id.ToString())
        }),
        Expires = DateTime.UtcNow.AddHours(8),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(chave), 
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

    // Endpoint para o Master cadastrar funcionários
    [Authorize(Roles = "Master")] // Apenas usuários com perfil Master
    [HttpPost("registrar-funcionario")]
    public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
    {
        // Verifica se o e-mail já está em uso
        var usuarioExistente = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email);

        if (usuarioExistente)
            return BadRequest(new { mensagem = "Este e-mail já está cadastrado no sistema." });

        // Cria o novo usuário com perfil de Funcionario
        var novoFuncionario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Email = request.Email,
            // Criptografa a senha usando BCrypt
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Perfil = PerfilUsuario.Funcionario, //funcionário por padrão neste endpoint
            DataCriacao = DateTime.Now
        };

        // Salva no banco de dados
        _context.Usuarios.Add(novoFuncionario);
        await _context.SaveChangesAsync();

        return Ok(new {
            mensagem = "Funcionário cadastrado com sucesso!",
            usuario = new { novoFuncionario.Nome, novoFuncionario.Email }
        });
    }
}

// Modelos para a requisição de login
public record LoginRequest(string Email, string Senha);
public record RegistroRequest(string Nome, string Email, string Senha);