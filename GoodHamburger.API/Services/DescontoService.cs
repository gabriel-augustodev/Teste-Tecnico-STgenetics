namespace GoodHamburger.API.Services
{
    public class DescontoService
    {
        public static (decimal Percentual, string Descricao) CalcularDesconto(
            bool temSanduiche,
            bool temBatata,
            bool temRefrigerante)
        {
            if (temSanduiche && temBatata && temRefrigerante)
                return (0.20m, "Combo completo: 20% de desconto");

            if (temSanduiche && temRefrigerante)
                return (0.15m, "Sanduíche + Refrigerante: 15% de desconto");

            if (temSanduiche && temBatata)
                return (0.10m, "Sanduíche + Batata: 10% de desconto");

            return (0, "Nenhum desconto aplicado");
        }
    }
}
