using Microsoft.AspNetCore.Mvc;
using EVENTIA.Extensions;
using EVENTIA.Services;
using EVENTIA.ViewModels;

namespace EVENTIA.Controllers;

public class CarritoController : Controller
{
    private readonly DisponibilidadService _disponibilidadService;
    private readonly PedidoService _pedidoService;
    private const string SessionKey = "Carrito";

    public CarritoController(DisponibilidadService disponibilidadService, PedidoService pedidoService)
    {
        _disponibilidadService = disponibilidadService;
        _pedidoService = pedidoService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        var carrito = GetCarrito();
        return View(carrito);
    }

    [HttpPost]
    public async Task<IActionResult> Agregar(int articuloId, string? nombre, decimal precio, int cantidad,
        int proveedorId, string? proveedorNombre, string? imagenUrl)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        if (cantidad < 1) cantidad = 1;

        var carrito = GetCarrito();

        var existente = carrito.Items.FirstOrDefault(i => i.ArticuloId == articuloId);
        if (existente != null)
        {
            existente.Cantidad += cantidad;
        }
        else
        {
            if (carrito.Items.Any() && carrito.Items.First().ProveedorId != proveedorId)
            {
                TempData["Error"] = "Solo puedes alquilar artículos de un mismo proveedor por pedido. Vacia el carrito primero.";
                return RedirectToAction("Index");
            }

            carrito.Items.Add(new CarritoItem
            {
                ArticuloId = articuloId,
                Nombre = nombre ?? "",
                ImagenUrl = imagenUrl,
                PrecioUnitario = precio,
                Cantidad = cantidad,
                ProveedorId = proveedorId,
                ProveedorNombre = proveedorNombre ?? ""
            });
        }

        SaveCarrito(carrito);
        TempData["Mensaje"] = $"Artículo \"{nombre}\" agregado al carrito.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Actualizar(int articuloId, int cantidad)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        var carrito = GetCarrito();
        var item = carrito.Items.FirstOrDefault(i => i.ArticuloId == articuloId);
        if (item != null)
        {
            if (cantidad <= 0)
                carrito.Items.Remove(item);
            else
                item.Cantidad = cantidad;
        }

        SaveCarrito(carrito);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Eliminar(int articuloId)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        var carrito = GetCarrito();
        carrito.Items.RemoveAll(i => i.ArticuloId == articuloId);
        SaveCarrito(carrito);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Vaciar()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult SeleccionarFecha()
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        var carrito = GetCarrito();
        if (!carrito.Items.Any()) return RedirectToAction("Index");

        return View(carrito);
    }

    [HttpPost]
    public async Task<IActionResult> Confirmar(DateOnly fechaEvento)
    {
        var clienteId = HttpContext.Session.GetInt32("ClienteId");
        if (clienteId == null) return RedirectToAction("Login", "Cuenta");

        var carrito = GetCarrito();
        if (!carrito.Items.Any())
        {
            TempData["Error"] = "El carrito está vacío.";
            return RedirectToAction("Index");
        }

        foreach (var item in carrito.Items)
        {
            var hayStock = await _disponibilidadService.HayDisponibilidad(item.ArticuloId, fechaEvento, item.Cantidad);
            if (!hayStock)
            {
                TempData["Error"] = $"No hay stock para \"{item.Nombre}\" en la fecha seleccionada.";
                return RedirectToAction("SeleccionarFecha");
            }
        }

        var items = carrito.Items.Select(i => new Dtos.DetallePedidoDto
        {
            ArticuloId = i.ArticuloId,
            Cantidad = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario,
            Subtotal = i.Subtotal
        }).ToList();

        var dto = new Dtos.PedidoDto
        {
            ClienteId = clienteId.Value,
            ProveedorId = carrito.ProveedorId!.Value,
            FechaEvento = fechaEvento,
            MontoTotal = carrito.MontoTotal
        };

        var pedidoId = await _pedidoService.Crear(dto, items);

        HttpContext.Session.Remove(SessionKey);

        TempData["Mensaje"] = "Pedido creado exitosamente.";
        return RedirectToAction("Detalle", "Pedido", new { id = pedidoId });
    }

    private CarritoViewModel GetCarrito()
    {
        return HttpContext.Session.GetObject<CarritoViewModel>(SessionKey) ?? new CarritoViewModel();
    }

    private void SaveCarrito(CarritoViewModel carrito)
    {
        HttpContext.Session.SetObject(SessionKey, carrito);
    }
}
