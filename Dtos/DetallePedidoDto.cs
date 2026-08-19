using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class DetallePedidoDto
{
    public int DetallePedidoId { get; set; }

    public int PedidoId { get; set; }

    [Required(ErrorMessage = "Seleccione un artículo")]
    public int ArticuloId { get; set; }

    [Required(ErrorMessage = "Ingrese la cantidad")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public string? ArticuloNombre { get; set; }
}
