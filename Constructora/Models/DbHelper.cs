using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Constructora.Models
{
    public static class DbHelper
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["ConstructoraDB"].ConnectionString;


        public static Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "SELECT id_usuario,nombre,correo,password,rol,fecha_registro FROM usuarios WHERE correo=@c", cn))
            {
                cmd.Parameters.AddWithValue("@c", correo);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? MapUsuario(r) : null;
            }
        }

        public static Usuario ObtenerUsuarioPorId(int id)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "SELECT id_usuario,nombre,correo,password,rol,fecha_registro FROM usuarios WHERE id_usuario=@id", cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? MapUsuario(r) : null;
            }
        }

        public static List<Usuario> ListarUsuarios()
        {
            var lista = new List<Usuario>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "SELECT id_usuario,nombre,correo,password,rol,fecha_registro FROM usuarios ORDER BY nombre", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(MapUsuario(r));
            }
            return lista;
        }

        public static int CrearUsuario(string nombre, string correo, string passwordHash, string rol = "cliente")
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "INSERT INTO usuarios(nombre,correo,password,rol) VALUES(@n,@c,@p,@r); SELECT SCOPE_IDENTITY();", cn))
            {
                cmd.Parameters.AddWithValue("@n", nombre);
                cmd.Parameters.AddWithValue("@c", correo);
                cmd.Parameters.AddWithValue("@p", passwordHash);
                cmd.Parameters.AddWithValue("@r", rol);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void ActualizarRolUsuario(int idUsuario, string nuevoRol)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("UPDATE usuarios SET rol=@r WHERE id_usuario=@id", cn))
            {
                cmd.Parameters.AddWithValue("@r", nuevoRol);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public static List<Cliente> ListarClientes()
        {
            var lista = new List<Cliente>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("EXEC sp_listar_clientes", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(MapCliente(r));
            }
            return lista;
        }

        public static int CrearCliente(Cliente c)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "EXEC sp_crear_cliente @Nombre,@Telefono,@Correo,@Direccion,@Empresa", cn))
            {
                cmd.Parameters.AddWithValue("@Nombre",    c.Nombre);
                cmd.Parameters.AddWithValue("@Telefono",  (object)c.Telefono  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Correo",    (object)c.Correo    ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", (object)c.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Empresa",   (object)c.Empresa   ?? DBNull.Value);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public static List<Empleado> ListarEmpleados()
        {
            var lista = new List<Empleado>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("EXEC sp_listar_empleados", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(MapEmpleado(r));
            }
            return lista;
        }

        public static int CrearEmpleado(Empleado e)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "EXEC sp_crear_empleado @Nombre,@Puesto,@Telefono,@Correo,@Salario", cn))
            {
                cmd.Parameters.AddWithValue("@Nombre",   e.Nombre);
                cmd.Parameters.AddWithValue("@Puesto",   (object)e.Puesto   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", (object)e.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Correo",   (object)e.Correo   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Salario",  e.Salario == 0 ? (object)DBNull.Value : e.Salario);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public static List<Proveedor> ListarProveedores()
        {
            var lista = new List<Proveedor>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("EXEC sp_listar_proveedores", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(MapProveedor(r));
            }
            return lista;
        }

        public static int CrearProveedor(Proveedor p)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "EXEC sp_crear_proveedor @Nombre,@Telefono,@Correo,@Direccion", cn))
            {
                cmd.Parameters.AddWithValue("@Nombre",    p.Nombre);
                cmd.Parameters.AddWithValue("@Telefono",  (object)p.Telefono  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Correo",    (object)p.Correo    ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", (object)p.Direccion ?? DBNull.Value);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public static List<Proyecto> ListarProyectos(int? idCliente = null)
        {
            var lista = new List<Proyecto>();
            string sql = @"SELECT p.id_proyecto, p.nombre_proyecto,
                            ISNULL(c.nombre,'') AS cliente,
                            p.fecha_inicio, p.fecha_fin, p.estado,
                            ISNULL(p.presupuesto_total,0) AS presupuesto_total,
                            ISNULL(p.tipo,'')    AS tipo,
                            ISNULL(p.ciudad,'')  AS ciudad,
                            ISNULL(p.m2,0)       AS m2,
                            ISNULL(p.imagen,'')  AS imagen,
                            ISNULL(p.descripcion,'') AS descripcion
                           FROM proyectos p
                           LEFT JOIN clientes c ON p.id_cliente = c.id_cliente"
                + (idCliente.HasValue ? " WHERE p.id_cliente=@ic" : "")
                + " ORDER BY p.id_proyecto DESC";

            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (idCliente.HasValue)
                    cmd.Parameters.AddWithValue("@ic", idCliente.Value);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(MapProyecto(r));
            }
            return lista;
        }

        public static int CrearProyecto(Proyecto p)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
        INSERT INTO proyectos(nombre_proyecto,descripcion,id_cliente,fecha_inicio,
            fecha_fin,estado,presupuesto_total,tipo,ciudad,m2,imagen)
        VALUES(@np,@desc,@ic,@fi,@ff,@est,@pres,@tipo,@ciudad,@m2,@img);
        SELECT SCOPE_IDENTITY();", cn))
            {
                cmd.Parameters.AddWithValue("@np", p.NombreProyecto);
                cmd.Parameters.AddWithValue("@desc", (object)p.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ic", p.IdCliente);
                cmd.Parameters.AddWithValue("@fi", p.FechaInicio == default ? (object)DBNull.Value : p.FechaInicio);
                cmd.Parameters.AddWithValue("@ff", p.FechaFin.HasValue ? (object)p.FechaFin.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@est", p.Estado ?? "En proceso");
                cmd.Parameters.AddWithValue("@pres", p.PresupuestoTotal == 0 ? (object)DBNull.Value : p.PresupuestoTotal);
                cmd.Parameters.AddWithValue("@tipo", (object)p.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ciudad", (object)p.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@m2", p.M2 == 0 ? (object)DBNull.Value : p.M2);
                cmd.Parameters.AddWithValue("@img", (object)p.Imagen ?? DBNull.Value);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public static List<AvanceObra> ListarAvances(int? idProyecto = null)
        {
            var lista = new List<AvanceObra>();
            string sql = @"SELECT a.id_avance, p.nombre_proyecto, c.nombre AS cliente,
                            a.descripcion, a.porcentaje_avance, a.fecha, a.id_proyecto
                           FROM avances_obra a
                           INNER JOIN proyectos p ON a.id_proyecto = p.id_proyecto
                           INNER JOIN clientes  c ON p.id_cliente  = c.id_cliente"
                + (idProyecto.HasValue ? " WHERE a.id_proyecto=@ip" : "")
                + " ORDER BY a.fecha DESC";
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (idProyecto.HasValue)
                    cmd.Parameters.AddWithValue("@ip", idProyecto.Value);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new AvanceObra
                        {
                            IdAvance         = r.GetInt32(0),
                            NombreProyecto   = r["nombre_proyecto"].ToString(),
                            NombreCliente    = r["cliente"].ToString(),
                            Descripcion      = r["descripcion"].ToString(),
                            PorcentajeAvance = r.GetInt32(4),
                            Fecha            = r.GetDateTime(5),
                            IdProyecto       = r.GetInt32(6)
                        });
            }
            return lista;
        }


        public static List<Factura> ListarFacturas(int? idProyecto = null)
        {
            var lista = new List<Factura>();
            string sql = @"SELECT f.id_factura, p.nombre_proyecto, c.nombre AS cliente,
                            f.monto, f.fecha, f.estado_pago, f.id_proyecto
                           FROM facturas f
                           INNER JOIN proyectos p ON f.id_proyecto = p.id_proyecto
                           INNER JOIN clientes  c ON p.id_cliente  = c.id_cliente"
                + (idProyecto.HasValue ? " WHERE f.id_proyecto=@ip" : "")
                + " ORDER BY f.fecha DESC";
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (idProyecto.HasValue)
                    cmd.Parameters.AddWithValue("@ip", idProyecto.Value);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new Factura
                        {
                            IdFactura      = r.GetInt32(0),
                            NombreProyecto = r["nombre_proyecto"].ToString(),
                            NombreCliente  = r["cliente"].ToString(),
                            Monto          = r.GetDecimal(3),
                            Fecha          = r.GetDateTime(4),
                            EstadoPago     = r["estado_pago"].ToString(),
                            IdProyecto     = r.GetInt32(6)
                        });
            }
            return lista;
        }


        public static List<ResumenFinanciero> ListarResumenFinanciero()
        {
            var lista = new List<ResumenFinanciero>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT * FROM vista_resumen_financiero", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new ResumenFinanciero
                        {
                            IdProyecto = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                            NombreProyecto = r["nombre_proyecto"].ToString(),
                            PresupuestoTotal = r.IsDBNull(2) ? 0 : r.GetDecimal(2),
                            TotalFacturado = r.IsDBNull(3) ? 0 : r.GetDecimal(3),
                            SaldoPendiente = r.IsDBNull(4) ? 0 : r.GetDecimal(4),
                            Estado = r.IsDBNull(5) ? "" : r["estado"].ToString()
                        });
            }
            return lista;
        }


        public static List<CompraProveedor> ListarCompras()
        {
            var lista = new List<CompraProveedor>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "SELECT * FROM vista_compras_proveedor ORDER BY fecha DESC", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new CompraProveedor
                        {
                            IdCompra = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                            Proveedor = r.IsDBNull(1) ? "" : r.GetString(1),
                            Material = r.IsDBNull(2) ? "" : r.GetString(2),
                            Cantidad = r.IsDBNull(3) ? 0 : r.GetInt32(3),
                            Precio = r.IsDBNull(4) ? 0 : r.GetDecimal(4),
                            Subtotal = r.IsDBNull(5) ? 0 : r.GetDecimal(5),
                            Fecha = r.IsDBNull(6) ? DateTime.Now : r.GetDateTime(6)
                        });
            }
            return lista;
        }


        public static List<Material> ListarMateriales()
        {
            var lista = new List<Material>();
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                @"SELECT m.id_material, m.nombre, m.descripcion, m.unidad_medida,
                         m.precio_unitario, m.id_proveedor, ISNULL(p.nombre,'') AS proveedor
                  FROM materiales m
                  LEFT JOIN proveedores p ON m.id_proveedor = p.id_proveedor
                  ORDER BY m.nombre", cn))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new Material
                        {
                            IdMaterial      = r.GetInt32(0),
                            Nombre          = r["nombre"].ToString(),
                            Descripcion     = r["descripcion"].ToString(),
                            UnidadMedida    = r["unidad_medida"].ToString(),
                            PrecioUnitario  = r.GetDecimal(4),
                            IdProveedor     = r.IsDBNull(5) ? 0 : r.GetInt32(5),
                            NombreProveedor = r["proveedor"].ToString()
                        });
            }
            return lista;
        }


        private static Usuario MapUsuario(SqlDataReader r) => new Usuario
        {
            IdUsuario     = r.GetInt32(0),
            Nombre        = r["nombre"].ToString(),
            Correo        = r["correo"].ToString(),
            Password      = r["password"].ToString(),
            Rol           = r["rol"] == DBNull.Value ? "cliente" : r["rol"].ToString(),
            FechaRegistro = r.IsDBNull(5) ? DateTime.Now : r.GetDateTime(5)
        };

        private static Cliente MapCliente(SqlDataReader r) => new Cliente
        {
            IdCliente = r.GetInt32(0),
            Nombre    = r["nombre"].ToString(),
            Telefono  = r["telefono"].ToString(),
            Correo    = r["correo"].ToString(),
            Direccion = r["direccion"].ToString(),
            Empresa   = r["empresa"].ToString()
        };

        private static Empleado MapEmpleado(SqlDataReader r) => new Empleado
        {
            IdEmpleado = r.GetInt32(0),
            Nombre     = r["nombre"].ToString(),
            Puesto     = r["puesto"].ToString(),
            Telefono   = r["telefono"].ToString(),
            Correo     = r["correo"].ToString(),
            Salario    = r.IsDBNull(5) ? 0 : r.GetDecimal(5)
        };

        private static Proveedor MapProveedor(SqlDataReader r) => new Proveedor
        {
            IdProveedor = r.GetInt32(0),
            Nombre      = r["nombre"].ToString(),
            Telefono    = r["telefono"].ToString(),
            Correo      = r["correo"].ToString(),
            Direccion   = r["direccion"].ToString()
        };

        private static Proyecto MapProyecto(SqlDataReader r) => new Proyecto
        {
            IdProyecto = r.GetInt32(0),
            NombreProyecto = r["nombre_proyecto"].ToString(),
            NombreCliente = r["cliente"].ToString(),
            FechaInicio = r.IsDBNull(3) ? DateTime.Now : r.GetDateTime(3),
            FechaFin = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
            Estado = r["estado"].ToString(),
            PresupuestoTotal = r.IsDBNull(6) ? 0 : r.GetDecimal(6),
            Tipo = r["tipo"].ToString(),
            Ciudad = r["ciudad"].ToString(),
            M2 = r.IsDBNull(9) ? 0 : r.GetInt32(9),
            Imagen = r["imagen"].ToString(),
            Descripcion = r["descripcion"].ToString()
        };

        public static int CrearSolicitud(Solicitud s)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                INSERT INTO solicitudes
                    (id_usuario, nombre_proyecto, tipo, ciudad, descripcion, presupuesto_estimado)
                VALUES
                    (@idu, @np, @tipo, @ciudad, @desc, @pres);
                SELECT SCOPE_IDENTITY();", cn))
            {
                cmd.Parameters.AddWithValue("@idu", s.IdUsuario);
                cmd.Parameters.AddWithValue("@np", s.NombreProyecto);
                cmd.Parameters.AddWithValue("@tipo", (object)s.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ciudad", (object)s.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@desc", (object)s.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@pres", s.PresupuestoEstimado == 0 ? (object)DBNull.Value : s.PresupuestoEstimado);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static List<Solicitud> ListarSolicitudes(string estado = null)
        {
            var lista = new List<Solicitud>();
            string sql = @"SELECT s.id_solicitud, s.id_usuario, u.nombre, u.correo,
                            s.nombre_proyecto, s.tipo, s.ciudad, s.descripcion,
                            ISNULL(s.presupuesto_estimado,0), s.fecha_solicitud,
                            s.estado, ISNULL(s.notas_admin,'')
                           FROM solicitudes s
                           INNER JOIN usuarios u ON s.id_usuario = u.id_usuario"
                + (estado != null ? " WHERE s.estado=@est" : "")
                + " ORDER BY s.fecha_solicitud DESC";

            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (estado != null)
                    cmd.Parameters.AddWithValue("@est", estado);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new Solicitud
                        {
                            IdSolicitud = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                            IdUsuario = r.IsDBNull(1) ? 0 : r.GetInt32(1),
                            NombreUsuario = r["nombre"].ToString(),
                            CorreoUsuario = r["correo"].ToString(),
                            NombreProyecto = r["nombre_proyecto"].ToString(),
                            Tipo = r.IsDBNull(5) ? "" : r.GetString(5),
                            Ciudad = r.IsDBNull(6) ? "" : r.GetString(6),
                            Descripcion = r.IsDBNull(7) ? "" : r.GetString(7),
                            PresupuestoEstimado = r.IsDBNull(8) ? 0 : r.GetDecimal(8),
                            FechaSolicitud = r.IsDBNull(9) ? DateTime.Now : r.GetDateTime(9),
                            Estado = r.IsDBNull(10) ? "" : r.GetString(10),
                            NotasAdmin = r.IsDBNull(11) ? "" : r.GetString(11)
                        });
            }
            return lista;
        }

        public static List<Solicitud> ListarSolicitudesPorUsuario(int idUsuario)
        {
            var lista = new List<Solicitud>();
            string sql = @"SELECT s.id_solicitud, s.id_usuario, u.nombre, u.correo,
                s.nombre_proyecto, s.tipo, s.ciudad, s.descripcion,
                ISNULL(s.presupuesto_estimado,0) AS presupuesto_estimado,
                s.fecha_solicitud,
                s.estado,
                ISNULL(s.notas_admin,'') AS notas_admin
               FROM solicitudes s
               INNER JOIN usuarios u ON s.id_usuario = u.id_usuario
               WHERE s.id_usuario = @idu
               ORDER BY s.fecha_solicitud DESC";

            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@idu", idUsuario);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(new Solicitud
                        {
                            IdSolicitud = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                            IdUsuario = r.IsDBNull(1) ? 0 : r.GetInt32(1),
                            NombreUsuario = r["nombre"].ToString(),
                            CorreoUsuario = r["correo"].ToString(),
                            NombreProyecto = r["nombre_proyecto"].ToString(),
                            Tipo = r.IsDBNull(5) ? "" : r["tipo"].ToString(),
                            Ciudad = r.IsDBNull(6) ? "" : r["ciudad"].ToString(),
                            Descripcion = r.IsDBNull(7) ? "" : r["descripcion"].ToString(),
                            PresupuestoEstimado = r.IsDBNull(8) ? 0 : r.GetDecimal(8),
                            FechaSolicitud = r.IsDBNull(9) ? DateTime.Now : r.GetDateTime(9),
                            Estado = r.IsDBNull(10) ? "" : r.GetString(10),
                            NotasAdmin = r.IsDBNull(11) ? "" : r.GetString(11)
                        });
            }
            return lista;
        }

        public static void ActualizarEstadoSolicitud(int idSolicitud, string estado, string notasAdmin)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                UPDATE solicitudes
                SET estado=@est, notas_admin=@notas
                WHERE id_solicitud=@id", cn))
            {
                cmd.Parameters.AddWithValue("@est", estado);
                cmd.Parameters.AddWithValue("@notas", (object)notasAdmin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", idSolicitud);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static int CrearMaterial(Material m)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                "INSERT INTO materiales(nombre,descripcion,unidad_medida,precio_unitario,id_proveedor) VALUES(@n,@d,@u,@p,@ip); SELECT SCOPE_IDENTITY();", cn))
            {
                cmd.Parameters.AddWithValue("@n", m.Nombre);
                cmd.Parameters.AddWithValue("@d", (object)m.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@u", (object)m.UnidadMedida ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p", m.PrecioUnitario);
                cmd.Parameters.AddWithValue("@ip", m.IdProveedor == 0 ? (object)DBNull.Value : m.IdProveedor);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
