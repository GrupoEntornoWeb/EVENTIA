using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class DetallePedido
{
    public int DetallePedidoId { get; set; }

    public int PedidoId { get; set; }

    public int ArticuloId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Articulo Articulo { get; set; } = null!;

    public virtual Pedido Pedido { get; set; } = null!;
}
