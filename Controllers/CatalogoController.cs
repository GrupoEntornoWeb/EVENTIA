using Microsoft.AspNetCore.Mvc;
using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Services;
using EVENTIA.ViewModels;

namespace EVENTIA.Controllers;

public class CatalogoController : Controller
{
    private readonly ArticuloService _articuloService;
    private readonly CategoriaService _categoriaService;
    private readonly DisponibilidadService _disponibilidadService;

    public CatalogoController(
        ArticuloService articuloService,
        CategoriaService categoriaService,
        DisponibilidadService disponibilidadService)
    {
        _articuloService = articuloService;
        _categoriaService = categoriaService;
        _disponibilidadService = disponibilidadService;
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(string? buscar, int? categoriaId, DateOnly? fecha, int? proveedorId)
    {
        var articulos = await _articuloService.Buscar(buscar, categoriaId, proveedorId, fecha);
        var categorias = await _categoriaService.GetAll();
        var todosArticulos = await _articuloService.GetAll();
        var proveedores = todosArticulos
            .Select(a => new Proveedor { ProveedorId = a.ProveedorId, NombreNegocio = a.ProveedorNombre })
            .GroupBy(p => p.ProveedorId)
            .Select(g => g.First())
            .ToList();

        var viewModel = new BuscarCatalogoViewModel
        {
            Buscar = buscar,
            CategoriaId = categoriaId,
            FechaEvento = fecha,
            ProveedorId = proveedorId,
            Articulos = articulos,
            Categorias = categorias,
            Proveedores = proveedores
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id, DateOnly? fecha)
    {
        var articulo = await _articuloService.GetById(id);
        if (articulo == null) return NotFound();

        var disponibilidad = fecha.HasValue
            ? await _disponibilidadService.GetByArticuloFecha(id, fecha.Value)
            : null;

        ViewBag.Disponibilidad = disponibilidad;
        ViewBag.FechaSeleccionada = fecha;

        var item = new ArticuloListItem
        {
            ArticuloId = articulo.ArticuloId,
            Nombre = articulo.Nombre,
            Descripcion = articulo.Descripcion,
            Precio = articulo.Precio,
            CantidadTotal = articulo.CantidadTotal,
            ImagenUrl = articulo.ImagenUrl,
            CategoriaNombre = articulo.Categoria?.Nombre ?? "",
            ProveedorNombre = articulo.Proveedor?.NombreNegocio ?? "",
            ProveedorId = articulo.ProveedorId,
            Activo = articulo.Activo
        };

        return View(item);
    }
}
