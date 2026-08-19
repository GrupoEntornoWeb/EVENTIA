using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class PedidoDto
{
    public int PedidoId { get; set; }

    [Required(ErrorMessage = "Seleccione la fecha del evento")]
    public DateOnly FechaEvento { get; set; }

    public int ClienteId { get; set; }

    public int ProveedorId { get; set; }

    public string Estado { get; set; } = "Reservado";

    public decimal MontoTotal { get; set; }
}
