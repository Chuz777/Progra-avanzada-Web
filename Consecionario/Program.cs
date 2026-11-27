using System.Security.Claims;
using ConcesionarioN.Data.Context;
using ConcesionarioN.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ConcesionarioNDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionDB")));

// Servicios de la aplicación
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IMecanicaService, MecanicaService>();

// HU-01: autenticación por cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;

        // HU-06: un usuario desactivado/eliminado (o con otro rol) pierde la sesión de inmediato.
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var idClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var rolClaim = context.Principal?.FindFirstValue(ClaimTypes.Role);

                var valido = false;
                if (int.TryParse(idClaim, out var id))
                {
                    var db = context.HttpContext.RequestServices.GetRequiredService<ConcesionarioNDbContext>();
                    var datos = await db.Usuarios.AsNoTracking()
                        .Where(u => u.Id == id)
                        .Select(u => new { u.Activo, u.Rol })
                        .FirstOrDefaultAsync();
                    valido = datos != null && datos.Activo && datos.Rol == rolClaim;
                }

                if (!valido)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

// HU-01/CA-03: toda página exige sesión salvo las marcadas con [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Aplica migraciones pendientes y crea las cuentas de prueba la primera vez
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConcesionarioNDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<IUsuarioService>().SembrarDatosInicialesAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Páginas de estado: 404 muestra "No encontrado"
app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Cultura fija: los números de los formularios usan punto decimal (12500.50)
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US")
    .AddSupportedUICultures("en-US"));

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
