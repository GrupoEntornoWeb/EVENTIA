using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class RegistroDto
{
    [Required(ErrorMessage = "Ingrese su nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su correo")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Contrasena { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Seleccione su perfil")]
    public string TipoPerfil { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public string? NombreNegocio { get; set; }
}
