public class UsuarioModel
{
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string Password { get; set; } // Coincide con la propiedad 'password' que mandas desde JS
    public string TipoUso { get; set; }
}