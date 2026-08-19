using EVENTIA.Models;

namespace EVENTIA.ViewModels;

public class MisPedidosViewModel
{
    public string? FiltroEstado { get; set; }
    public DateOnly? FiltroFecha { get; set; }

    public List<Pedido> Pedidos { get; set; } = new();
    public List<Pedido> PedidosPagina { get; set; } = new();
    public string? TipoPerfil { get; set; }

    public int PaginaActual { get; set; } = 1;
    public int TamanoPagina { get; set; } = 5;
    public int TotalRegistros { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
}
