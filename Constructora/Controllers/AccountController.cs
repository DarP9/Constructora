using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Constructora.Models;

namespace Constructora.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult TestHash()
        {
            return Content(
                "Admin2024! = " + HashPassword("Admin2024!") + "<br>" +
                "Empleado123 = " + HashPassword("Empleado123") + "<br>" +
                "Cliente123 = " + HashPassword("Cliente123")
            );
        }
        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            if (Session["IdUsuario"] != null)
                return RedirectSegunRol(Session["Rol"]?.ToString());

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = DbHelper.ObtenerUsuarioPorCorreo(model.Correo);

            if (usuario == null || usuario.Password != HashPassword(model.Password))
            {
                ModelState.AddModelError("", "Correo o contrasena incorrectos.");
                return View(model);
            }

            Session["IdUsuario"] = usuario.IdUsuario;
            Session["Nombre"] = usuario.Nombre;
            Session["Correo"] = usuario.Correo;
            Session["Rol"] = usuario.Rol ?? "cliente";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectSegunRol(usuario.Rol);
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            if (Session["IdUsuario"] != null)
                return RedirectSegunRol(Session["Rol"]?.ToString());

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (DbHelper.ObtenerUsuarioPorCorreo(model.Correo) != null)
            {
                ModelState.AddModelError("Correo", "Este correo ya esta registrado.");
                return View(model);
            }

            DbHelper.CrearUsuario(model.Nombre, model.Correo, HashPassword(model.Password), "cliente");

            TempData["Registro"] = "Cuenta creada correctamente. Ya puedes iniciar sesion.";
            return RedirectToAction("Login");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectSegunRol(string rol)
        {
            switch ((rol ?? "cliente").ToLower())
            {
                case "administrador": return RedirectToAction("Index", "Admin");
                case "empleado": return RedirectToAction("Index", "Empleado");
                default: return RedirectToAction("Index", "Cliente");
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
