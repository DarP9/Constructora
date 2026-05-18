using System.Collections.Generic;
using System.Web.Mvc;

namespace Constructora.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Pagina = "inicio";
            return View();
        }
    }

    public class CatalogoController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Pagina = "catalogo";
            try
            {
                var proyectos = Constructora.Models.DbHelper.ListarProyectos();
                return View(proyectos);
            }
            catch
            {
                return View(new System.Collections.Generic.List<Constructora.Models.Proyecto>());
            }
        }
    }
}