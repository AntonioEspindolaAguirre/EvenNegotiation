using EventNegotiation.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EventNegotiation.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddControllersWithViews();


#pragma warning disable CS8604 // Posible argumento de referencia nulo
// Registrar el contexto como un singleton si necesitas acceder a él en otros servicios
builder.Services.AddSingleton(new Contexto(builder.Configuration.GetConnectionString("DefaultConnection")));
// Configurar la conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configurar la autenticación de cookies
// Configurar Google Authentication

builder.Services.AddAuthentication(options =>
{
    // Establecer el esquema predeterminado para la autenticación
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Establecer el esquema de autenticación para iniciar sesión
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Establecer el esquema de desafío para Google
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    

})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Index"; // Ruta de inicio de sesión
    options.LogoutPath = "/Home/CerrarSesion"; // Página de cierre de sesión
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // Expiración de la cookie
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
  

});

var app = builder.Build();

// Configurar el middleware HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Middleware para evitar la caché después de cerrar sesión
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Splash}/{id?}");

app.Run();

