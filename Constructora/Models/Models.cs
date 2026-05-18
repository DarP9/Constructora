using System;
using System.ComponentModel.DataAnnotations;

namespace Constructora.Models
{
    public class Usuario
    {
        public int      IdUsuario     { get; set; }
        public string   Nombre        { get; set; }
        public string   Correo        { get; set; }
        public string   Password      { get; set; }
        public string   Rol           { get; set; } 
        public DateTime FechaRegistro { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresa tu correo")]
        public string Correo   { get; set; }
        [Required(ErrorMessage = "Ingresa tu contrasena")]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre   { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]
        public string Correo   { get; set; }
        [Required(ErrorMessage = "La contrasena es obligatoria")]
        public string Password { get; set; }
        public string Telefono { get; set; }
    }

    public class Cliente
    {
        public int    IdCliente  { get; set; }
        [Required] public string Nombre    { get; set; }
        public string Telefono   { get; set; }
        public string Correo     { get; set; }
        public string Direccion  { get; set; }
        public string Empresa    { get; set; }
    }

    public class Empleado
    {
        public int     IdEmpleado { get; set; }
        [Required] public string Nombre    { get; set; }
        public string  Puesto    { get; set; }
        public string  Telefono  { get; set; }
        public string  Correo    { get; set; }
        public decimal Salario   { get; set; }
    }

    public class Proveedor
    {
        public int    IdProveedor { get; set; }
        [Required] public string Nombre    { get; set; }
        public string Telefono   { get; set; }
        public string Correo     { get; set; }
        public string Direccion  { get; set; }
    }

    public class Proyecto
    {
        public int       IdProyecto       { get; set; }
        [Required] public string NombreProyecto   { get; set; }
        public string    Descripcion      { get; set; }
        public int       IdCliente        { get; set; }
        public string    NombreCliente    { get; set; }
        public DateTime  FechaInicio      { get; set; }
        public DateTime? FechaFin         { get; set; }
        public string    Estado           { get; set; }
        public decimal   PresupuestoTotal { get; set; }
        public string    Tipo             { get; set; }
        public string    Ciudad           { get; set; }
        public int       Year             { get; set; }
        public int       M2               { get; set; }
        public string    Imagen           { get; set; }
    }

    public class Material
    {
        public int     IdMaterial      { get; set; }
        [Required] public string Nombre          { get; set; }
        public string  Descripcion     { get; set; }
        public string  UnidadMedida    { get; set; }
        public decimal PrecioUnitario  { get; set; }
        public int     IdProveedor     { get; set; }
        public string  NombreProveedor { get; set; }
    }

    public class Presupuesto
    {
        public int      IdPresupuesto  { get; set; }
        public int      IdProyecto     { get; set; }
        public string   NombreProyecto { get; set; }
        public DateTime Fecha          { get; set; }
        public decimal  CostoTotal     { get; set; }
        public string   Estado         { get; set; }
    }

    public class AvanceObra
    {
        public int      IdAvance         { get; set; }
        public int      IdProyecto       { get; set; }
        public string   NombreProyecto   { get; set; }
        public string   NombreCliente    { get; set; }
        public string   Descripcion      { get; set; }
        public int      PorcentajeAvance { get; set; }
        public DateTime Fecha            { get; set; }
    }

    public class Factura
    {
        public int      IdFactura      { get; set; }
        public int      IdProyecto     { get; set; }
        public string   NombreProyecto { get; set; }
        public string   NombreCliente  { get; set; }
        public decimal  Monto          { get; set; }
        public DateTime Fecha          { get; set; }
        public string   EstadoPago     { get; set; }
    }

    public class ResumenFinanciero
    {
        public int     IdProyecto       { get; set; }
        public string  NombreProyecto   { get; set; }
        public decimal PresupuestoTotal { get; set; }
        public decimal TotalFacturado   { get; set; }
        public decimal SaldoPendiente   { get; set; }
        public string  Estado           { get; set; }
    }

    public class CompraProveedor
    {
        public int      IdCompra  { get; set; }
        public string   Proveedor { get; set; }
        public string   Material  { get; set; }
        public int      Cantidad  { get; set; }
        public decimal  Precio    { get; set; }
        public decimal  Subtotal  { get; set; }
        public DateTime Fecha     { get; set; }
    }
    public class Solicitud
    {
        public int IdSolicitud { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        [Required(ErrorMessage = "El nombre del proyecto es obligatorio")]
        public string NombreProyecto { get; set; }
        public string Tipo { get; set; }
        public string Ciudad { get; set; }
        public string Descripcion { get; set; }
        public decimal PresupuestoEstimado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; } 
        public string NotasAdmin { get; set; }
    }
}
