using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class DisponibilidadDto
{
    public int DisponibilidadId { get; set; }

    [Required(ErrorMessage = "Seleccione un artículo")]
    public int ArticuloId { get; set; }

    [Required(ErrorMessage = "Seleccione una fecha")]
    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "Ingrese la cantidad disponible")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
    public int CantidadDisponible { get; set; }

    public int CantidadReservada { get; set; } = 0;
}
