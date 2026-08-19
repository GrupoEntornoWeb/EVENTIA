using EVENTIA.Models;
using EVENTIA.Repositories;

namespace EVENTIA.Services;

public class CategoriaService
{
    private readonly CategoriaRepository _categoriaRepository;

    public CategoriaService(CategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<List<Categorium>> GetAll()
    {
        return await _categoriaRepository.GetAll();
    }

    public async Task<Categorium?> GetById(int id)
    {
        return await _categoriaRepository.GetById(id);
    }
}
