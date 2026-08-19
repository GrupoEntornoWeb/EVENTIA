using EVENTIA.Data;
using EVENTIA.Dtos;
using EVENTIA.Models;
using Microsoft.EntityFrameworkCore;

namespace EVENTIA.Repositories;

public class ArticuloRepository
{
    private readonly AppDbContext _appDbContext;

    public ArticuloRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<List<ArticuloListItem>> Buscar(string? buscar, int? categoriaId, int? proveedorId, DateOnly? fecha)
    {
        var query = _appDbContext.Articulos
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .Where(a => a.Activo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(buscar))
            query = query.Where(a => a.Nombre.Contains(buscar) || (a.Descripcion != null && a.Descripcion.Contains(buscar)));

        if (categoriaId.HasValue)
            query = query.Where(a => a.CategoriaId == categoriaId.Value);

        if (proveedorId.HasValue)
            query = query.Where(a => a.ProveedorId == proveedorId.Value);

        var articulos = await query.OrderBy(a => a.Nombre).ToListAsync();

        var result = articulos.Select(a =>
        {
            int? cantDisp = null;
            bool? disp = null;
            if (fecha.HasValue)
            {
                var d = _appDbContext.Disponibilidads
                    .FirstOrDefault(x => x.ArticuloId == a.ArticuloId && x.Fecha == fecha.Value);
                if (d != null)
                {
                    cantDisp = d.CantidadDisponible - d.CantidadReservada;
                    disp = cantDisp > 0;
                }
                else
                {
                    cantDisp = 0;
                    disp = false;
                }
            }

            return new ArticuloListItem
            {
                ArticuloId = a.ArticuloId,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Precio = a.Precio,
                CantidadTotal = a.CantidadTotal,
                ImagenUrl = a.ImagenUrl,
                CategoriaNombre = a.Categoria.Nombre,
                ProveedorNombre = a.Proveedor.NombreNegocio,
                ProveedorId = a.ProveedorId,
                Activo = a.Activo,
                CantidadDisponible = cantDisp,
                DisponibleParaFecha = disp
            };
        }).ToList();

        return result;
    }

    public async Task<List<ArticuloListItem>> GetAll()
    {
        return await _appDbContext.Articulos
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .OrderBy(a => a.Nombre)
            .Select(a => new ArticuloListItem
            {
                ArticuloId = a.ArticuloId,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Precio = a.Precio,
                CantidadTotal = a.CantidadTotal,
                ImagenUrl = a.ImagenUrl,
                CategoriaNombre = a.Categoria.Nombre,
                ProveedorNombre = a.Proveedor.NombreNegocio,
                ProveedorId = a.ProveedorId,
                Activo = a.Activo
            })
            .ToListAsync();
    }

    public async Task<List<ArticuloListItem>> GetByProveedor(int proveedorId)
    {
        return await _appDbContext.Articulos
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .Where(a => a.ProveedorId == proveedorId)
            .OrderBy(a => a.Nombre)
            .Select(a => new ArticuloListItem
            {
                ArticuloId = a.ArticuloId,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Precio = a.Precio,
                CantidadTotal = a.CantidadTotal,
                ImagenUrl = a.ImagenUrl,
                CategoriaNombre = a.Categoria.Nombre,
                ProveedorNombre = a.Proveedor.NombreNegocio,
                ProveedorId = a.ProveedorId,
                Activo = a.Activo
            })
            .ToListAsync();
    }

    public async Task<Articulo?> GetById(int id)
    {
        return await _appDbContext.Articulos
            .Include(a => a.Categoria)
            .Include(a => a.Proveedor).ThenInclude(p => p.ProveedorNavigation)
            .FirstOrDefaultAsync(a => a.ArticuloId == id);
    }

    public async Task Crear(ArticuloDto dto)
    {
        var articulo = new Articulo
        {
            ProveedorId = dto.ProveedorId,
            CategoriaId = dto.CategoriaId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            CantidadTotal = dto.CantidadTotal,
            ImagenUrl = dto.ImagenUrl,
            Activo = dto.Activo
        };

        _appDbContext.Articulos.Add(articulo);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task Actualizar(ArticuloDto dto)
    {
        var articulo = await _appDbContext.Articulos.FindAsync(dto.ArticuloId);
        if (articulo == null) return;

        articulo.Nombre = dto.Nombre;
        articulo.Descripcion = dto.Descripcion;
        articulo.Precio = dto.Precio;
        articulo.CantidadTotal = dto.CantidadTotal;
        articulo.ImagenUrl = dto.ImagenUrl;
        articulo.CategoriaId = dto.CategoriaId;
        articulo.Activo = dto.Activo;

        await _appDbContext.SaveChangesAsync();
    }

    public async Task CambiarEstado(int id, bool activo)
    {
        var articulo = await _appDbContext.Articulos.FindAsync(id);
        if (articulo == null) return;

        articulo.Activo = activo;
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<int> GetProveedorIdByUsuarioId(int usuarioId)
    {
        var proveedor = await _appDbContext.Proveedors.FindAsync(usuarioId);
        return proveedor?.ProveedorId ?? 0;
    }
}
