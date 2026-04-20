namespace GoodHamburger.API.DTOs
{
    public class PedidoResponse
    {
        public int Id { get; set; }
        public string? Sanduiche { get; set; }
        public string? Batata { get; set; }
        public string? Refrigerante { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Total { get; set; }
        public string? RegraDesconto { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
