using NAudio.Wave;

namespace VoxMorph.Audio.Monitoreo;

internal sealed class BufferCircularMuestras : ISampleProvider
{
    private readonly object _sincronizacion = new();
    private readonly float[] _buffer;
    private int _lectura;
    private int _escritura;
    private int _cantidad;

    public BufferCircularMuestras(WaveFormat formato, int capacidad)
    {
        WaveFormat = formato;
        _buffer = new float[Math.Max(capacidad, 1)];
    }

    public WaveFormat WaveFormat { get; }

    public void Escribir(float[] origen, int offset, int cantidad)
    {
        lock (_sincronizacion)
        {
            for (var indice = 0; indice < cantidad; indice++)
            {
                if (_cantidad == _buffer.Length)
                {
                    _lectura = (_lectura + 1) % _buffer.Length;
                    _cantidad--;
                }

                _buffer[_escritura] = origen[offset + indice];
                _escritura = (_escritura + 1) % _buffer.Length;
                _cantidad++;
            }
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (_sincronizacion)
        {
            var disponibles = Math.Min(count, _cantidad);
            for (var indice = 0; indice < disponibles; indice++)
            {
                buffer[offset + indice] = _buffer[_lectura];
                _lectura = (_lectura + 1) % _buffer.Length;
            }

            _cantidad -= disponibles;
            Array.Clear(buffer, offset + disponibles, count - disponibles);
            return count;
        }
    }

    public void Limpiar()
    {
        lock (_sincronizacion)
        {
            _lectura = 0;
            _escritura = 0;
            _cantidad = 0;
            Array.Clear(_buffer);
        }
    }
}
