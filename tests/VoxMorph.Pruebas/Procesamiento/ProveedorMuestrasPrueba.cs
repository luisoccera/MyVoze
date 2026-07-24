using NAudio.Wave;

namespace VoxMorph.Pruebas.Procesamiento;

internal sealed class ProveedorMuestrasPrueba : ISampleProvider
{
    private readonly float[] _muestras;
    private int _posicion;

    public ProveedorMuestrasPrueba(params float[] muestras)
    {
        _muestras = muestras;
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48_000, 1);
    }

    public WaveFormat WaveFormat { get; }

    public int Read(float[] buffer, int offset, int count)
    {
        var disponibles = Math.Min(count, _muestras.Length - _posicion);
        if (disponibles <= 0)
        {
            return 0;
        }

        Array.Copy(_muestras, _posicion, buffer, offset, disponibles);
        _posicion += disponibles;
        return disponibles;
    }
}
