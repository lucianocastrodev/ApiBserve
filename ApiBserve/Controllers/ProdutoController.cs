using ApiBserve.Data;
using ApiBserve.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBserve.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoRepository _repo;

    public ProdutosController(ProdutoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _repo.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var produto = await _repo.GetById(id);
        if (produto == null)
            return NotFound(new { mensagem = "Produto não encontrado" });

        return Ok(produto);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Titulo))
            return BadRequest(new { mensagem = "Título é obrigatório" });

        var id = await _repo.Create(produto);
        produto.Id = id;

        return CreatedAtAction(nameof(Get), new { id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Produto produto)
    {
        produto.Id = id;

        var updated = await _repo.Update(produto);
        if (!updated)
            return NotFound(new { mensagem = "Produto não encontrado" });

        return Ok(new { mensagem = "Atualizado com sucesso" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repo.Delete(id);
        if (!deleted)
            return NotFound(new { mensagem = "Produto não encontrado" });

        return Ok(new { mensagem = "Removido com sucesso" });
    }
}