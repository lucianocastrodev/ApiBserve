using ApiBserve.Data;
using ApiBserve.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioRepository _repo;

    public UsuarioController(UsuarioRepository repo)
    {
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Usuario usuario)
    {
        try
        {
            await _repo.Criar(usuario);
            return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // violação de UNIQUE
        {
            return BadRequest(new { mensagem = "Email já cadastrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro inesperado: " + ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var usuario = await _repo.ObterTodos(); // ou criar método ObterPorId no repo
        var u = usuario.FirstOrDefault(u => u.Id == id);
        if (u == null) return NotFound();
        return Ok(u);
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var lista = await _repo.ObterTodos();
        return Ok(lista);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] Usuario usuario)
    {
        if (id != usuario.Id)
            return BadRequest(new { mensagem = "ID inválido" });

        var sucesso = await _repo.Atualizar(usuario);
        if (!sucesso) return NotFound(new { mensagem = "Usuário não encontrado" });

        return Ok(usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        var sucesso = await _repo.Deletar(id);
        if (!sucesso) return NotFound(new { mensagem = "Usuário não encontrado" });

        return NoContent();
    }
}