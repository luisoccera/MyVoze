using VoxMorph.Entidades.Enumeraciones;

namespace VoxMorph.Entidades.Modelos;

public sealed class DispositivoAudio
{
    public required string Id { get; init; }
    public required string Nombre { get; init; }
    public required TipoDispositivoAudio Tipo { get; init; }
    public bool EsVbCable { get; init; }
    public bool EsEntradaVirtualVbCable { get; init; }

    public override string ToString() => Nombre;
}
