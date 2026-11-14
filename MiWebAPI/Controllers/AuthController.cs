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

    private string CadenaSql =>
        _configuration.GetConnectionString("CadenaSQL")
        ?? throw new InvalidOperationException("Falta la cadena de conexión 'CadenaSQL' en appsettings.json.");

    private IActionResult RespuestaError(SqlException ex)
    {
        if (ex.Number >= 50001 && ex.Number <= 50009)
        {
            return BadRequest(new { success = false, message = ex.Message, errorNumber = ex.Number });
        }

        return StatusCode(500, new { success = false, message = "Ocurrió un error en el servidor. Inténtalo de nuevo." });
    }

    // ---------------------------------------------------------------
    // CUENTA
    // ---------------------------------------------------------------

    [HttpPost("registrar")]
    public IActionResult Registrar([FromBody] UsuarioRegistroDto modelo)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
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
            return RespuestaError(ex);
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UsuarioLoginDto modelo)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
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
            return RespuestaError(ex);
        }
    }

    // ---------------------------------------------------------------
    // CATÁLOGOS
    // ---------------------------------------------------------------

    [HttpGet("catalogos/tipos-perfil")]
    public IActionResult ListarTiposPerfil()
    {
        var tipos = new List<object>();

        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("sp_TipoPerfil_Listar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tipos.Add(new
                            {
                                idTipoPerfil = reader["ID_TIPO_PERFIL"],
                                nombre = reader["NOMBRE"].ToString()
                            });
                        }
                    }
                }
            }

            return Ok(tipos);
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    // Catálogo de vías de administración desde la BD
    [HttpGet("catalogos/vias-administracion")]
    public IActionResult ListarViasAdministracion()
    {
        var vias = new List<object>();

        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ID_VIA, NOMBRE FROM VIA_ADMINISTRACION", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vias.Add(new
                            {
                                idVia = reader["ID_VIA"],
                                nombre = reader["NOMBRE"].ToString()
                            });
                        }
                    }
                }
            }

            return Ok(vias);
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    // ---------------------------------------------------------------
    // PERFILES
    // ---------------------------------------------------------------

    [HttpGet("perfiles/{idUsuario}")]
    public IActionResult ListarPerfiles(long idUsuario)
    {
        var perfiles = new List<object>();

        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
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
                                idPerfil = reader["ID_PERFIL"],
                                nombre = reader["NOMBRE"].ToString(),
                                fechaNacimiento = Convert.ToDateTime(reader["FECHA_NACIMIENTO"]).ToString("yyyy-MM-dd"),
                                edad = reader["EDAD"],
                                idTipoPerfil = reader["ID_TIPO_PERFIL"],
                                tipoPerfil = reader["TIPO_PERFIL"].ToString()
                            });
                        }
                    }
                }
            }

            return Ok(perfiles);
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    [HttpPost("perfil/crear")]
    public IActionResult CrearPerfil([FromBody] PerfilCreacionDto modelo)
    {
        if (modelo == null || modelo.FechaNacimiento == default(DateTime))
        {
            return BadRequest(new { success = false, message = "Completa el nombre, el tipo y la fecha de nacimiento." });
        }

        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Perfil_Crear", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", modelo.IdUsuario);
                    cmd.Parameters.AddWithValue("@IdTipoPerfil", modelo.IdTipoPerfil);
                    cmd.Parameters.AddWithValue("@Nombre", modelo.Nombre ?? string.Empty);
                    cmd.Parameters.Add("@FechaNacimiento", SqlDbType.Date).Value = modelo.FechaNacimiento.Date;

                    conn.Open();
                    object idPerfil = cmd.ExecuteScalar();

                    return Ok(new { success = true, message = "Perfil creado correctamente", idPerfil = idPerfil });
                }
            }
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    // ---------------------------------------------------------------
    // MEDICAMENTOS
    // ---------------------------------------------------------------

    [HttpGet("medicamentos/activos/{idUsuario}/{idPerfil}")]
    public IActionResult ListarMedicamentosActivos(long idUsuario, long idPerfil)
    {
        var medicamentos = new List<object>();

        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
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
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    [HttpPost("medicamento/crear")]
    public IActionResult RegistrarMedicamento([FromBody] MedicamentoCreacionDto modelo)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Medicamento_Registrar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", modelo.IdUsuario);
                    cmd.Parameters.AddWithValue("@IdPerfil", modelo.IdPerfil);
                    cmd.Parameters.AddWithValue("@IdVia", modelo.IdVia);
                    cmd.Parameters.AddWithValue("@NombreMedicamento", modelo.NombreMedicamento);

                    // Manejo seguro de la foto (si es null o vacía, manda NULL a SQL)
                    cmd.Parameters.AddWithValue("@Foto", string.IsNullOrEmpty(modelo.Foto) ? (object)DBNull.Value : modelo.Foto);

                    cmd.Parameters.AddWithValue("@IntervaloHoras", modelo.IntervaloHoras);
                    cmd.Parameters.AddWithValue("@DuracionDias", modelo.DuracionDias);
                    cmd.Parameters.AddWithValue("@HoraPrimeraToma", modelo.HoraInicio);

                    // Manejo seguro de la fecha de inicio
                    if (modelo.FechaInicio.HasValue)
                    {
                        cmd.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = modelo.FechaInicio.Value.Date;
                    }
                    else
                    {
                        cmd.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = DBNull.Value;
                    }

                    conn.Open();
                    object resultado = cmd.ExecuteScalar();

                    return Ok(new { success = true, message = "Medicamento registrado correctamente", idMedicamento = resultado });
                }
            }
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    [HttpGet("medicamentos/detalle/{idUsuario}/{idMedicamento}")]
    public IActionResult ObtenerDetalleMedicamento(long idUsuario, long idMedicamento)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Medicamento_ObtenerDetalle", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@IdMedicamento", idMedicamento);
                    cmd.Parameters.AddWithValue("@Fecha", DBNull.Value);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound(new { success = false, message = "Medicamento no encontrado." });
                        }

                        var medicamentoDetalle = new
                        {
                            idMedicamento = reader["ID_MEDICAMENTO"],
                            nombreMedicamento = reader["NOMBRE_MEDICAMENTO"].ToString(),
                            idVia = reader["ID_VIA"], // <--- ¡AQUÍ ESTABA EL FALTANTE PARA EL SELECT!
                            viaAdministracion = reader["VIA_ADMINISTRACION"].ToString(),
                            intervaloHoras = reader["INTERVALO_HORAS"], // <--- También agregamos intervalo y duración por seguridad
                            duracionDias = reader["DURACION_DIAS"],
                            horaPrimeraToma = reader["HORA_PRIMERA_TOMA"].ToString(), // <--- ¡AQUÍ ESTABA EL FALTANTE PARA LA HORA!
                            foto = reader["FOTO"] != DBNull.Value ? reader["FOTO"].ToString() : null,
                            tomas = new List<object>()
                        };

                        // 2. Leer las tomas programadas (Segundo Resultado del SP)
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                ((List<object>)medicamentoDetalle.tomas).Add(new
                                {
                                    hora = Convert.ToDateTime(reader["FECHA_HORA_PROGRAMADA"]).ToString("HH:mm"),
                                    estado = reader["ESTADO"].ToString()
                                });
                            }
                        }

                        return Ok(medicamentoDetalle);
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    [HttpGet("agenda/hoy/{idUsuario}")]
    public IActionResult ObtenerAgendaHoy(long idUsuario)
    {
        var agenda = new List<object>();
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Toma_ListarHoy", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@Fecha", DBNull.Value); // Toma el día actual por defecto

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            agenda.Add(new
                            {
                                idToma = reader["ID_TOMA"],
                                fechaHoraProgramada = reader["FECHA_HORA_PROGRAMADA"],
                                estadoToma = reader["ESTADO"].ToString(),
                                idMedicamento = reader["ID_MEDICAMENTO"],
                                nombreMedicamento = reader["NOMBRE_MEDICAMENTO"].ToString(),
                                perfil = new
                                {
                                    idPerfil = reader["ID_PERFIL"],
                                    nombre = reader["NOMBRE_PERFIL"].ToString(),
                                    relacion = reader["TIPO_PERFIL"].ToString(),
                                    fechaNacimiento = Convert.ToDateTime(reader["FECHA_NACIMIENTO"]).ToString("yyyy-MM-dd")
                                }
                            });
                        }
                    }
                }
            }
            return Ok(agenda);
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    [HttpPost("agenda/actualizar-toma")]
    public IActionResult ActualizarEstadoToma([FromBody] TomaActualizacionDto modelo)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                // Puedes usar un UPDATE directo si no tienes un SP para actualizar la toma
                string query = "UPDATE TOMA SET ID_ESTADO = (SELECT ID_ESTADO FROM ESTADO_TOMA WHERE NOMBRE = @NuevoEstado), FECHA_HORA_CONFIRMADA = GETDATE() WHERE ID_TOMA = @IdToma";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdToma", modelo.IdToma);
                    cmd.Parameters.AddWithValue("@NuevoEstado", modelo.NuevoEstado); // 'Tomada' u 'Omitida'

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { success = true, message = "Estado actualizado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("medicamento/editar")]
    public IActionResult EditarMedicamento([FromBody] MedicamentoEdicionDto modelo)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Medicamento_Editar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", modelo.IdUsuario);
                    cmd.Parameters.AddWithValue("@IdMedicamento", modelo.IdMedicamento);
                    cmd.Parameters.AddWithValue("@IdVia", modelo.IdVia);
                    cmd.Parameters.AddWithValue("@NombreMedicamento", modelo.NombreMedicamento);
                    cmd.Parameters.AddWithValue("@Foto", string.IsNullOrEmpty(modelo.Foto) ? (object)DBNull.Value : modelo.Foto);
                    cmd.Parameters.AddWithValue("@IntervaloHoras", modelo.IntervaloHoras);
                    cmd.Parameters.AddWithValue("@DuracionDias", modelo.DuracionDias);
                    cmd.Parameters.AddWithValue("@HoraPrimeraToma", modelo.HoraPrimeraToma); // Formato "HH:mm"

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            long idMed = Convert.ToInt64(reader["ID_MEDICAMENTO"]);
                            int tomasRegeneradas = Convert.ToInt32(reader["TOMAS_REGENERADAS"]);

                            return Ok(new
                            {
                                success = true,
                                message = "Medicamento actualizado correctamente",
                                idMedicamento = idMed,
                                tomasRegeneradas = tomasRegeneradas
                            });
                        }
                    }
                }
            }
            return BadRequest(new { success = false, message = "No se pudo actualizar el medicamento." });
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
    }

    [HttpGet("medicamentos/concluidos/usuario/{idUsuario}")]
    public IActionResult ListarMedicamentosConcluidosPorUsuario(long idUsuario)
    {
        var concluidos = new List<object>();

        try
        {
            using (SqlConnection conn = new SqlConnection(CadenaSql))
            {
                // Primero obtenemos todos los perfiles que le pertenecen al usuario
                var perfilesIds = new List<long>();
                using (SqlCommand cmdPerfil = new SqlCommand("SELECT ID_PERFIL FROM PERFIL_CUIDADO WHERE ID_USUARIO = @IdUsuario", conn))
                {
                    cmdPerfil.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    conn.Open();
                    using (SqlDataReader rPerfil = cmdPerfil.ExecuteReader())
                    {
                        while (rPerfil.Read())
                        {
                            perfilesIds.Add(Convert.ToInt64(rPerfil["ID_PERFIL"]));
                        }
                    }
                }

                // Para cada perfil, traemos sus medicamentos concluidos usando tu SP original
                foreach (var idPerfil in perfilesIds)
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Medicamento_ListarConcluidos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        cmd.Parameters.AddWithValue("@IdPerfil", idPerfil);

                        if (conn.State != ConnectionState.Open) conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                concluidos.Add(new
                                {
                                    idMedicamento = reader["ID_MEDICAMENTO"],
                                    nombreMedicamento = reader["NOMBRE_MEDICAMENTO"].ToString(),
                                    foto = reader["FOTO"] != DBNull.Value ? reader["FOTO"].ToString() : null,
                                    viaAdministracion = reader["VIA_ADMINISTRACION"].ToString(),
                                    fechaInicio = Convert.ToDateTime(reader["FECHA_INICIO"]).ToString("yyyy-MM-dd"),
                                    fechaFin = Convert.ToDateTime(reader["FECHA_FIN"]).ToString("yyyy-MM-dd"),
                                    diasTomados = reader["DIAS_TOMADOS"],
                                    tomasTomadas = reader["TOMAS_TOMADAS"],
                                    tomasTotales = reader["TOMAS_TOTALES"]
                                });
                            }
                        }
                    }
                }
            }

            return Ok(concluidos);
        }
        catch (SqlException ex)
        {
            return RespuestaError(ex);
        }
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

public class PerfilCreacionDto
{
    public long IdUsuario { get; set; }
    public long IdTipoPerfil { get; set; }
    public string Nombre { get; set; }
    public DateTime FechaNacimiento { get; set; }
}

public class UsuarioLoginDto
{
    public string Correo { get; set; }
    public string Contrasena { get; set; }
}

public class MedicamentoCreacionDto
{
    public long IdUsuario { get; set; }
    public long IdPerfil { get; set; }
    public long IdVia { get; set; }
    public string NombreMedicamento { get; set; }
    public string? Foto { get; set; } = null; // Opcional
    public int IntervaloHoras { get; set; }
    public int DuracionDias { get; set; }
    public string HoraInicio { get; set; } // Coincide con TIME
    public DateTime? FechaInicio { get; set; } = null; // Opcional
}

public class TomaActualizacionDto
{
    public long IdToma { get; set; }
    public string NuevoEstado { get; set; }
}

public class MedicamentoEdicionDto
{
    public long IdUsuario { get; set; }
    public long IdMedicamento { get; set; }
    public long IdVia { get; set; }
    public string NombreMedicamento { get; set; }
    public string? Foto { get; set; } = null;
    public int IntervaloHoras { get; set; }
    public int DuracionDias { get; set; }
    public string HoraPrimeraToma { get; set; }
}