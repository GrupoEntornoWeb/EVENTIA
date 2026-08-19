using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Repositories;

namespace EVENTIA.Services;

public class PagoService
{
    private readonly PagoRepository _pagoRepository;

    public PagoService(PagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    public async Task<Pago?> GetByPedidoId(int pedidoId)
    {
        return await _pagoRepository.GetByPedidoId(pedidoId);
    }

    public async Task RegistrarPago(PagoDto dto)
    {
        var pago = new Pago
        {
            PedidoId = dto.PedidoId,
            MetodoPago = dto.MetodoPago,
            MontoPagado = dto.MontoPagado,
            FechaPago = DateTime.Now,
            Estado = "Pagado"
        };

        await _pagoRepository.RegistrarPago(pago);
    }

    public async Task<Pago> ConfirmarPago(PagoDto dto)
    {
        var pago = new Pago
        {
            PedidoId = dto.PedidoId,
            MetodoPago = dto.MetodoPago,
            MontoPagado = dto.MontoPagado,
            FechaPago = DateTime.Now,
            Estado = "Pagado"
        };

        return await _pagoRepository.ConfirmarPago(pago);
    }
}
