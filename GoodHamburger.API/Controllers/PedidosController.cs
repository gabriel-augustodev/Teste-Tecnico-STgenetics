using Microsoft.AspNetCore.Mvc;
using GoodHamburger.API.DTOs;
using GoodHamburger.API.Services;

namespace GoodHamburger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidoService;

    public PedidosController(PedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pedidos = await _pedidoService.GetAllPedidosAsync();
        return Ok(pedidos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pedido = await _pedidoService.GetPedidoByIdAsync(id);

        if (pedido == null)
            return NotFound(new { mensagem = $"Pedido {id} não encontrado" });

        return Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PedidoRequest request)
    {
        if (request.SanduicheId == null)
            return BadRequest(new { mensagem = "É obrigatório informar um sanduíche" });

        var (success, errorMessage, pedido) = await _pedidoService.CreatePedidoAsync(request);

        if (!success)
            return BadRequest(new { mensagem = errorMessage });

        return CreatedAtAction(nameof(GetById), new { id = pedido!.Id }, pedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PedidoRequest request)
    {
        if (request.SanduicheId == null)
            return BadRequest(new { mensagem = "É obrigatório informar um sanduíche" });

        var (success, errorMessage, pedido) = await _pedidoService.UpdatePedidoAsync(id, request);

        if (!success)
        {
            if (errorMessage?.Contains("não encontrado") == true)
                return NotFound(new { mensagem = errorMessage });

            return BadRequest(new { mensagem = errorMessage });
        }

        return Ok(pedido);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _pedidoService.DeletePedidoAsync(id);

        if (!success)
            return NotFound(new { mensagem = errorMessage });

        return NoContent();
    }
}