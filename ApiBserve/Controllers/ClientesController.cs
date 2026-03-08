using Microsoft.AspNetCore.Mvc;
using ApiBserve.Data;
using ApiBserve.Models;

namespace ApiBserve.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClienteRepository _repo;

    public ClientesController(ClienteRepository repo)
    {
        _repo = repo;
    }

    // GET: api/clientes
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _repo.GetAll());

    // GET: api/clientes/1
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cliente = await _repo.GetById(id);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado" });

        return Ok(cliente);
    }

    // POST: api/clientes
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome) ||
            string.IsNullOrWhiteSpace(cliente.Email))
            return BadRequest(new { mensagem = "Nome e Email são obrigatórios" });

        var id = await _repo.Create(cliente);
        cliente.Id = id;

        return CreatedAtAction(nameof(Get), new { id }, cliente);
    }

    // PUT: api/clientes/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Cliente cliente)
    {
        cliente.Id = id;
        var updated = await _repo.Update(cliente);
        if (!updated) return NotFound(new { mensagem = "Cliente não encontrado" });
        return Ok(new { mensagem = "Atualizado com sucesso" });
    }

    // DELETE: api/clientes/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repo.Delete(id);
        if (!deleted) return NotFound(new { mensagem = "Cliente não encontrado" });
        return Ok(new { mensagem = "Removido com sucesso" });
    }
}