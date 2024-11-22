using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using EventNegotiation.Data;
using EventNegotiation.Data.Servicios;
using EventNegotiation.Models;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using static EventNegotiation.Data.Servicios.GeneralServicio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using EventNegotiation.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.Google;



namespace EventNegotiation.Controllers
{
    public class HomeController : Controller
    {
        private readonly Contexto _contexto;
        private readonly GeneralServicio _generalServicio;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, Contexto contexto)
        {
            _logger = logger;
            _contexto = contexto;
            _generalServicio = new GeneralServicio(contexto);



        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registrar()
        {
            return View();
        }
        public IActionResult Splash()
        {
            return View();  // Carga la vista de splash
        }




        [HttpPost]
        public IActionResult CrearUsuario(Usuarios model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el correo ya existe
                    if (_generalServicio.EmailExists(model.Email))
                    {
                        ModelState.AddModelError("Email", "El correo electrónico ya existe.");
                        return View("Registrar", model); // Retorna la vista con el mensaje de error
                    }

                    // Llamamos al servicio que ejecuta la función agregar_usuario
                    int usuarioId = _generalServicio.AgregarUsuario(model.Nombre, model.Email, model.Password, model.Telefono);

                    // Asegúrate de guardar el usuarioId en TempData antes de redirigir
                    TempData["UsuarioId"] = usuarioId;
                    TempData["SuccessMessage"] = "Usuario registrado exitosamente.";

                    // Redirigir a la acción 'AltaEmpresa'
                    return RedirectToAction("AltaEmpresa", "Home"); // Asegúrate de que el nombre de la acción y controlador sean correctos
                }
                catch (Exception ex)
                {
                    // Manejo de excepciones
                    ModelState.AddModelError("", "Ocurrió un error al crear el usuario: " + ex.Message);
                    return View("Registrar", model); // Retorna la vista con el mensaje de error
                }
            }

            // Si no es válido, retorna la vista con los errores de validación
            return View("AgregarUsuario", model);
        }
        public IActionResult AltaEmpresa()
        {
            // Obtener el usuarioId desde TempData
            int usuarioId = (int?)TempData["UsuarioId"] ?? 0;

            // Verifica si se ha asignado correctamente el usuarioId
            if (usuarioId > 0)
            {
                // Aquí puedes hacer lo que necesites con el usuarioId
                ViewData["UsuarioId"] = usuarioId;
            }
            else
            {
                // Si el usuarioId no está disponible, muestra un mensaje de error o redirige
                TempData["ErrorMessage"] = "No se encontró un usuario válido.";
                return RedirectToAction("Index", "Home");
            }

            // Asegúrate de pasar un nuevo modelo Empresa a la vista
            return View(new Empresa());
        }



        [HttpPost]
        public IActionResult RegistrarEmpresa(Empresa model, int usuarioId)
        {
            if (usuarioId > 0)
            {
                // Lógica para registrar la empresa
                int id_empresa = _generalServicio.AgregarEmpresa(model.Nombre, model.Direccion, model.Telefono);
                TempData["SuccessMessage"] = "Empresa registrada exitosamente.";

                // Asociar la empresa al usuario
                _generalServicio.AsociarEmpresaAUsuario(usuarioId, id_empresa);

                // Redirigir a otra vista o acción
                return RedirectToAction("Index"); // Cambia por la acción a la que quieras redirigir
            }
            else
            {
                TempData["ErrorMessage"] = "No se encontró un usuario válido.";
                return RedirectToAction("Index", "Home");
            }
        }




        public async Task<IActionResult> Login(UsuarioLogin model)
        {
            try
            {
                using (NpgsqlConnection con = new(_contexto.Conexion))
                {
                    using (NpgsqlCommand cmd = new("SELECT * FROM ValidarUsuario(@p_correo)", con))
                    {
                        cmd.Parameters.AddWithValue("p_correo", model.Correo);
                        con.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                bool passwordMatch = BCrypt.Net.BCrypt.Verify(model.LaPoderosa, dr["password"].ToString());
                                if (passwordMatch)
                                {
                                    int usuarioId = (int)dr["usuario_id"];
                                    string nombreCompleto = (string)dr["nombre"];
                                    string email = (string)dr["email"];

                                    // Crear claims para la autenticación
                                    var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()), // ID de usuario
                                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                                new Claim(ClaimTypes.Name, nombreCompleto),
                                new Claim(ClaimTypes.Email, email), // Agregar el claim de correo electrónico
                                new Claim(ClaimTypes.Role, "Rol_1") // Asignar un rol fijo
                            };

                                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                    var authProperties = new AuthenticationProperties
                                    {
                                        AllowRefresh = true,
                                        IsPersistent = true,
                                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                                    };

                                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

                                    return RedirectToAction("Index", "Admin");
                                }
                                else
                                {
                                    TempData["Error"] = "Contraseña incorrecta.";
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                            else
                            {
                                TempData["Error"] = "Usuario no registrado.";
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error en el inicio de sesión: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }



        public async Task<IActionResult> CerrarSesion()
        {
            try
            {
                // Cierra sesión en la aplicación
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Elimina todas las cookies manualmente
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                // Redirige al logout de Google si el usuario inició sesión con Google
                if (User.Identity.AuthenticationType == GoogleDefaults.AuthenticationScheme)
                {
                    var googleLogoutUrl = "https://accounts.google.com/logout";
                    return Redirect(googleLogoutUrl);
                }

                // Redirige a la página de inicio de sesión para usuarios normales
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cerrar sesión: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }






        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
