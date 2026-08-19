using EVENTIA.Data;
using EVENTIA.Models;
using Microsoft.EntityFrameworkCore;

namespace EVENTIA.Repositories;

public class UsuarioRepository
{
    private readonly AppDbContext _appDbContext;

    public UsuarioRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Usuario?> GetByCorreo(string correo)
    {
        return await _appDbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task<Usuario?> GetById(int id)
    {
        return await _appDbContext.Usuarios.FindAsync(id);
    }

    public async Task Registrar(Usuario usuario)
    {
        _appDbContext.Usuarios.Add(usuario);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task RegistrarCliente(Cliente cliente)
    {
        _appDbContext.Clientes.Add(cliente);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task RegistrarProveedor(Proveedor proveedor)
    {
        _appDbContext.Proveedors.Add(proveedor);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<Cliente?> GetClienteById(int id)
    {
        return await _appDbContext.Clientes.FindAsync(id);
    }

    public async Task<Proveedor?> GetProveedorById(int id)
    {
        return await _appDbContext.Proveedors.FindAsync(id);
    }
}
