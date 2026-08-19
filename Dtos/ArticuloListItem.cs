namespace EVENTIA.Dtos;

public record ArticuloListItem
{
    public int ArticuloId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal Precio { get; init; }
    public int CantidadTotal { get; init; }
    public string? ImagenUrl { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public string ProveedorNombre { get; init; } = string.Empty;
    public int ProveedorId { get; init; }
    public bool Activo { get; init; }
    public int? CantidadDisponible { get; init; }
    public bool? DisponibleParaFecha { get; init; }
}
