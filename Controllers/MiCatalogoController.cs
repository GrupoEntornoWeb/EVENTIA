using Microsoft.AspNetCore.Mvc;
using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Services;
using EVENTIA.ViewModels;

namespace EVENTIA.Controllers;

public class MiCatalogoController : Controller
{
    private readonly ArticuloService _articuloService;
    private readonly CategoriaService _categoriaService;

    public MiCatalogoController(ArticuloService articuloService, CategoriaService categoriaService)
    {
        _articuloService = articuloService;
        _categoriaService = categoriaService;
    }

    private int? GetProveedorId()
    {
        return HttpContext.Session.GetInt32("ProveedorId");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulos = await _articuloService.GetByProveedor(proveedorId.Value);
        return View(articulos);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        ViewBag.Categorias = await _categoriaService.GetAll();
        var dto = new ArticuloDto { ProveedorId = proveedorId.Value };
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ArticuloDto dto)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        dto.ProveedorId = proveedorId.Value;

        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categoriaService.GetAll();
            return View(dto);
        }

        await _articuloService.Crear(dto);
        TempData["Mensaje"] = "Artículo creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulo = await _articuloService.GetById(id);
        if (articulo == null || articulo.ProveedorId != proveedorId.Value)
            return NotFound();

        ViewBag.Categorias = await _categoriaService.GetAll();
        var dto = new ArticuloDto
        {
            ArticuloId = articulo.ArticuloId,
            Nombre = articulo.Nombre,
            Descripcion = articulo.Descripcion,
            Precio = articulo.Precio,
            CantidadTotal = articulo.CantidadTotal,
            ImagenUrl = articulo.ImagenUrl,
            CategoriaId = articulo.CategoriaId,
            ProveedorId = articulo.ProveedorId,
            Activo = articulo.Activo
        };

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(ArticuloDto dto)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulo = await _articuloService.GetById(dto.ArticuloId);
        if (articulo == null || articulo.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este artículo.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categoriaService.GetAll();
            return View(dto);
        }

        await _articuloService.Actualizar(dto);
        TempData["Mensaje"] = "Artículo actualizado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activo)
    {
        var proveedorId = GetProveedorId();
        if (proveedorId == null) return RedirectToAction("Login", "Cuenta");

        var articulo = await _articuloService.GetById(id);
        if (articulo == null || articulo.ProveedorId != proveedorId.Value)
        {
            TempData["Error"] = "No tiene acceso a este artículo.";
            return RedirectToAction(nameof(Index));
        }

        await _articuloService.CambiarEstado(id, !activo);
        TempData["Mensaje"] = activo ? "Artículo desactivado." : "Artículo activado.";
        return RedirectToAction(nameof(Index));
    }
}
