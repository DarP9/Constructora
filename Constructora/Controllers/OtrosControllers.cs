using System;
using System.Web;
using System.Web.Mvc;
using Constructora.Models;

namespace Constructora.Controllers
{
    public class AutorizarRolAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;
        public AutorizarRolAttribute(params string[] roles) { _roles = roles; }

        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var session = ctx.HttpContext.Session;
            if (session["IdUsuario"] == null)
            {
                ctx.Result = new RedirectResult("~/Account/Login?returnUrl=" +
                    HttpUtility.UrlEncode(ctx.HttpContext.Request.RawUrl));
                return;
            }
            var rol = session["Rol"]?.ToString() ?? "cliente";
            bool permitido = false;
            foreach (var r in _roles)
                if (string.Equals(r, rol, StringComparison.OrdinalIgnoreCase))
                { permitido = true; break; }

            if (!permitido)
                ctx.Result = new HttpStatusCodeResult(403, "Sin permiso.");
        }
    }

    [AutorizarRol("administrador")]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Pagina = "admin-dashboard";
            ViewBag.TotalClientes = DbHelper.ListarClientes().Count;
            ViewBag.TotalEmpleados = DbHelper.ListarEmpleados().Count;
            ViewBag.TotalProyectos = DbHelper.ListarProyectos().Count;
            ViewBag.Finanzas = DbHelper.ListarResumenFinanciero();
            return View();
        }

        public ActionResult Clientes()
        {
            ViewBag.Pagina = "admin-clientes";
            return View(DbHelper.ListarClientes());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearCliente(Cliente c)
        {
            if (ModelState.IsValid) DbHelper.CrearCliente(c);
            return RedirectToAction("Clientes");
        }

        public ActionResult Empleados()
        {
            ViewBag.Pagina = "admin-empleados";
            return View(DbHelper.ListarEmpleados());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearEmpleado(Empleado e)
        {
            if (ModelState.IsValid) DbHelper.CrearEmpleado(e);
            return RedirectToAction("Empleados");
        }

        public ActionResult Proveedores()
        {
            ViewBag.Pagina = "admin-prov";
            return View(DbHelper.ListarProveedores());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearProveedor(Proveedor p)
        {
            if (ModelState.IsValid) DbHelper.CrearProveedor(p);
            return RedirectToAction("Proveedores");
        }

        public ActionResult Proyectos()
        {
            ViewBag.Pagina = "admin-proyectos";
            ViewBag.Clientes = DbHelper.ListarClientes();
            return View(DbHelper.ListarProyectos());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearProyecto(Proyecto p)
        {
            if (ModelState.IsValid) DbHelper.CrearProyecto(p);
            return RedirectToAction("Proyectos");
        }

        public ActionResult Avances()
        {
            ViewBag.Pagina = "admin-avances";
            return View(DbHelper.ListarAvances());
        }

        public ActionResult Facturas()
        {
            ViewBag.Pagina = "admin-facturas";
            return View(DbHelper.ListarFacturas());
        }

        public ActionResult Finanzas()
        {
            ViewBag.Pagina = "admin-finanzas";
            return View(DbHelper.ListarResumenFinanciero());
        }

        public ActionResult Materiales()
        {
            ViewBag.Pagina = "admin-mat";
            return View(DbHelper.ListarMateriales());
        }

        public ActionResult Compras()
        {
            ViewBag.Pagina = "admin-compras";
            return View(DbHelper.ListarCompras());
        }

        public ActionResult Usuarios()
        {
            ViewBag.Pagina = "admin-usuarios";
            return View(DbHelper.ListarUsuarios());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarRol(int idUsuario, string nuevoRol)
        {
            DbHelper.ActualizarRolUsuario(idUsuario, nuevoRol);
            return RedirectToAction("Usuarios");
        }
        public ActionResult Solicitudes()
        {
            ViewBag.Pagina = "admin-solicitudes";
            return View(DbHelper.ListarSolicitudes());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ActualizarSolicitud(int idSolicitud, string estado, string notasAdmin)
        {
            DbHelper.ActualizarEstadoSolicitud(idSolicitud, estado, notasAdmin);
            TempData["OkAdmin"] = "Solicitud actualizada correctamente.";
            return RedirectToAction("Solicitudes");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ConvertirEnProyecto(int idSolicitud, int idCliente)
        {
            var solicitudes = DbHelper.ListarSolicitudes();
            var s = solicitudes.Find(x => x.IdSolicitud == idSolicitud);
            if (s != null)
            {
                var p = new Constructora.Models.Proyecto
                {
                    NombreProyecto = s.NombreProyecto,
                    Descripcion = s.Descripcion,
                    IdCliente = idCliente,
                    FechaInicio = DateTime.Today,
                    Estado = "En proceso",
                    PresupuestoTotal = s.PresupuestoEstimado,
                    Tipo = s.Tipo,
                    Ciudad = s.Ciudad
                };
                DbHelper.CrearProyecto(p);
                DbHelper.ActualizarEstadoSolicitud(idSolicitud, "Aprobada", "Convertida en proyecto.");
                TempData["OkAdmin"] = "Proyecto creado exitosamente.";
            }
            return RedirectToAction("Solicitudes");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearMaterial(Material m)
        {
            if (ModelState.IsValid) DbHelper.CrearMaterial(m);
            return RedirectToAction("Materiales");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearCompra(int IdProveedor, int IdMaterial, int Cantidad, decimal Precio, DateTime Fecha)
        {
            using (var cn = new System.Data.SqlClient.SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConstructoraDB"].ConnectionString))
            {
                var cmd = new System.Data.SqlClient.SqlCommand(
                    "INSERT INTO compras_materiales(id_proveedor,fecha) VALUES(@p,@f); SELECT SCOPE_IDENTITY();", cn);
                cmd.Parameters.AddWithValue("@p", IdProveedor);
                cmd.Parameters.AddWithValue("@f", Fecha);
                cn.Open();
                int idCompra = Convert.ToInt32(cmd.ExecuteScalar());

                var cmd2 = new System.Data.SqlClient.SqlCommand(
                    "INSERT INTO detalle_compra(id_compra,id_material,cantidad,precio) VALUES(@c,@m,@cant,@pr)", cn);
                cmd2.Parameters.AddWithValue("@c", idCompra);
                cmd2.Parameters.AddWithValue("@m", IdMaterial);
                cmd2.Parameters.AddWithValue("@cant", Cantidad);
                cmd2.Parameters.AddWithValue("@pr", Precio);
                cmd2.ExecuteNonQuery();
            }
            return RedirectToAction("Compras");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearAvance(int IdProyecto, string Descripcion, int PorcentajeAvance, DateTime Fecha)
        {
            using (var cn = new System.Data.SqlClient.SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConstructoraDB"].ConnectionString))
            using (var cmd = new System.Data.SqlClient.SqlCommand(
                "INSERT INTO avances_obra(id_proyecto,descripcion,porcentaje_avance,fecha) VALUES(@p,@d,@pct,@f)", cn))
            {
                cmd.Parameters.AddWithValue("@p", IdProyecto);
                cmd.Parameters.AddWithValue("@d", (object)Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@pct", PorcentajeAvance);
                cmd.Parameters.AddWithValue("@f", Fecha);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Avances");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearFactura(int IdProyecto, decimal Monto, DateTime Fecha, string EstadoPago)
        {
            using (var cn = new System.Data.SqlClient.SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConstructoraDB"].ConnectionString))
            using (var cmd = new System.Data.SqlClient.SqlCommand(
                "INSERT INTO facturas(id_proyecto,monto,fecha,estado_pago) VALUES(@p,@m,@f,@e)", cn))
            {
                cmd.Parameters.AddWithValue("@p", IdProyecto);
                cmd.Parameters.AddWithValue("@m", Monto);
                cmd.Parameters.AddWithValue("@f", Fecha);
                cmd.Parameters.AddWithValue("@e", EstadoPago ?? "Pendiente");
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Facturas");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearUsuarioAdmin(string Nombre, string Correo, string Password, string Rol)
        {
            if (DbHelper.ObtenerUsuarioPorCorreo(Correo) == null)
                DbHelper.CrearUsuario(Nombre, Correo, AccountController.HashPassword(Password), Rol ?? "cliente");
            TempData["OkUsuario"] = "Usuario creado correctamente.";
            return RedirectToAction("Usuarios");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditarUsuario(int IdUsuario, string Nombre, string Correo, string Password)
        {
            using (var cn = new System.Data.SqlClient.SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConstructoraDB"].ConnectionString))
            {
                string sql = string.IsNullOrEmpty(Password)
                    ? "UPDATE usuarios SET nombre=@n, correo=@c WHERE id_usuario=@id"
                    : "UPDATE usuarios SET nombre=@n, correo=@c, password=@p WHERE id_usuario=@id";
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@n", Nombre);
                    cmd.Parameters.AddWithValue("@c", Correo);
                    cmd.Parameters.AddWithValue("@id", IdUsuario);
                    if (!string.IsNullOrEmpty(Password))
                        cmd.Parameters.AddWithValue("@p", AccountController.HashPassword(Password));
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["OkUsuario"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Usuarios");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EliminarUsuario(int idUsuario)
        {
            using (var cn = new System.Data.SqlClient.SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConstructoraDB"].ConnectionString))
            using (var cmd = new System.Data.SqlClient.SqlCommand(
                "DELETE FROM usuarios WHERE id_usuario=@id", cn))
            {
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            TempData["OkUsuario"] = "Usuario eliminado.";
            return RedirectToAction("Usuarios");
        }
    }

    [AutorizarRol("cliente")]
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Pagina = "cli-dash";
            return View(DbHelper.ListarProyectos());
        }

        public ActionResult MisProyectos()
        {
            ViewBag.Pagina = "cli-proy";
            return View(DbHelper.ListarProyectos());
        }

        public ActionResult Avances()
        {
            ViewBag.Pagina = "cli-avances";
            return View(DbHelper.ListarAvances());
        }

        public ActionResult Facturas()
        {
            ViewBag.Pagina = "cli-facturas";
            return View(DbHelper.ListarFacturas());
        }
        public ActionResult MisSolicitudes()
        {
            ViewBag.Pagina = "cli-solicitudes";
            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            return View(DbHelper.ListarSolicitudesPorUsuario(idUsuario));
        }

        public ActionResult NuevaSolicitud()
        {
            ViewBag.Pagina = "cli-solicitudes";
            return View(new Constructora.Models.Solicitud());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult NuevaSolicitud(Constructora.Models.Solicitud s)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pagina = "cli-solicitudes";
                return View(s);
            }
            s.IdUsuario = Convert.ToInt32(Session["IdUsuario"]);
            DbHelper.CrearSolicitud(s);
            TempData["OkSolicitud"] = "Tu solicitud fue enviada correctamente. El equipo la revisara pronto.";
            return RedirectToAction("MisSolicitudes");
        }
    }

    [AutorizarRol("empleado")]
    public class EmpleadoController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Pagina = "emp-dash";
            ViewBag.Proyectos = DbHelper.ListarProyectos();
            ViewBag.Avances = DbHelper.ListarAvances();
            return View();
        }

        public ActionResult Proyectos()
        {
            ViewBag.Pagina = "emp-proy";
            return View(DbHelper.ListarProyectos());
        }

        public ActionResult Avances()
        {
            ViewBag.Pagina = "emp-av";
            return View(DbHelper.ListarAvances());
        }

        public ActionResult Materiales()
        {
            ViewBag.Pagina = "emp-mat";
            return View(DbHelper.ListarMateriales());
        }
    }
}