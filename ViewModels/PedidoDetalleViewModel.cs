using EVENTIA.Dtos;
using EVENTIA.Models;

namespace EVENTIA.ViewModels;

public class PedidoDetalleViewModel
{
    public Pedido Pedido { get; set; } = null!;
    public List<DetallePedidoDto> Detalles { get; set; } = new();
    public Pago? Pago { get; set; }
    public List<string> EstadosDisponibles { get; set; } = new();
}
