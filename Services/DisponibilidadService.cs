using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Repositories;

namespace EVENTIA.Services;

public class DisponibilidadService
{
    private readonly DisponibilidadRepository _disponibilidadRepository;

    public DisponibilidadService(DisponibilidadRepository disponibilidadRepository)
    {
        _disponibilidadRepository = disponibilidadRepository;
    }

    public async Task<List<Disponibilidad>> GetByArticulo(int articuloId)
    {
        return await _disponibilidadRepository.GetByArticulo(articuloId);
    }

    public async Task<Disponibilidad?> GetByArticuloFecha(int articuloId, DateOnly fecha)
    {
        return await _disponibilidadRepository.GetByArticuloFecha(articuloId, fecha);
    }

    public async Task Actualizar(DisponibilidadDto dto)
    {
        await _disponibilidadRepository.Actualizar(dto);
    }

    public async Task<bool> TieneReservasConfirmadas(int articuloId, DateOnly fecha)
    {
        return await _disponibilidadRepository.TieneReservasConfirmadas(articuloId, fecha);
    }

    public async Task<bool> HayDisponibilidad(int articuloId, DateOnly fecha, int cantidad)
    {
        return await _disponibilidadRepository.HayDisponibilidad(articuloId, fecha, cantidad);
    }

    public async Task Reservar(int articuloId, DateOnly fecha, int cantidad)
    {
        await _disponibilidadRepository.Reservar(articuloId, fecha, cantidad);
    }

    public async Task Liberar(int articuloId, DateOnly fecha, int cantidad)
    {
        await _disponibilidadRepository.Liberar(articuloId, fecha, cantidad);
    }
}
