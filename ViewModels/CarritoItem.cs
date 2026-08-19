namespace EVENTIA.ViewModels;

public class CarritoItem
{
    public int ArticuloId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public decimal Subtotal => PrecioUnitario * Cantidad;
}
