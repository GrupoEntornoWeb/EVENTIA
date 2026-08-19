using EVENTIA.Dtos;
using EVENTIA.Models;

namespace EVENTIA.ViewModels;

public class DisponibilidadViewModel
{
    public int? ArticuloSeleccionadoId { get; set; }
    public List<Articulo> Articulos { get; set; } = new();
    public Articulo? ArticuloSeleccionado { get; set; }
    public List<Disponibilidad> Disponibilidades { get; set; } = new();
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
}
