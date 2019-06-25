using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    public class Fallback : Controller
    {
        public IActionResult Index() 
        {
            // configura a URL retornada com 'path' (caminho) real do arquivo com index.html e passa o tipo
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}