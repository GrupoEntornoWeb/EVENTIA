using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public string? Telefono { get; set; }

    public string TipoPerfil { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual Proveedor? Proveedor { get; set; }
}
