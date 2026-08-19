using EVENTIA.Data;
using EVENTIA.Models;
using Microsoft.EntityFrameworkCore;

namespace EVENTIA.Repositories;

public class CategoriaRepository
{
    private readonly AppDbContext _appDbContext;

    public CategoriaRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<List<Categorium>> GetAll()
    {
        return await _appDbContext.Categoria
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Categorium?> GetById(int id)
    {
        return await _appDbContext.Categoria.FindAsync(id);
    }
}
