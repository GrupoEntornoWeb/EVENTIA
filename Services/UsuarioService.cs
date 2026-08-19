using EVENTIA.Dtos;
using EVENTIA.Models;
using EVENTIA.Repositories;

namespace EVENTIA.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _usuarioRepository;

    public UsuarioService(UsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Usuario?> Login(LoginDto dto)
    {
        var usuario = await _usuarioRepository.GetByCorreo(dto.Correo);
        if (usuario == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Contrasena, usuario.ContrasenaHash))
            return null;

        return usuario;
    }

    public async Task<bool> Registrar(RegistroDto dto)
    {
        var existente = await _usuarioRepository.GetByCorreo(dto.Correo);
        if (existente != null) return false;

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
            Telefono = dto.Telefono,
            TipoPerfil = dto.TipoPerfil,
            FechaRegistro = DateTime.Now
        };

        await _usuarioRepository.Registrar(usuario);

        if (dto.TipoPerfil == "Cliente")
        {
            var cliente = new Cliente
            {
                ClienteId = usuario.UsuarioId,
                Direccion = dto.Direccion
            };
            await _usuarioRepository.RegistrarCliente(cliente);
        }
        else if (dto.TipoPerfil == "Proveedor")
        {
            var proveedor = new Proveedor
            {
                ProveedorId = usuario.UsuarioId,
                NombreNegocio = dto.NombreNegocio ?? string.Empty,
                Direccion = dto.Direccion
            };
            await _usuarioRepository.RegistrarProveedor(proveedor);
        }

        return true;
    }

    public async Task<Cliente?> GetClienteById(int id)
    {
        return await _usuarioRepository.GetClienteById(id);
    }

    public async Task<Proveedor?> GetProveedorById(int id)
    {
        return await _usuarioRepository.GetProveedorById(id);
    }
}
