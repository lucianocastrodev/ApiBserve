using ApiBserve.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiBserve.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UsuarioRepository _repo;

    public AuthController(UsuarioRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _repo.ObterPorEmail(dto.Email);
        if (usuario == null || !usuario.Ativo)
            return Unauthorized();

        var passwordHasher = new PasswordHasher<string>();
        var result = passwordHasher.VerifyHashedPassword(null!, usuario.SenhaHash, dto.Senha);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role)
        };

        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            "MyCookieAuth",
            principal,

            new AuthenticationProperties
            {
                // Para desenvolvimento: true para manter a sessão mesmo após fechar o navegador
                // Para produção: false para expirar a sessão ao fechar o navegador
                // Ou se tiver função de "Lembrar-me", usar o valor do checkbox
                IsPersistent = true
            });

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MyCookieAuth");
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return Ok(new SessionModel
            {
                Logado = true,
                Usuario = new Usuario
                {
                    Nome = User.Identity.Name ?? "",
                    Role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? ""
                }
            });
        }

        return Unauthorized();
    }
}

public record LoginDto(string Email, string Senha);

public class SessionModel
{
    public bool Logado { get; set; }
    public Usuario Usuario { get; set; } = new Usuario();
}

