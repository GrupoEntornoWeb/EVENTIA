using System.ComponentModel.DataAnnotations;

namespace EVENTIA.Dtos;

public class LoginDto
{
    [Required(ErrorMessage = "Ingrese su correo")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña")]
    public string Contrasena { get; set; } = string.Empty;
}
