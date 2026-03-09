using ApiBserve.Data;
using ApiBserve.Models;
using ApiBserve.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ApiBserve.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoRepository _repo;
    private readonly IHubContext<ProdutosHub> _hub;

    public ProdutosController(
        ProdutoRepository repo,
        IHubContext<ProdutosHub> hub)
    {
        _repo = repo;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var produtos = await _repo.GetAll();
        return Ok(produtos);
    }

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

        // 🔴 Evento realtime
        await _hub.Clients.All.SendAsync("produtoCriado", produto);

        return CreatedAtAction(nameof(Get), new { id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Produto produto)
    {
        produto.Id = id;

        var updated = await _repo.Update(produto);

        if (!updated)
            return NotFound(new { mensagem = "Produto não encontrado" });

        // 🔴 Evento realtime
        await _hub.Clients.All.SendAsync("produtoAtualizado", produto);

        return Ok(new { mensagem = "Atualizado com sucesso" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repo.Delete(id);

        if (!deleted)
            return NotFound(new { mensagem = "Produto não encontrado" });

        // 🔴 Evento realtime
        await _hub.Clients.All.SendAsync("produtoRemovido", id);

        return Ok(new { mensagem = "Removido com sucesso" });
    }
}