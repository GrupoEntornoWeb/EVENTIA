using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public string? Direccion { get; set; }

    public virtual Usuario ClienteNavigation { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
