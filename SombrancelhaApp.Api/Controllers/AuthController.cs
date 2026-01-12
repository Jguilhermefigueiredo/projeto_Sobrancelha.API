using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.Infrastructure.Data;

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
        
        // IMPORTANTE: Use a mesma chave que você configurou no Program.cs
        // O ideal é que ela venha do appsettings.json
        var chaveStr = "Sua_Chave_Super_Secreta_De_32_Caracteres_Aqui"; 
        var chave = Encoding.ASCII.GetBytes(chaveStr);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString()), // Master ou Funcionario
                new Claim("UsuarioId", usuario.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8), // Token expira em 8 horas
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(chave), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// Modelos para a requisição de login
public record LoginRequest(string Email, string Senha);