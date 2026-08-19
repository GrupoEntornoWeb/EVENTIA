using EVENTIA.Data;
using EVENTIA.Dtos;
using EVENTIA.Models;
using Microsoft.EntityFrameworkCore;

namespace EVENTIA.Repositories;

public class DisponibilidadRepository
{
    private readonly AppDbContext _appDbContext;

    public DisponibilidadRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<List<Disponibilidad>> GetByArticulo(int articuloId)
    {
        return await _appDbContext.Disponibilidads
            .AsNoTracking()
            .Include(d => d.Articulo)
            .Where(d => d.ArticuloId == articuloId)
            .OrderBy(d => d.Fecha)
            .ToListAsync();
    }

    public async Task<Disponibilidad?> GetByArticuloFecha(int articuloId, DateOnly fecha)
    {
        return await _appDbContext.Disponibilidads
            .FirstOrDefaultAsync(d => d.ArticuloId == articuloId && d.Fecha == fecha);
    }

    public async Task Crear(DisponibilidadDto dto)
    {
        var disponibilidad = new Disponibilidad
        {
            ArticuloId = dto.ArticuloId,
            Fecha = dto.Fecha,
            CantidadDisponible = dto.CantidadDisponible,
            CantidadReservada = dto.CantidadReservada
        };

        _appDbContext.Disponibilidads.Add(disponibilidad);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task Actualizar(DisponibilidadDto dto)
    {
        var disponibilidad = await _appDbContext.Disponibilidads
            .FirstOrDefaultAsync(d => d.ArticuloId == dto.ArticuloId && d.Fecha == dto.Fecha);

        if (disponibilidad == null)
        {
            await Crear(dto);
            return;
        }

        disponibilidad.CantidadDisponible = dto.CantidadDisponible;
        disponibilidad.CantidadReservada = dto.CantidadReservada;

        await _appDbContext.SaveChangesAsync();
    }

    public async Task<bool> TieneReservasConfirmadas(int articuloId, DateOnly fecha)
    {
        return await _appDbContext.Pedidos
            .AnyAsync(p => p.Proveedor.Articulos.Any(a => a.ArticuloId == articuloId)
                && p.FechaEvento == fecha
                && (p.Estado == "Confirmado" || p.Estado == "Entregado"));
    }

    public async Task<bool> HayDisponibilidad(int articuloId, DateOnly fecha, int cantidad)
    {
        var disponibilidad = await GetByArticuloFecha(articuloId, fecha);
        if (disponibilidad == null) return false;

        return disponibilidad.CantidadDisponible - disponibilidad.CantidadReservada >= cantidad;
    }

    public async Task Reservar(int articuloId, DateOnly fecha, int cantidad)
    {
        var disponibilidad = await GetByArticuloFecha(articuloId, fecha);
        if (disponibilidad == null) return;

        disponibilidad.CantidadReservada += cantidad;
        await _appDbContext.SaveChangesAsync();
    }

    public async Task Liberar(int articuloId, DateOnly fecha, int cantidad)
    {
        var disponibilidad = await GetByArticuloFecha(articuloId, fecha);
        if (disponibilidad == null) return;

        disponibilidad.CantidadReservada = Math.Max(0, disponibilidad.CantidadReservada - cantidad);
        await _appDbContext.SaveChangesAsync();
    }
}
