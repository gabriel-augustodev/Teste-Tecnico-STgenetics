using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.API.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Categoria { get; set; } = string.Empty; // "Sanduiche", "Acompanhamento"

        [Required]
        [Range(0, 1000)]
        public decimal Preco { get; set; }
    }
}
