using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Pago
{
    public int PagoId { get; set; }

    public int PedidoId { get; set; }

    public string MetodoPago { get; set; } = null!;

    public decimal MontoPagado { get; set; }

    public DateTime FechaPago { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Pedido Pedido { get; set; } = null!;
}
