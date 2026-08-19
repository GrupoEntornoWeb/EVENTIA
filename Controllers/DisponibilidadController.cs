using Microsoft.AspNetCore.Mvc;
using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Services;
using EVENTIA.ViewModels;

namespace EVENTIA.Controllers;

public class DisponibilidadController : Controller
{
    private readonly DisponibilidadService _disponibilidadService;
    private readonly ArticuloService _articuloService;

    public DisponibilidadController(DisponibilidadService disponibilidadService, ArticuloService articuloService)
    {
        _disponibilidadService = disponibilidadService;
        _articuloService = articuloService;
    }

    private int? GetProveedorId()
    {
        return HttpContext.Session.GetInt32("ProveedorId");
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? articuloId, DateOnly? fechaInicio)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulos = await _articuloService.GetByProveedor(proveedorId.Value);
        var articulosEntidad = articulos.Select(a => new Articulo
        {
            ArticuloId = a.ArticuloId,
            Nombre = a.Nombre
        }).ToList();

        var viewModel = new DisponibilidadViewModel
        {
            ArticuloSeleccionadoId = articuloId,
            Articulos = articulosEntidad,
            FechaInicio = fechaInicio
        };

        if (articuloId.HasValue)
        {
            viewModel.ArticuloSeleccionado = await _articuloService.GetById(articuloId.Value);
            viewModel.Disponibilidades = await _disponibilidadService.GetByArticulo(articuloId.Value);
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(DisponibilidadDto dto)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulo = await _articuloService.GetById(dto.ArticuloId);
        if (articulo == null || articulo.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este artículo.";
            return RedirectToAction(nameof(Index));
        }

        await _disponibilidadService.Actualizar(dto);
        TempData["Mensaje"] = "Disponibilidad actualizada.";
        return RedirectToAction(nameof(Index), new { articuloId = dto.ArticuloId });
    }

    [HttpPost]
    public async Task<IActionResult> BloquearFecha(int articuloId, DateOnly fecha)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulo = await _articuloService.GetById(articuloId);
        if (articulo == null || articulo.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este artículo.";
            return RedirectToAction(nameof(Index));
        }

        var tieneReservas = await _disponibilidadService.TieneReservasConfirmadas(articuloId, fecha);
        if (tieneReservas)
        {
            TempData["Error"] = "No se puede bloquear una fecha con reservas confirmadas.";
            return RedirectToAction(nameof(Index), new { articuloId });
        }

        var dto = new DisponibilidadDto
        {
            ArticuloId = articuloId,
            Fecha = fecha,
            CantidadDisponible = 0,
            CantidadReservada = 1
        };

        await _disponibilidadService.Actualizar(dto);
        TempData["Mensaje"] = "Fecha bloqueada.";
        return RedirectToAction(nameof(Index), new { articuloId });
    }

    [HttpPost]
    public async Task<IActionResult> DesbloquearFecha(int articuloId, DateOnly fecha)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulo = await _articuloService.GetById(articuloId);
        if (articulo == null || articulo.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este articulo.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new DisponibilidadDto
        {
            ArticuloId = articuloId,
            Fecha = fecha,
            CantidadDisponible = articulo.CantidadTotal,
            CantidadReservada = 0
        };

        await _disponibilidadService.Actualizar(dto);
        TempData["Mensaje"] = "Fecha desbloqueada.";
        return RedirectToAction(nameof(Index), new { articuloId });
    }
}
