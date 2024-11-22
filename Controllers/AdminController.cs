using EventNegotiation.Data;
using EventNegotiation.Data.Servicios;
using EventNegotiation.Models;
using EventNegotiation.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;

namespace EventNegotiation.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly Contexto _contexto;
        private readonly GeneralServicio _generalServicio;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger, Contexto contexto)
        {
            _logger = logger;
            _contexto = contexto;
            _generalServicio = new GeneralServicio(contexto);
        }

        // Método para iniciar sesión con Google
        [HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", "Admin") // Redirige a GoogleResponse
            }, GoogleDefaults.AuthenticationScheme);
        }

        // Método para manejar la respuesta de Google después del inicio de sesión
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal != null)
            {
                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

                // Consultar la base de datos para obtener el usuarioId
                using (NpgsqlConnection con = new(_contexto.Conexion))
                {
                    using (NpgsqlCommand cmd = new("SELECT usuario_id FROM public.usuario WHERE email = @p_correo", con))
                    {
                        cmd.Parameters.AddWithValue("p_correo", email);
                        con.Open();

                        var usuarioId = cmd.ExecuteScalar();

                        if (usuarioId != null)
                        {
                            // Agrega el usuarioId al claim
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                                new Claim(ClaimTypes.Email, email),
                                new Claim(ClaimTypes.Name, name)
                            };

                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            // Inicia sesión con los nuevos claims
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                            return RedirectToAction("Index", "Admin"); // Redirige al dashboard
                        }
                        else
                        {
                            TempData["Error"] = "El correo electrónico no está registrado.";
                            return RedirectToAction("Registrar", "Home");
                        }
                    }
                }
            }

            TempData["Error"] = "No se pudo obtener información del usuario.";
            return RedirectToAction("NoAutorizado", "Home");
        }

        // Vista principal del administrador
        public IActionResult Index()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var usuarioId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            ViewBag.UsuarioId = usuarioId;
            ViewBag.Email = email;
            ViewBag.Name = name;

            return View();
        }

        public IActionResult Usuarios()
        {
            return View();
        }
       

        // Vista de eventos
        public IActionResult Eventos()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;


            var usuarioId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            return View();
       
        }




    }
}
