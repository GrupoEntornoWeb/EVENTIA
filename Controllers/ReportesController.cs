using Microsoft.AspNetCore.Mvc;
using EVENTIA.Services;
using EVENTIA.ViewModels;

namespace EVENTIA.Controllers;

public class ReportesController : Controller
{
    private readonly PedidoService _pedidoService;
    private readonly ArticuloService _articuloService;

    public ReportesController(PedidoService pedidoService, ArticuloService articuloService)
    {
        _pedidoService = pedidoService;
        _articuloService = articuloService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? fechaInicio, DateOnly? fechaFin, string? estado, int? articuloId, int pagina = 1)
    {
        var proveedorId = HttpContext.Session.GetInt32("ProveedorId");
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var pedidos = await _pedidoService.GetByProveedorFiltrado(proveedorId.Value, estado, fechaInicio, fechaFin);

        if (articuloId.HasValue)
        {
            pedidos = pedidos.Where(p =>
                p.DetallePedidos.Any(d => d.ArticuloId == articuloId.Value)).ToList();
        }

        var articulos = await _articuloService.GetByProveedor(proveedorId.Value);
        var articulosEntidad = articulos.Select(a => new Models.Articulo
        {
            ArticuloId = a.ArticuloId,
            Nombre = a.Nombre
        }).ToList();

        var viewModel = new ReporteViewModel
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            FiltroEstado = estado,
            FiltroArticuloId = articuloId,
            Pedidos = pedidos,
            Articulos = articulosEntidad,
            PaginaActual = Math.Max(1, Math.Min(pagina, Math.Max(1, (int)Math.Ceiling((double)pedidos.Count / 5))))
        };

        return View(viewModel);
    }
}
