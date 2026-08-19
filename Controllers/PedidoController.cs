using Microsoft.AspNetCore.Mvc;
using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Services;
using EVENTIA.ViewModels;

namespace EVENTIA.Controllers;

public class PedidoController : Controller
{
    private readonly PedidoService _pedidoService;
    private readonly PagoService _pagoService;
    private readonly ArticuloService _articuloService;
    private readonly DisponibilidadService _disponibilidadService;

    public PedidoController(
        PedidoService pedidoService,
        PagoService pagoService,
        ArticuloService articuloService,
        DisponibilidadService disponibilidadService)
    {
        _pedidoService = pedidoService;
        _pagoService = pagoService;
        _articuloService = articuloService;
        _disponibilidadService = disponibilidadService;
    }

    [HttpGet]
    public async Task<IActionResult> MisPedidos(string? estado, DateOnly? fecha, int pagina = 1)
    {
        var tipoPerfil = HttpContext.Session.GetString("TipoPerfil");
        if (tipoPerfil == null) return RedirectToAction("Login", "Cuenta");

        List<Models.Pedido> pedidos;
        if (tipoPerfil == "Proveedor")
        {
            var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
            if (proveedorId == null) return RedirectToAction("Login", "Cuenta");
            pedidos = await _pedidoService.GetByProveedor(proveedorId.Value);
        }
        else
        {
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            if (clienteId == null) return RedirectToAction("Login", "Cuenta");
            pedidos = await _pedidoService.GetByCliente(clienteId.Value);
        }

        if (!string.IsNullOrEmpty(estado))
            pedidos = pedidos.Where(p => p.Estado == estado).ToList();

        if (fecha.HasValue)
            pedidos = pedidos.Where(p => p.FechaEvento == fecha.Value).ToList();

        var totalRegistros = pedidos.Count;
        var tamanoPagina = 5;
        var totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
        if (pagina < 1) pagina = 1;
        if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

        var pedidosPagina = pedidos
            .OrderByDescending(p => p.FechaPedido)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        var viewModel = new MisPedidosViewModel
        {
            FiltroEstado = estado,
            FiltroFecha = fecha,
            Pedidos = pedidos,
            PedidosPagina = pedidosPagina,
            TipoPerfil = tipoPerfil,
            PaginaActual = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = totalRegistros
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioId == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(id);
        if (pedido == null) return NotFound();

        var tipoPerfil = HttpContext.Session.GetString("TipoPerfil");
        if (tipoPerfil == "Cliente" && pedido.ClienteId != HttpContext.Session.GetInt32("ClienteId"))
        {
            TempData["Error"] = "No tiene acceso a este pedido.";
            return RedirectToAction("MisPedidos");
        }

        if (tipoPerfil == "Proveedor" && pedido.ProveedorId != HttpContext.Session.GetInt32("ProveedorId"))
        {
            TempData["Error"] = "No tiene acceso a este pedido.";
            return RedirectToAction("Index", "Reportes");
        }

        var detalles = pedido.DetallePedidos.Select(d => new DetallePedidoDto
        {
            DetallePedidoId = d.DetallePedidoId,
            PedidoId = d.PedidoId,
            ArticuloId = d.ArticuloId,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList();

        var viewModel = new PedidoDetalleViewModel
        {
            Pedido = pedido,
            Detalles = detalles,
            Pago = pedido.Pago,
            EstadosDisponibles = GetEstadosDisponibles(pedido.Estado, tipoPerfil!)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Pago(int pedidoId)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(pedidoId);
        if (pedido == null || pedido.ClienteId != clienteId.Value) return NotFound();
        if (pedido.Estado != "Reservado") return RedirectToAction(nameof(Detalle), new { id = pedidoId });

        var dto = new PagoDto
        {
            PedidoId = pedidoId,
            MontoPagado = pedido.MontoTotal
        };

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Pago(PagoDto dto)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        if (!ModelState.IsValid) return View(dto);

        var pedido = await _pedidoService.GetById(dto.PedidoId);
        if (pedido == null || pedido.ClienteId != clienteId.Value)
            return NotFound();

        if (pedido.Estado != "Reservado")
        {
            TempData["Error"] = "Este pedido no se puede pagar.";
            return RedirectToAction(nameof(Detalle), new { id = dto.PedidoId });
        }

        if (pedido.Pago != null)
        {
            TempData["Error"] = "Este pedido ya tiene un pago registrado.";
            return RedirectToAction(nameof(Detalle), new { id = dto.PedidoId });
        }

        try
        {
            await _pagoService.ConfirmarPago(dto);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detalle), new { id = dto.PedidoId });
        }

        TempData["Mensaje"] = "Pago confirmado exitosamente.";
        return RedirectToAction(nameof(Detalle), new { id = dto.PedidoId });
    }

    [HttpPost]
    public async Task<IActionResult> Entregar(int id)
    {
        var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(id);
        if (pedido == null || pedido.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este pedido.";
            return RedirectToAction("Index", "Reportes");
        }

        var resultado = await _pedidoService.MarcarEntregado(id);
        if (!resultado) TempData["Error"] = "No se pudo marcar como entregado.";

        TempData["Mensaje"] = "Pedido marcado como entregado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Devolver(int id, string? observaciones)
    {
        var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(id);
        if (pedido == null || pedido.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este pedido.";
            return RedirectToAction("Index", "Reportes");
        }

        var resultado = await _pedidoService.RegistrarDevolucion(id);
        if (!resultado) TempData["Error"] = "No se pudo registrar la devolución.";

        TempData["Mensaje"] = "Devolución registrada.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        var tipoPerfil = HttpContext.Session.GetString("TipoPerfil");
        if (tipoPerfil == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(id);
        if (pedido == null)
        {
            TempData["Error"] = "Pedido no encontrado.";
            return RedirectToAction("MisPedidos");
        }

        if (tipoPerfil == "Cliente")
        {
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            if (pedido.ClienteId != clienteId)
            {
                TempData["Error"] = "No tiene acceso a este pedido.";
                return RedirectToAction("MisPedidos");
            }
        }
        else if (tipoPerfil == "Proveedor")
        {
            var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
            if (pedido.ProveedorId != proveedorId)
            {
                TempData["Error"] = "No tiene acceso a este pedido.";
                return RedirectToAction("Index", "Reportes");
            }
        }

        var resultado = await _pedidoService.Cancelar(id);
        if (!resultado) TempData["Error"] = "No se pudo cancelar el pedido.";

        TempData["Mensaje"] = "Pedido cancelado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Reprogramar(int id)
    {
        var tipoPerfil = HttpContext.Session.GetString("TipoPerfil");
        if (tipoPerfil == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(id);
        if (pedido == null) return NotFound();

        if (tipoPerfil == "Cliente")
        {
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            if (pedido.ClienteId != clienteId)
            {
                TempData["Error"] = "No tiene acceso a este pedido.";
                return RedirectToAction("MisPedidos");
            }
        }
        else if (tipoPerfil == "Proveedor")
        {
            var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
            if (pedido.ProveedorId != proveedorId)
            {
                TempData["Error"] = "No tiene acceso a este pedido.";
                return RedirectToAction("Index", "Reportes");
            }
        }

        if (pedido.Estado != "Reservado" && pedido.Estado != "Confirmado")
            return RedirectToAction(nameof(Detalle), new { id });

        var viewModel = new ReprogramarViewModel
        {
            PedidoId = pedido.PedidoId,
            FechaActual = pedido.FechaEvento,
            ProveedorNombre = pedido.Proveedor.NombreNegocio
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Reprogramar(ReprogramarViewModel viewModel)
    {
        var tipoPerfil = HttpContext.Session.GetString("TipoPerfil");
        if (tipoPerfil == null) return RedirectToAction("Login", "Cuenta");

        var pedido = await _pedidoService.GetById(viewModel.PedidoId);
        if (pedido == null)
        {
            TempData["Error"] = "Pedido no encontrado.";
            return RedirectToAction("MisPedidos");
        }

        if (tipoPerfil == "Cliente")
        {
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            if (pedido.ClienteId != clienteId)
            {
                TempData["Error"] = "No tiene acceso a este pedido.";
                return RedirectToAction("MisPedidos");
            }
        }
        else if (tipoPerfil == "Proveedor")
        {
            var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
            if (pedido.ProveedorId != proveedorId)
            {
                TempData["Error"] = "No tiene acceso a este pedido.";
                return RedirectToAction("Index", "Reportes");
            }
        }

        var resultado = await _pedidoService.Reprogramar(viewModel.PedidoId, viewModel.NuevaFecha);
        if (!resultado)
        {
            TempData["Error"] = "La nueva fecha no tiene disponibilidad.";
            return RedirectToAction(nameof(Reprogramar), new { id = viewModel.PedidoId });
        }

        TempData["Mensaje"] = "Pedido reprogramado exitosamente.";
        return RedirectToAction(nameof(Detalle), new { id = viewModel.PedidoId });
    }

    [HttpPost]
    public async Task<IActionResult> Crear(int proveedorId, DateOnly fechaEvento, List<DetallePedidoDto> items)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        if (items == null || !items.Any())
        {
            TempData["Error"] = "El carrito está vacío.";
            return RedirectToAction("Buscar", "Catalogo");
        }

        foreach (var item in items)
        {
            var hayStock = await _disponibilidadService.HayDisponibilidad(item.ArticuloId, fechaEvento, item.Cantidad);
            if (!hayStock)
            {
                TempData["Error"] = $"No hay stock suficiente para el artículo ID {item.ArticuloId} en la fecha seleccionada.";
                return RedirectToAction("Buscar", "Catalogo");
            }
        }

        var dto = new PedidoDto
        {
            ClienteId = clienteId.Value,
            ProveedorId = proveedorId,
            FechaEvento = fechaEvento,
            MontoTotal = items.Sum(i => i.Subtotal)
        };

        var pedidoId = await _pedidoService.Crear(dto, items);
        TempData["Mensaje"] = "Pedido creado exitosamente.";
        return RedirectToAction(nameof(Detalle), new { id = pedidoId });
    }

    private List<string> GetEstadosDisponibles(string estadoActual, string tipoPerfil)
    {
        var estados = new List<string>();

        if (tipoPerfil == "Cliente")
        {
            if (estadoActual == "Reservado")
            {
                estados.Add("Confirmar Pago");
                estados.Add("Reprogramar");
                estados.Add("Cancelar");
            }
            else if (estadoActual == "Confirmado")
            {
                estados.Add("Reprogramar");
                estados.Add("Cancelar");
            }
        }
        else if (tipoPerfil == "Proveedor")
        {
            if (estadoActual == "Confirmado")
            {
                estados.Add("Marcar Entregado");
            }
            else if (estadoActual == "Entregado")
            {
                estados.Add("Registrar Devolución");
            }

            if (estadoActual == "Reservado" || estadoActual == "Confirmado")
            {
                estados.Add("Cancelar");
            }
        }

        return estados;
    }
}
