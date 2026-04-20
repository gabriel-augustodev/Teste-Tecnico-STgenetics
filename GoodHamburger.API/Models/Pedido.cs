using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.API.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public int? SanduicheId { get; set; }
        public int? BatataId { get; set; }
        public int? RefrigeranteId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Total { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        // Propriedades de navegação (opcional)
        public Produto? Sanduiche { get; set; }
        public Produto? Batata { get; set; }
        public Produto? Refrigerante { get; set; }
    }
}
