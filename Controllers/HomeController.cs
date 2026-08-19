using Microsoft.AspNetCore.Mvc;

namespace EVENTIA.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Nombre") != null)
        {
            var tipoPerfil = HttpContext.Session.GetString("TipoPerfil");
            if (tipoPerfil == "Proveedor")
                return RedirectToAction("Index", "MiCatalogo");
            else
                return RedirectToAction("Buscar", "Catalogo");
        }

        return View();
    }
}
