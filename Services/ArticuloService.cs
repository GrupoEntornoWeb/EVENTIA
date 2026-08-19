using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Repositories;

namespace EVENTIA.Services;

public class ArticuloService
{
    private readonly ArticuloRepository _articuloRepository;

    public ArticuloService(ArticuloRepository articuloRepository)
    {
        _articuloRepository = articuloRepository;
    }

    public async Task<List<ArticuloListItem>> Buscar(string? buscar, int? categoriaId, int? proveedorId, DateOnly? fecha = null)
    {
        return await _articuloRepository.Buscar(buscar, categoriaId, proveedorId, fecha);
    }

    public async Task<List<ArticuloListItem>> GetAll()
    {
        return await _articuloRepository.GetAll();
    }

    public async Task<List<ArticuloListItem>> GetByProveedor(int proveedorId)
    {
        return await _articuloRepository.GetByProveedor(proveedorId);
    }

    public async Task<Articulo?> GetById(int id)
    {
        return await _articuloRepository.GetById(id);
    }

    public async Task Crear(ArticuloDto dto)
    {
        await _articuloRepository.Crear(dto);
    }

    public async Task Actualizar(ArticuloDto dto)
    {
        await _articuloRepository.Actualizar(dto);
    }

    public async Task CambiarEstado(int id, bool activo)
    {
        await _articuloRepository.CambiarEstado(id, activo);
    }

    public async Task<int> GetProveedorIdByUsuarioId(int usuarioId)
    {
        return await _articuloRepository.GetProveedorIdByUsuarioId(usuarioId);
    }
}
