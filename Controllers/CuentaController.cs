using Microsoft.AspNetCore.Mvc;
using EVENTIA.Dtos;
using EVENTIA.Services;

namespace EVENTIA.Controllers;

public class CuentaController : Controller
{
    private readonly UsuarioService _usuarioService;

    public CuentaController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("Nombre") != null)
            return RedirectToAction("Index", "Home");

        return View(new LoginDto());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var usuario = await _usuarioService.Login(dto);
        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas");
            return View(dto);
        }

        HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Correo", usuario.Correo);
        HttpContext.Session.SetString("TipoPerfil", usuario.TipoPerfil);

        if (usuario.TipoPerfil == "Proveedor")
        {
            var proveedor = await _usuarioService.GetProveedorById(usuario.UsuarioId);
            if (proveedor != null)
                HttpContext.Session.SetInt32("ProveedorId", proveedor.ProveedorId);
        }
        else if (usuario.TipoPerfil == "Cliente")
        {
            var cliente = await _usuarioService.GetClienteById(usuario.UsuarioId);
            if (cliente != null)
                HttpContext.Session.SetInt32("ClienteId", cliente.ClienteId);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Registro()
    {
        return View(new RegistroDto());
    }

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var resultado = await _usuarioService.Registrar(dto);
        if (!resultado)
        {
            ModelState.AddModelError(string.Empty, "El correo ya se encuentra registrado");
            return View(dto);
        }

        TempData["Mensaje"] = "Registro exitoso. Ahora puede iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
