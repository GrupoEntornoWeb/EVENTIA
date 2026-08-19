using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class ArticuloDto
{
    public int ArticuloId { get; set; }

    [Required(ErrorMessage = "Ingrese el nombre del artículo")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Ingrese el precio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "Ingrese la cantidad disponible")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int CantidadTotal { get; set; }

    public string? ImagenUrl { get; set; }

    [Required(ErrorMessage = "Seleccione una categoría")]
    public int CategoriaId { get; set; }

    public int ProveedorId { get; set; }

    public bool Activo { get; set; } = true;
}
