using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.API.DTOs
{
    public class PedidoRequest
    {
        [Required]
        public int? SanduicheId { get; set; }

        public int? BatataId { get; set; }

        public int? RefrigeranteId { get; set; }
    }
}
