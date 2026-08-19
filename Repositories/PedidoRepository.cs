using EVENTIA.Data;
using EVENTIA.Dtos;
using EVENTIA.Models;
using Microsoft.EntityFrameworkCore;

namespace EVENTIA.Repositories;

public class PedidoRepository
{
    private readonly AppDbContext _appDbContext;

    public PedidoRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<List<Pedido>> GetByCliente(int clienteId)
    {
        return await _appDbContext.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente).ThenInclude(c => c.ClienteNavigation)
            .Include(p => p.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .Include(p => p.DetallePedidos).ThenInclude(d => d.Articulo)
            .Include(p => p.Pago)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
    }

    public async Task<List<Pedido>> GetByProveedor(int proveedorId)
    {
        return await _appDbContext.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente).ThenInclude(c => c.ClienteNavigation)
            .Include(p => p.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .Include(p => p.DetallePedidos).ThenInclude(d => d.Articulo)
            .Include(p => p.Pago)
            .Where(p => p.ProveedorId == proveedorId)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
    }

    public async Task<List<Pedido>> GetByProveedorFiltrado(int proveedorId, string? estado, DateOnly? fechaInicio, DateOnly? fechaFin)
    {
        var query = _appDbContext.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente).ThenInclude(c => c.ClienteNavigation)
            .Include(p => p.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .Include(p => p.DetallePedidos).ThenInclude(d => d.Articulo)
            .Include(p => p.Pago)
            .Where(p => p.ProveedorId == proveedorId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(p => p.Estado == estado);

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaEvento >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaEvento <= fechaFin.Value);

        return await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
    }

    public async Task<Pedido?> GetById(int id)
    {
        return await _appDbContext.Pedidos
            .Include(p => p.Cliente).ThenInclude(c => c.ClienteNavigation)
            .Include(p => p.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .Include(p => p.DetallePedidos).ThenInclude(d => d.Articulo)
            .Include(p => p.Pago)
            .FirstOrDefaultAsync(p => p.PedidoId == id);
    }

    public async Task<int> Crear(Pedido pedido, List<DetallePedido> detalles)
    {
        using var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            _appDbContext.Pedidos.Add(pedido);
            await _appDbContext.SaveChangesAsync();

            foreach (var detalle in detalles)
            {
                detalle.PedidoId = pedido.PedidoId;
                _appDbContext.DetallePedidos.Add(detalle);
            }

            await _appDbContext.SaveChangesAsync();

            foreach (var detalle in detalles)
            {
                var disponibilidad = await _appDbContext.Disponibilidads
                    .FirstOrDefaultAsync(d => d.ArticuloId == detalle.ArticuloId && d.Fecha == pedido.FechaEvento);

                if (disponibilidad != null)
                {
                    disponibilidad.CantidadReservada += detalle.Cantidad;
                }
            }

            await _appDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return pedido.PedidoId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ActualizarEstado(int pedidoId, string nuevoEstado)
    {
        var pedido = await _appDbContext.Pedidos.FindAsync(pedidoId);
        if (pedido == null) return;

        pedido.Estado = nuevoEstado;
        await _appDbContext.SaveChangesAsync();
    }

    public async Task Cancelar(int pedidoId)
    {
        var pedido = await _appDbContext.Pedidos
            .Include(p => p.DetallePedidos)
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

        if (pedido == null) return;

        using var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            pedido.Estado = "Cancelado";

            foreach (var detalle in pedido.DetallePedidos)
            {
                var disponibilidad = await _appDbContext.Disponibilidads
                    .FirstOrDefaultAsync(d => d.ArticuloId == detalle.ArticuloId && d.Fecha == pedido.FechaEvento);

                if (disponibilidad != null)
                {
                    disponibilidad.CantidadReservada = Math.Max(0, disponibilidad.CantidadReservada - detalle.Cantidad);
                }
            }

            await _appDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> Reprogramar(int pedidoId, DateOnly nuevaFecha)
    {
        var pedido = await _appDbContext.Pedidos
            .Include(p => p.DetallePedidos)
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

        if (pedido == null) return false;

        var fechaOriginal = pedido.FechaEvento;

        foreach (var detalle in pedido.DetallePedidos)
        {
            var disponibilidadNueva = await _appDbContext.Disponibilidads
                .FirstOrDefaultAsync(d => d.ArticuloId == detalle.ArticuloId && d.Fecha == nuevaFecha);

            if (disponibilidadNueva == null) return false;

            var libres = disponibilidadNueva.CantidadDisponible - disponibilidadNueva.CantidadReservada;
            if (libres < detalle.Cantidad) return false;
        }

        using var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            pedido.FechaEvento = nuevaFecha;

            foreach (var detalle in pedido.DetallePedidos)
            {
                var disponibilidadOriginal = await _appDbContext.Disponibilidads
                    .FirstOrDefaultAsync(d => d.ArticuloId == detalle.ArticuloId && d.Fecha == fechaOriginal);

                if (disponibilidadOriginal != null)
                {
                    disponibilidadOriginal.CantidadReservada = Math.Max(0, disponibilidadOriginal.CantidadReservada - detalle.Cantidad);
                }

                var disponibilidadNueva = await _appDbContext.Disponibilidads
                    .FirstOrDefaultAsync(d => d.ArticuloId == detalle.ArticuloId && d.Fecha == nuevaFecha);

                if (disponibilidadNueva != null)
                {
                    disponibilidadNueva.CantidadReservada += detalle.Cantidad;
                }
            }

            await _appDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
