using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace VoxMorph.Audio.Mezcla;

public sealed class MezcladorSoundboardSampleProvider : ISampleProvider
{
    private const float LimiteSalida = 0.95f;
    private readonly object _sincronizacion = new();
    private readonly ISampleProvider _voz;
    private readonly List<ReproduccionSonido> _reproducciones = [];
    private float[] _bufferTemporal = [];
    private float _nivelSalida;

    public MezcladorSoundboardSampleProvider(ISampleProvider voz)
    {
        ArgumentNullException.ThrowIfNull(voz);
        if (voz.WaveFormat.Channels != 1)
        {
            throw new ArgumentException("El mezclador requiere una fuente mono.", nameof(voz));
        }

        _voz = voz;
    }

    public WaveFormat WaveFormat => _voz.WaveFormat;
    public float NivelSalida => Volatile.Read(ref _nivelSalida);

    public void Reproducir(string ruta, float volumen)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruta);
        if (!File.Exists(ruta))
        {
            throw new FileNotFoundException("El archivo de sonido ya no existe.", ruta);
        }

        var muestras = CargarMuestras(ruta, WaveFormat.SampleRate);
        lock (_sincronizacion)
        {
            _reproducciones.Add(new ReproduccionSonido(muestras, Math.Clamp(volumen, 0, 1.5f)));
        }
    }

    public void DetenerTodo()
    {
        lock (_sincronizacion)
        {
            _reproducciones.Clear();
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var leidos = _voz.Read(buffer, offset, count);
        if (leidos == 0)
        {
            return 0;
        }

        lock (_sincronizacion)
        {
            if (_bufferTemporal.Length < leidos)
            {
                _bufferTemporal = new float[leidos];
            }

            for (var indice = _reproducciones.Count - 1; indice >= 0; indice--)
            {
                Array.Clear(_bufferTemporal, 0, leidos);
                var reproduccion = _reproducciones[indice];
                var muestrasSonido = reproduccion.Leer(_bufferTemporal, leidos);
                for (var muestra = 0; muestra < muestrasSonido; muestra++)
                {
                    buffer[offset + muestra] += _bufferTemporal[muestra];
                }

                if (reproduccion.Finalizada)
                {
                    _reproducciones.RemoveAt(indice);
                }
            }
        }

        var suma = 0.0;
        for (var indice = offset; indice < offset + leidos; indice++)
        {
            buffer[indice] = Math.Clamp(buffer[indice], -LimiteSalida, LimiteSalida);
            suma += buffer[indice] * buffer[indice];
        }

        Volatile.Write(ref _nivelSalida, (float)Math.Sqrt(suma / leidos));
        return leidos;
    }

    private static float[] CargarMuestras(string ruta, int frecuenciaDestino)
    {
        const int duracionMaximaSegundos = 600;
        using var lector = new AudioFileReader(ruta);
        ISampleProvider proveedor = lector;
        if (proveedor.WaveFormat.Channels == 2)
        {
            proveedor = new StereoToMonoSampleProvider(proveedor)
            {
                LeftVolume = 0.5f,
                RightVolume = 0.5f
            };
        }
        else if (proveedor.WaveFormat.Channels != 1)
        {
            throw new NotSupportedException("El soundboard admite archivos mono o estereo.");
        }

        if (proveedor.WaveFormat.SampleRate != frecuenciaDestino)
        {
            proveedor = new WdlResamplingSampleProvider(proveedor, frecuenciaDestino);
        }

        var resultado = new List<float>();
        var bloque = new float[8192];
        int leidos;
        while ((leidos = proveedor.Read(bloque, 0, bloque.Length)) > 0)
        {
            if (resultado.Count + leidos > frecuenciaDestino * duracionMaximaSegundos)
            {
                throw new NotSupportedException("El soundboard admite sonidos de hasta 10 minutos.");
            }

            resultado.AddRange(bloque.AsSpan(0, leidos).ToArray());
        }

        return resultado.ToArray();
    }

    private sealed class ReproduccionSonido(float[] muestras, float volumen)
    {
        private int _posicion;

        public bool Finalizada => _posicion >= muestras.Length;

        public int Leer(float[] destino, int cantidad)
        {
            var disponibles = Math.Min(cantidad, muestras.Length - _posicion);
            for (var indice = 0; indice < disponibles; indice++)
            {
                destino[indice] = muestras[_posicion + indice] * volumen;
            }

            _posicion += disponibles;
            return disponibles;
        }
    }
}
