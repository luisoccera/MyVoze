using System.Runtime.InteropServices;
using NAudio.Wave;

namespace VoxMorph.Audio.Procesamiento;

public sealed class ReductorRuidoNeuronalSampleProvider : ISampleProvider, IDisposable
{
    private const int FrecuenciaMuestreo = 48_000;
    private const int MuestrasPorMarco = 480;
    private const float EscalaPcm16 = short.MaxValue;
    private const float EscalaPcm16Inversa = 1.0f / EscalaPcm16;
    private readonly ISampleProvider _origen;
    private readonly float[] _entrada = new float[MuestrasPorMarco];
    private readonly float[] _salida = new float[MuestrasPorMarco];
    private IntPtr _estado;
    private int _indiceSalida;
    private int _muestrasDisponibles;
    private float _intensidad;
    private bool _desechado;

    public ReductorRuidoNeuronalSampleProvider(ISampleProvider origen, float intensidad)
    {
        ArgumentNullException.ThrowIfNull(origen);
        if (origen.WaveFormat.Channels != 1 || origen.WaveFormat.SampleRate != FrecuenciaMuestreo)
        {
            throw new ArgumentException(
                "RNNoise requiere audio mono a 48 kHz.",
                nameof(origen));
        }

        _origen = origen;
        _intensidad = Math.Clamp(intensidad, 0, 1);
        _estado = RnNoiseNative.Crear(IntPtr.Zero);
        if (_estado == IntPtr.Zero)
        {
            throw new InvalidOperationException("RNNoise no pudo inicializar su modelo neuronal.");
        }
    }

    public WaveFormat WaveFormat => _origen.WaveFormat;

    public void ActualizarIntensidad(float intensidad)
        => Volatile.Write(ref _intensidad, Math.Clamp(intensidad, 0, 1));

    public int Read(float[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_desechado, this);
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (offset + count > buffer.Length)
        {
            throw new ArgumentException("El rango solicitado excede el búfer.", nameof(count));
        }

        var escritos = 0;
        while (escritos < count)
        {
            if (_muestrasDisponibles == 0 && !ProcesarSiguienteMarco())
            {
                break;
            }

            var cantidad = Math.Min(count - escritos, _muestrasDisponibles);
            Array.Copy(_salida, _indiceSalida, buffer, offset + escritos, cantidad);
            _indiceSalida += cantidad;
            _muestrasDisponibles -= cantidad;
            escritos += cantidad;
        }

        return escritos;
    }

    public void Dispose()
    {
        if (_desechado)
        {
            return;
        }

        if (_estado != IntPtr.Zero)
        {
            RnNoiseNative.Destruir(_estado);
            _estado = IntPtr.Zero;
        }

        _desechado = true;
        GC.SuppressFinalize(this);
    }

    private bool ProcesarSiguienteMarco()
    {
        var leidos = 0;
        while (leidos < MuestrasPorMarco)
        {
            var cantidad = _origen.Read(_entrada, leidos, MuestrasPorMarco - leidos);
            if (cantidad == 0)
            {
                break;
            }

            leidos += cantidad;
        }

        if (leidos == 0)
        {
            return false;
        }

        if (leidos < MuestrasPorMarco)
        {
            Array.Clear(_entrada, leidos, MuestrasPorMarco - leidos);
        }

        var intensidad = Volatile.Read(ref _intensidad);
        if (intensidad <= 0)
        {
            Array.Copy(_entrada, _salida, leidos);
        }
        else
        {
            for (var indice = 0; indice < MuestrasPorMarco; indice++)
            {
                _entrada[indice] *= EscalaPcm16;
            }

            RnNoiseNative.ProcesarMarco(_estado, _salida, _entrada);
            for (var indice = 0; indice < leidos; indice++)
            {
                var original = _entrada[indice] * EscalaPcm16Inversa;
                var limpia = _salida[indice] * EscalaPcm16Inversa;
                _salida[indice] = original * (1 - intensidad) + limpia * intensidad;
            }
        }

        _indiceSalida = 0;
        _muestrasDisponibles = leidos;
        return true;
    }

    private static class RnNoiseNative
    {
        private const string Biblioteca = "rnnoise";

        [DllImport(Biblioteca, EntryPoint = "rnnoise_create", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Crear(IntPtr modelo);

        [DllImport(Biblioteca, EntryPoint = "rnnoise_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Destruir(IntPtr estado);

        [DllImport(Biblioteca, EntryPoint = "rnnoise_process_frame", CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ProcesarMarco(
            IntPtr estado,
            [Out] float[] salida,
            [In] float[] entrada);
    }
}
