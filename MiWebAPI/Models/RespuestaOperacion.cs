namespace MiWebAPI.Models
{
    public class RespuestaOperacion
    {
        public int NumeroError { get; set; }
        public string Respuesta { get; set; } = string.Empty;
        public bool IsSuccess => NumeroError == 0;
    }
}
