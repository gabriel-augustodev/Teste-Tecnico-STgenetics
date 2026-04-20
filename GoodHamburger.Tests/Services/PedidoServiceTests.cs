using Xunit;
using Microsoft.EntityFrameworkCore;
using GoodHamburger.API.Data;
using GoodHamburger.API.Models;
using GoodHamburger.API.Services;
using GoodHamburger.API.DTOs;
using FluentAssertions;

namespace GoodHamburger.Tests.Services;

public class PedidoServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PedidoService _service;

    public PedidoServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        _context.Produtos.AddRange(
            new Produto { Id = 1, Nome = "X Burger", Categoria = "Sanduiche", Preco = 5.00m },
            new Produto { Id = 2, Nome = "X Egg", Categoria = "Sanduiche", Preco = 4.50m },
            new Produto { Id = 3, Nome = "X Bacon", Categoria = "Sanduiche", Preco = 7.00m },
            new Produto { Id = 4, Nome = "Batata frita", Categoria = "Acompanhamento", Preco = 2.00m },
            new Produto { Id = 5, Nome = "Refrigerante", Categoria = "Acompanhamento", Preco = 2.50m }
        );
        _context.SaveChanges();

        _service = new PedidoService(_context);
    }

    [Fact]
    public async Task CreatePedidoAsync_ComboCompleto_CalculaDesconto20Porcento()
    {
        // Arrange
        var request = new PedidoRequest
        {
            SanduicheId = 1,
            BatataId = 4,
            RefrigeranteId = 5
        };

        // Act
        var (success, error, pedido) = await _service.CreatePedidoAsync(request);

        // Assert
        Assert.True(success);
        Assert.Null(error);
        Assert.NotNull(pedido);
        Assert.Equal(9.50m, pedido!.Subtotal);
        Assert.Equal(1.90m, pedido.Desconto);
        Assert.Equal(7.60m, pedido.Total);
    }

    [Fact]
    public async Task CreatePedidoAsync_SanduicheInvalido_RetornaErro()
    {
        // Arrange
        var request = new PedidoRequest { SanduicheId = 99 };

        // Act
        var (success, error, pedido) = await _service.CreatePedidoAsync(request);

        // Assert
        Assert.False(success);
        Assert.Contains("Sanduíche inválido", error);
        Assert.Null(pedido);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}