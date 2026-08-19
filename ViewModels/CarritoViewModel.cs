namespace EVENTIA.ViewModels;

public class CarritoViewModel
{
    public List<CarritoItem> Items { get; set; } = new();
    public decimal MontoTotal => Items.Sum(i => i.Subtotal);
    public DateOnly? FechaEvento { get; set; }
    public int? ProveedorId => Items.FirstOrDefault()?.ProveedorId;
    public string? ProveedorNombre => Items.FirstOrDefault()?.ProveedorNombre;
    public int ItemCount => Items.Count;
}
