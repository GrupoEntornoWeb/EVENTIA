using System;
using System.Collections.Generic;

namespace EVENTIA.Models;

public partial class Disponibilidad
{
    public int DisponibilidadId { get; set; }

    public int ArticuloId { get; set; }

    public DateOnly Fecha { get; set; }

    public int CantidadDisponible { get; set; }

    public int CantidadReservada { get; set; }

    public virtual Articulo Articulo { get; set; } = null!;
}
