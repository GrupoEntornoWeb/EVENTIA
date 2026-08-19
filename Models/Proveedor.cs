using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Proveedor
{
    public int ProveedorId { get; set; }

    public string NombreNegocio { get; set; } = null!;

    public string? Direccion { get; set; }

    public virtual ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual Usuario ProveedorNavigation { get; set; } = null!;
}
