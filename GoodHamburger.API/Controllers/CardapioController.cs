using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GoodHamburger.API.Data;

namespace GoodHamburger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardapioController : ControllerBase
{
    private readonly AppDbContext _context;

    public CardapioController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCardapio()
    {
        var produtos = await _context.Produtos.ToListAsync();
        return Ok(produtos);
    }
}