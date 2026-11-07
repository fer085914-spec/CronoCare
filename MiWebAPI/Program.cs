using MiWebAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EmpleadoData>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Publica el frontend creado con HTML, CSS y JavaScript desde wwwroot.

// === NUEVA CONFIGURACIÓN PARA EL SPLASH SCREEN ===
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("splash.html");
app.UseDefaultFiles(options);
// ==================================================

app.UseStaticFiles();
app.UseCors("NuevaPolitica");
app.UseAuthorization();
app.MapControllers();

app.Run();