using GoodHamburger.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed do cardápio
            modelBuilder.Entity<Produto>().HasData(
                new Produto { Id = 1, Nome = "X Burger", Categoria = "Sanduiche", Preco = 5.00m },
                new Produto { Id = 2, Nome = "X Egg", Categoria = "Sanduiche", Preco = 4.50m },
                new Produto { Id = 3, Nome = "X Bacon", Categoria = "Sanduiche", Preco = 7.00m },
                new Produto { Id = 4, Nome = "Batata frita", Categoria = "Acompanhamento", Preco = 2.00m },
                new Produto { Id = 5, Nome = "Refrigerante", Categoria = "Acompanhamento", Preco = 2.50m }
            );
        }
    }
}
