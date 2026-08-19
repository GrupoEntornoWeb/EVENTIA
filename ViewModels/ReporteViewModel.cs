using EVENTIA.Models;

namespace EVENTIA.ViewModels;

public class ReporteViewModel
{
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string? FiltroEstado { get; set; }
    public int? FiltroArticuloId { get; set; }

    public List<Pedido> Pedidos { get; set; } = new();
    public List<Articulo> Articulos { get; set; } = new();

    public int PaginaActual { get; set; } = 1;
    public int TamanoPagina { get; set; } = 5;
    public int TotalRegistros => Pedidos.Count;
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
    public List<Pedido> PedidosPagina => Pedidos
        .Skip((PaginaActual - 1) * TamanoPagina)
        .Take(TamanoPagina)
        .ToList();

    public decimal IngresosTotales => Pedidos.Where(p => p.Estado != "Cancelado").Sum(p => p.MontoTotal);
}
