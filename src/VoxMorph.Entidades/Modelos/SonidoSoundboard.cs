namespace VoxMorph.Entidades.Modelos;

public sealed class SonidoSoundboard
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nombre { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public float Volumen { get; set; } = 0.8f;
    public string? Atajo { get; set; }

    public SonidoSoundboard Clonar()
    {
        return new SonidoSoundboard
        {
            Id = Id,
            Nombre = Nombre,
            Ruta = Ruta,
            Volumen = Volumen,
            Atajo = Atajo
        };
    }
}
