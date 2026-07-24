using NAudio.Wave;

namespace VoxMorph.Audio.Monitoreo;

internal sealed class DuplicadorMuestrasSampleProvider(
    ISampleProvider origen,
    BufferCircularMuestras destino) : ISampleProvider
{
    public WaveFormat WaveFormat => origen.WaveFormat;
    public bool Habilitado { get; set; }

    public int Read(float[] buffer, int offset, int count)
    {
        var leidos = origen.Read(buffer, offset, count);
        if (Habilitado && leidos > 0)
        {
            destino.Escribir(buffer, offset, leidos);
        }

        return leidos;
    }
}
