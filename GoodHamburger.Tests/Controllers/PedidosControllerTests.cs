using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GoodHamburger.API.Controllers;
using GoodHamburger.API.Data;
using GoodHamburger.API.Models;
using GoodHamburger.API.Services;
using GoodHamburger.API.DTOs;
using FluentAssertions;

namespace GoodHamburger.Tests.Controllers;

public class PedidosControllerIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PedidosController _controller;

    public PedidosControllerIntegrationTests()
    {
        // Criar banco em memória para cada teste
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Seed do cardápio
        _context.Produtos.AddRange(
            new Produto { Id = 1, Nome = "X Burger", Categoria = "Sanduiche", Preco = 5.00m },
            new Produto { Id = 2, Nome = "X Egg", Categoria = "Sanduiche", Preco = 4.50m },
            new Produto { Id = 3, Nome = "X Bacon", Categoria = "Sanduiche", Preco = 7.00m },
            new Produto { Id = 4, Nome = "Batata frita", Categoria = "Acompanhamento", Preco = 2.00m },
            new Produto { Id = 5, Nome = "Refrigerante", Categoria = "Acompanhamento", Preco = 2.50m }
        );
        _context.SaveChanges();

        var service = new PedidoService(_context);
        _controller = new PedidosController(service);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComListaDePedidos()
    {
        // Arrange - criar alguns pedidos primeiro
        var service = new PedidoService(_context);
        await service.CreatePedidoAsync(new PedidoRequest { SanduicheId = 1 });
        await service.CreatePedidoAsync(new PedidoRequest { SanduicheId = 2 });

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var pedidos = okResult.Value as List<PedidoResponse>;
        pedidos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_PedidoExistente_RetornaOk()
    {
        // Arrange - criar pedido
        var service = new PedidoService(_context);
        var (_, _, pedidoCriado) = await service.CreatePedidoAsync(new PedidoRequest { SanduicheId = 1 });

        // Act
        var result = await _controller.GetById(pedidoCriado!.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var pedido = okResult.Value as PedidoResponse;
        pedido.Should().NotBeNull();
        pedido!.Id.Should().Be(pedidoCriado.Id);
    }

    [Fact]
    public async Task GetById_PedidoNaoExistente_RetornaNotFound()
    {
        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ComSanduicheValido_RetornaCreated()
    {
        // Arrange
        var request = new PedidoRequest { SanduicheId = 1 };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PedidosController.GetById));

        var pedido = createdResult.Value as PedidoResponse;
        pedido.Should().NotBeNull();
        pedido!.Sanduiche.Should().Be("X Burger");
    }

    [Fact]
    public async Task Create_SemSanduiche_RetornaBadRequest()
    {
        // Arrange
        var request = new PedidoRequest();

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ComSanduicheInvalido_RetornaBadRequest()
    {
        // Arrange
        var request = new PedidoRequest { SanduicheId = 99 };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value?.ToString().Should().Contain("Sanduíche inválido");
    }

    [Fact]
    public async Task Update_PedidoExistente_RetornaOk()
    {
        // Arrange - criar pedido
        var service = new PedidoService(_context);
        var (_, _, pedidoCriado) = await service.CreatePedidoAsync(new PedidoRequest { SanduicheId = 1 });

        var updateRequest = new PedidoRequest
        {
            SanduicheId = 3,
            BatataId = 4
        };

        // Act
        var result = await _controller.Update(pedidoCriado!.Id, updateRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var pedido = okResult.Value as PedidoResponse;
        pedido.Should().NotBeNull();
        pedido!.Sanduiche.Should().Be("X Bacon");
        pedido.Batata.Should().Be("Batata frita");
    }

    [Fact]
    public async Task Update_PedidoNaoExistente_RetornaNotFound()
    {
        // Arrange
        var request = new PedidoRequest { SanduicheId = 1 };

        // Act
        var result = await _controller.Update(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_SemSanduiche_RetornaBadRequest()
    {
        // Arrange
        var request = new PedidoRequest();

        // Act
        var result = await _controller.Update(1, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_PedidoExistente_RetornaNoContent()
    {
        // Arrange - criar pedido
        var service = new PedidoService(_context);
        var (_, _, pedidoCriado) = await service.CreatePedidoAsync(new PedidoRequest { SanduicheId = 1 });

        // Act
        var result = await _controller.Delete(pedidoCriado!.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verificar que foi removido
        var getResult = await _controller.GetById(pedidoCriado.Id);
        getResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_PedidoNaoExistente_RetornaNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ComboCompleto_CalculaDesconto20Porcento()
    {
        // Arrange
        var request = new PedidoRequest
        {
            SanduicheId = 1,
            BatataId = 4,
            RefrigeranteId = 5
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var pedido = createdResult.Value as PedidoResponse;
        pedido.Should().NotBeNull();
        pedido!.Subtotal.Should().Be(9.50m);
        pedido.Desconto.Should().Be(1.90m);
        pedido.Total.Should().Be(7.60m);
        pedido.RegraDesconto.Should().Contain("20%");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}