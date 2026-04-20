using Microsoft.EntityFrameworkCore;
using GoodHamburger.API.Data;
using GoodHamburger.API.Models;
using GoodHamburger.API.DTOs;

namespace GoodHamburger.API.Services;

public class PedidoService
{
    private readonly AppDbContext _context;

    public PedidoService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os pedidos com seus relacionamentos
    /// </summary>
    public async Task<List<PedidoResponse>> GetAllPedidosAsync()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Sanduiche)
            .Include(p => p.Batata)
            .Include(p => p.Refrigerante)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();

        return pedidos.Select(p => MapToResponse(p)).ToList();
    }

    /// <summary>
    /// Obtém um pedido específico por ID
    /// </summary>
    public async Task<PedidoResponse?> GetPedidoByIdAsync(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Sanduiche)
            .Include(p => p.Batata)
            .Include(p => p.Refrigerante)
            .FirstOrDefaultAsync(p => p.Id == id);

        return pedido != null ? MapToResponse(pedido) : null;
    }

    /// <summary>
    /// Cria um novo pedido com validações e cálculos
    /// </summary>
    public async Task<(bool success, string? errorMessage, PedidoResponse? pedido)> CreatePedidoAsync(PedidoRequest request)
    {
        // 1. Validar sanduíche
        var sanduiche = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.SanduicheId && p.Categoria == "Sanduiche");

        if (sanduiche == null)
            return (false, "Sanduíche inválido ou não encontrado", null);

        // 2. Validar batata (se informada)
        Produto? batata = null;
        if (request.BatataId.HasValue)
        {
            batata = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == request.BatataId && p.Nome == "Batata frita");

            if (batata == null)
                return (false, "Batata frita inválida ou não encontrada", null);
        }

        // 3. Validar refrigerante (se informado)
        Produto? refrigerante = null;
        if (request.RefrigeranteId.HasValue)
        {
            refrigerante = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == request.RefrigeranteId && p.Nome == "Refrigerante");

            if (refrigerante == null)
                return (false, "Refrigerante inválido ou não encontrado", null);
        }

        // 4. Validar itens duplicados (impedir dois itens da mesma categoria)
        if (await TemItensDuplicados(request))
            return (false, "Não é permitido mais de um item da mesma categoria no pedido", null);

        // 5. Calcular subtotal
        var subtotal = CalcularSubtotal(sanduiche, batata, refrigerante);

        // 6. Calcular desconto
        var temBatata = batata != null;
        var temRefrigerante = refrigerante != null;
        var (percentual, descricaoDesconto) = DescontoService.CalcularDesconto(true, temBatata, temRefrigerante);

        var valorDesconto = subtotal * percentual;
        var total = subtotal - valorDesconto;

        // 7. Criar o pedido
        var pedido = new Pedido
        {
            SanduicheId = request.SanduicheId,
            BatataId = request.BatataId,
            RefrigeranteId = request.RefrigeranteId,
            Subtotal = subtotal,
            Desconto = valorDesconto,
            Total = total,
            DataCriacao = DateTime.Now
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // 8. Retornar resposta com dados completos
        var response = new PedidoResponse
        {
            Id = pedido.Id,
            Sanduiche = sanduiche.Nome,
            Batata = batata?.Nome,
            Refrigerante = refrigerante?.Nome,
            Subtotal = subtotal,
            Desconto = valorDesconto,
            Total = total,
            RegraDesconto = descricaoDesconto,
            DataCriacao = pedido.DataCriacao
        };

        return (true, null, response);
    }

    /// <summary>
    /// Atualiza um pedido existente
    /// </summary>
    public async Task<(bool success, string? errorMessage, PedidoResponse? pedido)> UpdatePedidoAsync(int id, PedidoRequest request)
    {
        // Verificar se pedido existe
        var pedidoExistente = await _context.Pedidos.FindAsync(id);
        if (pedidoExistente == null)
            return (false, $"Pedido {id} não encontrado", null);

        // Validar sanduíche
        var sanduiche = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.SanduicheId && p.Categoria == "Sanduiche");

        if (sanduiche == null)
            return (false, "Sanduíche inválido ou não encontrado", null);

        // Validar batata
        Produto? batata = null;
        if (request.BatataId.HasValue)
        {
            batata = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == request.BatataId && p.Nome == "Batata frita");

            if (batata == null)
                return (false, "Batata frita inválida ou não encontrada", null);
        }

        // Validar refrigerante
        Produto? refrigerante = null;
        if (request.RefrigeranteId.HasValue)
        {
            refrigerante = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == request.RefrigeranteId && p.Nome == "Refrigerante");

            if (refrigerante == null)
                return (false, "Refrigerante inválido ou não encontrado", null);
        }

        // Validar duplicados
        if (await TemItensDuplicados(request, id))
            return (false, "Não é permitido mais de um item da mesma categoria no pedido", null);

        // Calcular valores
        var subtotal = CalcularSubtotal(sanduiche, batata, refrigerante);
        var temBatata = batata != null;
        var temRefrigerante = refrigerante != null;
        var (percentual, descricaoDesconto) = DescontoService.CalcularDesconto(true, temBatata, temRefrigerante);

        var valorDesconto = subtotal * percentual;
        var total = subtotal - valorDesconto;

        // Atualizar pedido
        pedidoExistente.SanduicheId = request.SanduicheId;
        pedidoExistente.BatataId = request.BatataId;
        pedidoExistente.RefrigeranteId = request.RefrigeranteId;
        pedidoExistente.Subtotal = subtotal;
        pedidoExistente.Desconto = valorDesconto;
        pedidoExistente.Total = total;
        pedidoExistente.DataAtualizacao = DateTime.Now;

        await _context.SaveChangesAsync();

        var response = new PedidoResponse
        {
            Id = pedidoExistente.Id,
            Sanduiche = sanduiche.Nome,
            Batata = batata?.Nome,
            Refrigerante = refrigerante?.Nome,
            Subtotal = subtotal,
            Desconto = valorDesconto,
            Total = total,
            RegraDesconto = descricaoDesconto,
            DataCriacao = pedidoExistente.DataCriacao
        };

        return (true, null, response);
    }

    /// <summary>
    /// Remove um pedido
    /// </summary>
    public async Task<(bool success, string? errorMessage)> DeletePedidoAsync(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return (false, $"Pedido {id} não encontrado");

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    /// <summary>
    /// Verifica se há itens duplicados no pedido
    /// </summary>
    private async Task<bool> TemItensDuplicados(PedidoRequest request, int? excludePedidoId = null)
    {
        // Buscar produtos por categoria
        var sanduiches = await _context.Produtos
            .Where(p => p.Categoria == "Sanduiche")
            .Select(p => p.Id)
            .ToListAsync();

        var acompanhamentos = await _context.Produtos
            .Where(p => p.Categoria == "Acompanhamento")
            .Select(p => p.Id)
            .ToListAsync();

        // Contar quantos sanduíches foram solicitados (deve ser no máximo 1)
        int qtdSanduiches = request.SanduicheId.HasValue && sanduiches.Contains(request.SanduicheId.Value) ? 1 : 0;

        // Contar quantos acompanhamentos de cada tipo (na prática, só pode ter 1 batata e 1 refri)
        // Mas como são IDs diferentes, verificamos se há dois itens do mesmo tipo
        var itensAcompanhamento = new List<int?>();
        if (request.BatataId.HasValue) itensAcompanhamento.Add(request.BatataId);
        if (request.RefrigeranteId.HasValue) itensAcompanhamento.Add(request.RefrigeranteId);

        // Verificar se há IDs duplicados (mesmo produto repetido)
        var hasDuplicate = itensAcompanhamento
            .Where(id => id.HasValue)
            .GroupBy(id => id)
            .Any(g => g.Count() > 1);

        return qtdSanduiches > 1 || hasDuplicate;
    }

    /// <summary>
    /// Calcula o subtotal do pedido
    /// </summary>
    private decimal CalcularSubtotal(Produto sanduiche, Produto? batata, Produto? refrigerante)
    {
        decimal subtotal = sanduiche.Preco;

        if (batata != null)
            subtotal += batata.Preco;

        if (refrigerante != null)
            subtotal += refrigerante.Preco;

        return subtotal;
    }

    /// <summary>
    /// Mapeia Pedido para PedidoResponse
    /// </summary>
    private PedidoResponse MapToResponse(Pedido pedido)
    {
        var temBatata = pedido.BatataId.HasValue;
        var temRefrigerante = pedido.RefrigeranteId.HasValue;
        var (_, descricaoDesconto) = DescontoService.CalcularDesconto(true, temBatata, temRefrigerante);

        return new PedidoResponse
        {
            Id = pedido.Id,
            Sanduiche = pedido.Sanduiche?.Nome,
            Batata = pedido.Batata?.Nome,
            Refrigerante = pedido.Refrigerante?.Nome,
            Subtotal = pedido.Subtotal,
            Desconto = pedido.Desconto,
            Total = pedido.Total,
            RegraDesconto = descricaoDesconto,
            DataCriacao = pedido.DataCriacao
        };
    }
}