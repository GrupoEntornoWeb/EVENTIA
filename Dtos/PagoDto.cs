using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class PagoDto
{
    public int PagoId { get; set; }

    public int PedidoId { get; set; }

    [Required(ErrorMessage = "Seleccione un método de pago")]
    public string MetodoPago { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el monto a pagar")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoPagado { get; set; }
}
