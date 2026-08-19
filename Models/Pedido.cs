using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Pedido
{
    public int PedidoId { get; set; }

    public int ClienteId { get; set; }

    public int ProveedorId { get; set; }

    public DateTime FechaPedido { get; set; }

    public DateOnly FechaEvento { get; set; }

    public string Estado { get; set; } = null!;

    public decimal MontoTotal { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual Pago? Pago { get; set; }

    public virtual Proveedor Proveedor { get; set; } = null!;
}
