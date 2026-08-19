using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Articulo
{
    public int ArticuloId { get; set; }

    public int ProveedorId { get; set; }

    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int CantidadTotal { get; set; }

    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; }

    public virtual Categorium Categoria { get; set; } = null!;

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual ICollection<Disponibilidad> Disponibilidads { get; set; } = new List<Disponibilidad>();

    public virtual Proveedor Proveedor { get; set; } = null!;
}
