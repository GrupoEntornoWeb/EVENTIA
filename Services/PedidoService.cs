using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Repositories;

namespace EVENTIA.Services;

public class PedidoService
{
    private readonly PedidoRepository _pedidoRepository;
    private readonly DisponibilidadService _disponibilidadService;

    public PedidoService(PedidoRepository pedidoRepository, DisponibilidadService disponibilidadService)
    {
        _pedidoRepository = pedidoRepository;
        _disponibilidadService = disponibilidadService;
    }

    public async Task<List<Pedido>> GetByCliente(int clienteId)
    {
        return await _pedidoRepository.GetByCliente(clienteId);
    }

    public async Task<List<Pedido>> GetByProveedor(int proveedorId)
    {
        return await _pedidoRepository.GetByProveedor(proveedorId);
    }

    public async Task<List<Pedido>> GetByProveedorFiltrado(int proveedorId, string? estado, DateOnly? fechaInicio, DateOnly? fechaFin)
    {
        return await _pedidoRepository.GetByProveedorFiltrado(proveedorId, estado, fechaInicio, fechaFin);
    }

    public async Task<Pedido?> GetById(int id)
    {
        return await _pedidoRepository.GetById(id);
    }

    public async Task<int> Crear(PedidoDto dto, List<DetallePedidoDto> detalles)
    {
        var pedido = new Pedido
        {
            ClienteId = dto.ClienteId,
            ProveedorId = dto.ProveedorId,
            FechaEvento = dto.FechaEvento,
            Estado = "Reservado",
            MontoTotal = dto.MontoTotal
        };

        var detalleEntities = detalles.Select(d => new DetallePedido
        {
            ArticuloId = d.ArticuloId,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList();

        var pedidoId = await _pedidoRepository.Crear(pedido, detalleEntities);
        return pedidoId;
    }

    public async Task<bool> ConfirmarPago(int pedidoId)
    {
        var pedido = await _pedidoRepository.GetById(pedidoId);
        if (pedido == null || pedido.Estado != "Reservado") return false;

        await _pedidoRepository.ActualizarEstado(pedidoId, "Confirmado");
        return true;
    }

    public async Task<bool> MarcarEntregado(int pedidoId)
    {
        var pedido = await _pedidoRepository.GetById(pedidoId);
        if (pedido == null || pedido.Estado != "Confirmado") return false;

        await _pedidoRepository.ActualizarEstado(pedidoId, "Entregado");
        return true;
    }

    public async Task<bool> RegistrarDevolucion(int pedidoId)
    {
        var pedido = await _pedidoRepository.GetById(pedidoId);
        if (pedido == null || pedido.Estado != "Entregado") return false;

        await _pedidoRepository.ActualizarEstado(pedidoId, "Devuelto");

        foreach (var detalle in pedido.DetallePedidos)
        {
            await _disponibilidadService.Liberar(detalle.ArticuloId, pedido.FechaEvento, detalle.Cantidad);
        }

        return true;
    }

    public async Task<bool> Cancelar(int pedidoId)
    {
        var pedido = await _pedidoRepository.GetById(pedidoId);
        if (pedido == null) return false;

        if (pedido.Estado != "Reservado" && pedido.Estado != "Confirmado") return false;

        await _pedidoRepository.Cancelar(pedidoId);
        return true;
    }

    public async Task<bool> Reprogramar(int pedidoId, DateOnly nuevaFecha)
    {
        var pedido = await _pedidoRepository.GetById(pedidoId);
        if (pedido == null) return false;

        if (pedido.Estado != "Reservado" && pedido.Estado != "Confirmado") return false;

        return await _pedidoRepository.Reprogramar(pedidoId, nuevaFecha);
    }
}
