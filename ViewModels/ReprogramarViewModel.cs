using System.ComponentModel.DataAnnotations;

namespace EVENTIA.ViewModels;

public class ReprogramarViewModel
{
    public int PedidoId { get; set; }

    public DateOnly FechaActual { get; set; }

    public string ProveedorNombre { get; set; } = null!;

    [Required(ErrorMessage = "Seleccione la nueva fecha del evento")]
    [Display(Name = "Nueva fecha")]
    public DateOnly NuevaFecha { get; set; }
}
