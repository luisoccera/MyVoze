using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using VoxMorph.Audio.Conversion;
using VoxMorph.Audio.Mezcla;
using VoxMorph.Audio.Monitoreo;
using VoxMorph.Entidades.Enumeraciones;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Procesamiento;

public sealed class MotorAudio : IMotorAudio
{
    private readonly object _sincronizacion = new();
    private MMDeviceEnumerator? _enumerador;
    private MMDevice? _dispositivoEntrada;
    private MMDevice? _dispositivoSalida;
    private MMDevice? _dispositivoMonitoreo;
    private WasapiCapture? _captura;
    private WasapiOut? _salida;
    private WasapiOut? _salidaMonitoreo;
    private BufferedWaveProvider? _bufferEntrada;
    private ReductorRuidoNeuronalSampleProvider? _reductorRuidoNeuronal;
    private IConversorVoz? _conversorVoz;
    private MezcladorSoundboardSampleProvider? _mezclador;
    private BufferCircularMuestras? _bufferMonitoreo;
    private DuplicadorMuestrasSampleProvider? _duplicador;
    private string? _idSalidaPrincipal;
    private bool _deteniendoIntencionalmente;
    private bool _desechado;

    public EstadoMotorAudio Estado { get; private set; } = EstadoMotorAudio.Detenido;
    public string? UltimoError { get; private set; }
    public float NivelEntrada => _conversorVoz?.NivelEntrada ?? 0;
    public float NivelSalida => _mezclador?.NivelSalida ?? _conversorVoz?.NivelSalida ?? 0;
    public bool Silenciado { get; private set; }

    public event EventHandler<EstadoMotorAudio>? EstadoCambiado;

    public void Iniciar(
        string idEntrada,
        string idSalida,
        PerfilVoz perfil,
        string? idMonitoreo = null,
        bool monitoreoHabilitado = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idEntrada);
        ArgumentException.ThrowIfNullOrWhiteSpace(idSalida);
        ArgumentNullException.ThrowIfNull(perfil);

        lock (_sincronizacion)
        {
            ObjectDisposedException.ThrowIf(_desechado, this);
            if (Estado is EstadoMotorAudio.Iniciando or EstadoMotorAudio.Procesando)
            {
                return;
            }

            CambiarEstado(EstadoMotorAudio.Iniciando);
            UltimoError = null;
            _deteniendoIntencionalmente = false;

            try
            {
                _enumerador = new MMDeviceEnumerator();
                _dispositivoEntrada = _enumerador.GetDevice(idEntrada);
                _dispositivoSalida = _enumerador.GetDevice(idSalida);
                _idSalidaPrincipal = idSalida;
                _captura = new WasapiCapture(_dispositivoEntrada);

                if (_captura.WaveFormat.Channels is < 1 or > 2)
                {
                    throw new NotSupportedException(
                        $"El microfono usa {_captura.WaveFormat.Channels} canales. MyVoze admite entradas mono o estereo.");
                }

                _bufferEntrada = new BufferedWaveProvider(_captura.WaveFormat)
                {
                    BufferDuration = TimeSpan.FromMilliseconds(250),
                    DiscardOnBufferOverflow = true,
                    ReadFully = true
                };

                ISampleProvider muestras = _bufferEntrada.ToSampleProvider();
                if (muestras.WaveFormat.Channels == 2)
                {
                    muestras = new StereoToMonoSampleProvider(muestras)
                    {
                        LeftVolume = 0.5f,
                        RightVolume = 0.5f
                    };
                }

                if (muestras.WaveFormat.SampleRate != 48_000)
                {
                    muestras = new WdlResamplingSampleProvider(muestras, 48_000);
                }

                _reductorRuidoNeuronal = new ReductorRuidoNeuronalSampleProvider(
                    muestras,
                    perfil.ReduccionRuido);
                _conversorVoz = new ConversorVozDspSampleProvider(
                    _reductorRuidoNeuronal,
                    perfil);
                _conversorVoz.EstablecerSilencio(Silenciado);
                _mezclador = new MezcladorSoundboardSampleProvider(_conversorVoz);
                _bufferMonitoreo = new BufferCircularMuestras(
                    _mezclador.WaveFormat,
                    _mezclador.WaveFormat.SampleRate);
                _duplicador = new DuplicadorMuestrasSampleProvider(_mezclador, _bufferMonitoreo);

                _salida = CrearSalida(_dispositivoSalida, _duplicador);
                _captura.DataAvailable += AlRecibirAudio;
                _captura.RecordingStopped += AlDetenerCaptura;
                _salida.PlaybackStopped += AlDetenerSalida;
                _salida.Play();
                _captura.StartRecording();
                CambiarEstado(EstadoMotorAudio.Procesando);

                if (monitoreoHabilitado)
                {
                    ConfigurarMonitoreoInterno(idMonitoreo, true);
                }
            }
            catch (Exception excepcion)
            {
                UltimoError = CrearMensajeError(excepcion);
                LiberarRecursos();
                CambiarEstado(EstadoMotorAudio.Error);
                throw new InvalidOperationException(UltimoError, excepcion);
            }
        }
    }

    public void Detener()
    {
        lock (_sincronizacion)
        {
            if (Estado == EstadoMotorAudio.Detenido)
            {
                return;
            }

            _deteniendoIntencionalmente = true;
            CambiarEstado(EstadoMotorAudio.Deteniendo);
            LiberarRecursos();
            CambiarEstado(EstadoMotorAudio.Detenido);
        }
    }

    public void ActualizarPerfil(PerfilVoz perfil)
    {
        ArgumentNullException.ThrowIfNull(perfil);
        lock (_sincronizacion)
        {
            _reductorRuidoNeuronal?.ActualizarIntensidad(perfil.ReduccionRuido);
            _conversorVoz?.ActualizarPerfil(perfil);
        }
    }

    public void EstablecerSilencio(bool silenciado)
    {
        Silenciado = silenciado;
        _conversorVoz?.EstablecerSilencio(silenciado);
    }

    public void ConfigurarMonitoreo(string? idDispositivo, bool habilitado)
    {
        lock (_sincronizacion)
        {
            ObjectDisposedException.ThrowIf(_desechado, this);
            if (Estado != EstadoMotorAudio.Procesando)
            {
                return;
            }

            ConfigurarMonitoreoInterno(idDispositivo, habilitado);
        }
    }

    public void ReproducirSonido(string ruta, float volumen)
    {
        lock (_sincronizacion)
        {
            if (Estado != EstadoMotorAudio.Procesando || _mezclador is null)
            {
                throw new InvalidOperationException("Inicia el motor antes de reproducir sonidos.");
            }

            _mezclador.Reproducir(ruta, volumen);
        }
    }

    public void DetenerSonidos()
    {
        lock (_sincronizacion)
        {
            _mezclador?.DetenerTodo();
        }
    }

    public void Dispose()
    {
        lock (_sincronizacion)
        {
            if (_desechado)
            {
                return;
            }

            _deteniendoIntencionalmente = true;
            LiberarRecursos();
            _desechado = true;
            Estado = EstadoMotorAudio.Detenido;
        }

        GC.SuppressFinalize(this);
    }

    private WasapiOut CrearSalida(MMDevice dispositivo, ISampleProvider origen)
    {
        ISampleProvider salidaProcesada = origen;
        var formatoSalida = dispositivo.AudioClient.MixFormat;
        if (salidaProcesada.WaveFormat.SampleRate != formatoSalida.SampleRate)
        {
            salidaProcesada = new WdlResamplingSampleProvider(salidaProcesada, formatoSalida.SampleRate);
        }

        salidaProcesada = AjustarCanales(salidaProcesada, formatoSalida.Channels);
        var salida = new WasapiOut(dispositivo, AudioClientShareMode.Shared, true, 35);
        salida.Init(new SampleToWaveProvider(salidaProcesada));
        return salida;
    }

    private void ConfigurarMonitoreoInterno(string? idDispositivo, bool habilitado)
    {
        LiberarMonitoreo();
        if (!habilitado)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(idDispositivo))
        {
            throw new InvalidOperationException("Selecciona unos audifonos para activar el monitoreo.");
        }

        if (string.Equals(idDispositivo, _idSalidaPrincipal, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("El monitoreo debe usar una salida distinta de la salida principal.");
        }

        if (_enumerador is null || _bufferMonitoreo is null || _duplicador is null)
        {
            return;
        }

        _bufferMonitoreo.Limpiar();
        _dispositivoMonitoreo = _enumerador.GetDevice(idDispositivo);
        _salidaMonitoreo = CrearSalida(_dispositivoMonitoreo, _bufferMonitoreo);
        _salidaMonitoreo.PlaybackStopped += AlDetenerSalidaMonitoreo;
        _duplicador.Habilitado = true;
        _salidaMonitoreo.Play();
    }

    private void AlRecibirAudio(object? sender, WaveInEventArgs evento)
    {
        try
        {
            _bufferEntrada?.AddSamples(evento.Buffer, 0, evento.BytesRecorded);
        }
        catch (Exception excepcion)
        {
            ManejarFalloDispositivo($"Se perdio el flujo del microfono: {excepcion.Message}");
        }
    }

    private void AlDetenerCaptura(object? sender, StoppedEventArgs evento)
    {
        if (!_deteniendoIntencionalmente)
        {
            ManejarFalloDispositivo(
                evento.Exception is null
                    ? "El microfono se desconecto o dejo de responder."
                    : $"El microfono se desconecto: {evento.Exception.Message}");
        }
    }

    private void AlDetenerSalida(object? sender, StoppedEventArgs evento)
    {
        if (!_deteniendoIntencionalmente)
        {
            ManejarFalloDispositivo(
                evento.Exception is null
                    ? "La salida de audio se desconecto o dejo de responder."
                    : $"La salida de audio se desconecto: {evento.Exception.Message}");
        }
    }

    private void AlDetenerSalidaMonitoreo(object? sender, StoppedEventArgs evento)
    {
        if (!_deteniendoIntencionalmente)
        {
            UltimoError = evento.Exception is null
                ? "Los audifonos de monitoreo se desconectaron."
                : $"Se perdio el monitoreo: {evento.Exception.Message}";
            ThreadPool.QueueUserWorkItem(_ =>
            {
                lock (_sincronizacion)
                {
                    if (!_deteniendoIntencionalmente)
                    {
                        LiberarMonitoreo();
                    }
                }
            });
        }
    }

    private void ManejarFalloDispositivo(string mensaje)
    {
        UltimoError = mensaje;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            lock (_sincronizacion)
            {
                if (_deteniendoIntencionalmente || Estado != EstadoMotorAudio.Procesando)
                {
                    return;
                }

                _deteniendoIntencionalmente = true;
                LiberarRecursos();
                CambiarEstado(EstadoMotorAudio.Error);
            }
        });
    }

    private static ISampleProvider AjustarCanales(ISampleProvider origen, int canalesSalida)
    {
        return canalesSalida switch
        {
            1 => origen,
            2 => new MonoToStereoSampleProvider(origen),
            _ => throw new NotSupportedException(
                $"La salida seleccionada usa {canalesSalida} canales. Selecciona una salida mono o estereo.")
        };
    }

    private void LiberarMonitoreo()
    {
        if (_duplicador is not null)
        {
            _duplicador.Habilitado = false;
        }

        if (_salidaMonitoreo is not null)
        {
            _salidaMonitoreo.PlaybackStopped -= AlDetenerSalidaMonitoreo;
            try
            {
                _salidaMonitoreo.Stop();
            }
            catch
            {
            }

            _salidaMonitoreo.Dispose();
        }

        _dispositivoMonitoreo?.Dispose();
        _salidaMonitoreo = null;
        _dispositivoMonitoreo = null;
        _bufferMonitoreo?.Limpiar();
    }

    private void LiberarRecursos()
    {
        LiberarMonitoreo();
        if (_captura is not null)
        {
            _captura.DataAvailable -= AlRecibirAudio;
            _captura.RecordingStopped -= AlDetenerCaptura;
            try
            {
                _captura.StopRecording();
            }
            catch
            {
            }
        }

        if (_salida is not null)
        {
            _salida.PlaybackStopped -= AlDetenerSalida;
            try
            {
                _salida.Stop();
            }
            catch
            {
            }
        }

        _captura?.Dispose();
        _salida?.Dispose();
        _conversorVoz?.Dispose();
        _reductorRuidoNeuronal?.Dispose();
        _dispositivoEntrada?.Dispose();
        _dispositivoSalida?.Dispose();
        _enumerador?.Dispose();

        _captura = null;
        _salida = null;
        _dispositivoEntrada = null;
        _dispositivoSalida = null;
        _enumerador = null;
        _bufferEntrada = null;
        _reductorRuidoNeuronal = null;
        _conversorVoz = null;
        _mezclador = null;
        _bufferMonitoreo = null;
        _duplicador = null;
        _idSalidaPrincipal = null;
    }

    private void CambiarEstado(EstadoMotorAudio estado)
    {
        Estado = estado;
        EstadoCambiado?.Invoke(this, estado);
    }

    private static string CrearMensajeError(Exception excepcion)
    {
        return excepcion switch
        {
            UnauthorizedAccessException => "Windows no permitio acceder al microfono. Revisa los permisos de privacidad.",
            DllNotFoundException => "No se pudo cargar el modulo de limpieza IA integrado.",
            BadImageFormatException => "El modulo de limpieza IA no es compatible con esta arquitectura.",
            FileNotFoundException => excepcion.Message,
            NotSupportedException => excepcion.Message,
            InvalidOperationException => excepcion.Message,
            _ => $"No fue posible iniciar el audio. {excepcion.Message}"
        };
    }
}
