using ApiBserve.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddControllers();

// Repositórios
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ProdutoRepository>();

// CORS — essencial para Blazor WASM + cookies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor",
        policy =>
        {
            policy.WithOrigins(
                "https://localhost:7153",
                "https://192.168.1.115:7153",
                "http://192.168.1.115:5263",
                "http://localhost:5263"
                ) // URL do seu Blazor
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // 🔑 permite enviar cookies
        });
});

// Autenticação com cookie
builder.Services
    .AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "BserveAuth";
        options.Cookie.HttpOnly = true;

        // ================================
        // 🔐 PRODUÇÃO (HTTPS - RECOMENDADO)
        // ================================
        // Só envia cookie via HTTPS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;


        // ================================
        // 🧪 DESENVOLVIMENTO (HTTP via IP)
        // Descomente apenas se for testar em rede local sem HTTPS
        // ================================
        
        //options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        //options.Cookie.SameSite = SameSiteMode.Lax;
        

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

// Autorização
builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline
app.UseCors("AllowBlazor"); // 🔹 CORS primeiro
app.UseHttpsRedirection();

app.UseAuthentication();    // 🔹 Depois autentica
app.UseAuthorization();     // 🔹 Depois autoriza

app.MapControllers();
app.Run();