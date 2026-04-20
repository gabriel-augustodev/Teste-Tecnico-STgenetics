using Xunit;
using GoodHamburger.API.Services;
using FluentAssertions;

namespace GoodHamburger.Tests.Services;

public class DescontoServiceTests
{
    [Fact]
    public void CalcularDesconto_ComboCompleto_Retorna20Porcento()
    {
        // Act
        var (percentual, descricao) = DescontoService.CalcularDesconto(true, true, true);

        // Assert
        percentual.Should().Be(0.20m);
        descricao.Should().Contain("20%");
    }

    [Fact]
    public void CalcularDesconto_SanduicheComRefrigerante_Retorna15Porcento()
    {
        // Act
        var (percentual, descricao) = DescontoService.CalcularDesconto(true, false, true);

        // Assert
        percentual.Should().Be(0.15m);
        descricao.Should().Contain("15%");
    }

    [Fact]
    public void CalcularDesconto_SanduicheComBatata_Retorna10Porcento()
    {
        // Act
        var (percentual, descricao) = DescontoService.CalcularDesconto(true, true, false);

        // Assert
        percentual.Should().Be(0.10m);
        descricao.Should().Contain("10%");
    }

    [Fact]
    public void CalcularDesconto_ApenasSanduiche_RetornaZero()
    {
        // Act
        var (percentual, descricao) = DescontoService.CalcularDesconto(true, false, false);

        // Assert
        percentual.Should().Be(0);
        descricao.Should().Contain("Nenhum desconto");
    }
}