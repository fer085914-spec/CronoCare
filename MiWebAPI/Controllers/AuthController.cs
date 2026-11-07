//using Microsoft.AspNetCore.Mvc;
//using MiWebAPI.Models;

//namespace MiWebAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly IConfiguration _configuration;

//        public AuthController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        [HttpPost("login")]
//        public IActionResult Login([FromBody] LoginRequest request)
//        {
//            string correoConfigurado = _configuration["DemoLogin:Correo"] ?? string.Empty;
//            string passwordConfigurado = _configuration["DemoLogin:Password"] ?? string.Empty;
//            string nombreConfigurado = _configuration["DemoLogin:Nombre"] ?? "Usuario";

//            bool credencialesCorrectas =
//                request.Correo.Equals(correoConfigurado, StringComparison.OrdinalIgnoreCase) &&
//                request.Password == passwordConfigurado;

//            if (!credencialesCorrectas)
//            {
//                return Unauthorized(new
//                {
//                    isSuccess = false,
//                    numeroError = 401,
//                    respuesta = "Correo o contraseña incorrectos"
//                });
//            }

//            return Ok(new
//            {
//                isSuccess = true,
//                numeroError = 0,
//                respuesta = "Inicio de sesión correcto",
//                usuario = new
//                {
//                    nombre = nombreConfigurado,
//                    correo = correoConfigurado
//                }
//            });
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Serialization;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("registrar")]
    public IActionResult Registrar([FromBody] UsuarioRegistroDto modelo)
    {
        string connectionString = _configuration.GetConnectionString("CadenaSQL");

        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Usuario_Registrar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", modelo.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", modelo.Correo);
                    cmd.Parameters.AddWithValue("@Contrasena", modelo.Contrasena);
                    cmd.Parameters.AddWithValue("@TipoUso", modelo.TipoUso);

                    conn.Open();
                    object resultado = cmd.ExecuteScalar();

                    return Ok(new { success = true, message = "Usuario registrado correctamente", idUsuario = resultado });
                }
            }
        }
        catch (SqlException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, errorNumber = ex.Number });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UsuarioLoginDto modelo)
    {
        string connectionString = _configuration.GetConnectionString("CadenaSQL");

        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Llamamos directamente al Stored Procedure que tienes para obtener el usuario por correo
                using (SqlCommand cmd = new SqlCommand("sp_Usuario_ObtenerPorCorreo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Correo", modelo.Correo);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string contrasenaBD = reader["CONTRASENA"].ToString();

                            // Validamos la contraseña (en texto plano según tu esquema actual)
                            if (contrasenaBD == modelo.Contrasena)
                            {
                                return Ok(new
                                {
                                    success = true,
                                    message = "Inicio de sesión exitoso",
                                    usuario = new
                                    {
                                        id = reader["ID_USUARIO"],
                                        nombre = reader["NOMBRE"].ToString(),
                                        correo = reader["CORREO"].ToString(),
                                        tipoUso = reader["TIPO_USO"].ToString()
                                    }
                                });
                            }
                        }
                    }
                }
            }

            return BadRequest(new { success = false, message = "Correo o contraseña incorrectos." });
        }
        catch (SqlException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Endpoint obtener los perfiles del usuario
    [HttpGet("perfiles/{idUsuario}")]
    public IActionResult ListarPerfiles(long idUsuario)
    {
        string connectionString = _configuration.GetConnectionString("ConexionCronoCare");
        var perfiles = new List<object>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("sp_Perfil_ListarPorUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        perfiles.Add(new
                        {
                            IdPerfil = reader["ID_PERFIL"],
                            TipoPerfil = reader["TIPO_PERFIL"],
                            NombrePersonalizado = reader["NOMBRE_PERSONALIZADO"]
                        });
                    }
                }
            }
        }
        return Ok(perfiles);
    }

    [HttpGet("medicamentos/activos/{idUsuario}/{idPerfil}")]
    public IActionResult ListarMedicamentosActivos(long idUsuario, long idPerfil)
    {
        string connectionString = _configuration.GetConnectionString("ConexionCronoCare");
        var medicamentos = new List<object>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("sp_Medicamento_ListarActivos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdPerfil", idPerfil);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        medicamentos.Add(new
                        {
                            IdMedicamento = reader["ID_MEDICAMENTO"],
                            NombreMedicamento = reader["NOMBRE_MEDICAMENTO"],
                            Foto = reader["FOTO"] != DBNull.Value ? reader["FOTO"] : null,
                            ViaAdministracion = reader["VIA_ADMINISTRACION"],
                            ProximaToma = reader["PROXIMA_TOMA"] != DBNull.Value ? reader["PROXIMA_TOMA"] : null
                        });
                    }
                }
            }
        }
        return Ok(medicamentos);
    }
}

public class UsuarioRegistroDto
{
    public string Nombre { get; set; }
    public string Correo { get; set; }
    [JsonPropertyName("password")]
    public string Contrasena { get; set; }
    public string TipoUso { get; set; }
}

public class UsuarioLoginDto
{
    public string Correo { get; set; }
    public string Contrasena { get; set; }
}