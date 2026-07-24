using VoxMorph.Entidades.Enumeraciones;

namespace VoxMorph.Entidades.Modelos;

public sealed class AtajoGlobal
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public AccionAtajo Accion { get; set; }
    public string Combinacion { get; set; } = string.Empty;
    public string? IdSonido { get; set; }
    public bool Habilitado { get; set; } = true;

    public static List<AtajoGlobal> CrearPredeterminados()
    {
        return
        [
            new() { Id = "silencio", Accion = AccionAtajo.SilenciarMicrofono, Combinacion = "Ctrl+Alt+M" },
            new() { Id = "motor", Accion = AccionAtajo.ActivarDesactivarMotor, Combinacion = "Ctrl+Alt+V" },
            new() { Id = "perfil-siguiente", Accion = AccionAtajo.PerfilSiguiente, Combinacion = "Ctrl+Alt+Right" },
            new() { Id = "perfil-anterior", Accion = AccionAtajo.PerfilAnterior, Combinacion = "Ctrl+Alt+Left" },
            new() { Id = "detener-sonidos", Accion = AccionAtajo.DetenerSonidos, Combinacion = "Ctrl+Alt+S" }
        ];
    }
}
