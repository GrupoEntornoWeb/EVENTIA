using EVENTIA.Data;
using EVENTIA.Models;
using Microsoft.EntityFrameworkCore;

namespace EVENTIA.Repositories;

public class PagoRepository
{
    private readonly AppDbContext _appDbContext;

    public PagoRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Pago?> GetByPedidoId(int pedidoId)
    {
        return await _appDbContext.Pagos
            .AsNoTracking()
            .Include(p => p.Pedido)
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
    }

    public async Task RegistrarPago(Pago pago)
    {
        _appDbContext.Pagos.Add(pago);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<Pago> ConfirmarPago(Pago pago)
    {
        var existing = await _appDbContext.Pagos
            .FirstOrDefaultAsync(p => p.PedidoId == pago.PedidoId);
        if (existing != null)
            throw new InvalidOperationException("Este pedido ya tiene un pago registrado.");

        using var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            _appDbContext.Pagos.Add(pago);
            await _appDbContext.SaveChangesAsync();

            var pedido = await _appDbContext.Pedidos.FindAsync(pago.PedidoId);
            if (pedido == null)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Pedido no encontrado.");
            }

            pedido.Estado = "Confirmado";
            await _appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return pago;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
