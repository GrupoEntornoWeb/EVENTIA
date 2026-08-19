using EVENTIA.Dtos;
using EVENTIA.Models;

namespace EVENTIA.ViewModels;

public class BuscarCatalogoViewModel
{
    public string? Buscar { get; set; }
    public int? CategoriaId { get; set; }
    public DateOnly? FechaEvento { get; set; }
    public int? ProveedorId { get; set; }

    public List<ArticuloListItem> Articulos { get; set; } = new();
    public List<Categorium> Categorias { get; set; } = new();
    public List<Proveedor> Proveedores { get; set; } = new();
}
