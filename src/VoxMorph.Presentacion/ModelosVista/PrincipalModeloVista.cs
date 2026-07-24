using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using VoxMorph.Audio.Dispositivos;
using VoxMorph.Audio.Procesamiento;
using VoxMorph.Entidades.Configuracion;
using VoxMorph.Entidades.Enumeraciones;
using VoxMorph.Entidades.Modelos;
using VoxMorph.Entidades.Perfiles;
using VoxMorph.Presentacion.Infraestructura;
using VoxMorph.Presentacion.Servicios;

namespace VoxMorph.Presentacion.ModelosVista;

public sealed class PrincipalModeloVista : ObservableBase, IDisposable
{
    private readonly IServicioDispositivosAudio _servicioDispositivos;
    private readonly IMotorAudio _motor;
    private readonly IServicioConfiguracion _servicioConfiguracion;
    private readonly IServicioSeleccionArchivos _servicioSeleccionArchivos;
    private readonly IServicioAtajosGlobales _servicioAtajos;
    private readonly DispatcherTimer _temporizadorNiveles;
    private ConfiguracionAplicacion _configuracion = new();
    private DispositivoAudio? _entradaSeleccionada;
    private DispositivoAudio? _salidaSeleccionada;
    private DispositivoAudio? _monitoreoSeleccionado;
    private PerfilVoz? _perfilSeleccionado;
    private SonidoSoundboard? _sonidoSeleccionado;
    private string _estadoTexto = "Preparando...";
    private string _mensajeError = string.Empty;
    private string _nombreNuevoPerfil = "Mi voz";
    private double _nivelEntrada;
    private double _nivelSalida;
    private double _pitch = 1;
    private double _gananciaDb;
    private double _gravesDb;
    private double _mediosDb;
    private double _agudosDb;
    private double _umbralRuidoDb = -48;
    private double _reduccionRuido = 0.72;
    private double _feminizacion;
    private double _distorsion;
    private double _robotMezcla;
    private double _robotFrecuenciaHz = 55;
    private double _filtroTelefonico;
    private double _tremoloMezcla;
    private double _tremoloFrecuenciaHz = 5;
    private double _ecoMezcla;
    private double _ecoMilisegundos = 180;
    private double _mezclaEfecto = 1;
    private double _volumenFinal = 1;
    private bool _monitoreoHabilitado;
    private string _atajoSilencio = "Ctrl+Alt+M";
    private string _atajoMotor = "Ctrl+Alt+V";
    private string _atajoPerfilSiguiente = "Ctrl+Alt+Right";
    private string _atajoPerfilAnterior = "Ctrl+Alt+Left";
    private string _atajoDetenerSonidos = "Ctrl+Alt+S";
    private Window? _ventana;
    private bool _actualizandoControles;
    private bool _inicializado;
    private bool _ocupado;
    private bool _desechado;

    public PrincipalModeloVista(
        IServicioDispositivosAudio servicioDispositivos,
        IMotorAudio motor,
        IServicioConfiguracion servicioConfiguracion,
        IServicioSeleccionArchivos servicioSeleccionArchivos,
        IServicioAtajosGlobales servicioAtajos)
    {
        _servicioDispositivos = servicioDispositivos;
        _motor = motor;
        _servicioConfiguracion = servicioConfiguracion;
        _servicioSeleccionArchivos = servicioSeleccionArchivos;
        _servicioAtajos = servicioAtajos;

        ComandoIniciarDetener = new ComandoAsincrono(IniciarDetenerAsync, () => !Ocupado);
        ComandoActualizarDispositivos = new Comando(ActualizarDispositivos, () => !Ocupado);
        ComandoSilenciar = new Comando(CambiarSilencio);
        ComandoGuardarPerfil = new Comando(GuardarPerfilPersonalizado);
        ComandoEliminarPerfil = new Comando(EliminarPerfil, PuedeEliminarPerfil);
        ComandoRestablecerPerfil = new Comando(RestablecerPerfil);
        ComandoImportarSonidos = new Comando(ImportarSonidos);
        ComandoReproducirSonido = new ComandoAsincrono(ReproducirSonidoAsync, () => SonidoSeleccionado is not null);
        ComandoDetenerSonidos = new Comando(DetenerSonidos);
        ComandoEliminarSonido = new Comando(EliminarSonido, () => SonidoSeleccionado is not null);
        ComandoAplicarAtajos = new Comando(AplicarAtajos);

        _temporizadorNiveles = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(75)
        };
        _temporizadorNiveles.Tick += ActualizarIndicadores;
    }

    public ObservableCollection<DispositivoAudio> Entradas { get; } = [];
    public ObservableCollection<DispositivoAudio> SalidasVirtuales { get; } = [];
    public ObservableCollection<DispositivoAudio> Monitores { get; } = [];
    public ObservableCollection<PerfilVoz> Perfiles { get; } = [];
    public ObservableCollection<SonidoSoundboard> Sonidos { get; } = [];

    public ComandoAsincrono ComandoIniciarDetener { get; }
    public Comando ComandoActualizarDispositivos { get; }
    public Comando ComandoSilenciar { get; }
    public Comando ComandoGuardarPerfil { get; }
    public Comando ComandoEliminarPerfil { get; }
    public Comando ComandoRestablecerPerfil { get; }
    public Comando ComandoImportarSonidos { get; }
    public ComandoAsincrono ComandoReproducirSonido { get; }
    public Comando ComandoDetenerSonidos { get; }
    public Comando ComandoEliminarSonido { get; }
    public Comando ComandoAplicarAtajos { get; }

    public DispositivoAudio? EntradaSeleccionada
    {
        get => _entradaSeleccionada;
        set => Establecer(ref _entradaSeleccionada, value);
    }

    public DispositivoAudio? SalidaSeleccionada
    {
        get => _salidaSeleccionada;
        set
        {
            if (Establecer(ref _salidaSeleccionada, value))
            {
                Notificar(nameof(EstadoSalidaVirtual));
            }
        }
    }

    public DispositivoAudio? MonitoreoSeleccionado
    {
        get => _monitoreoSeleccionado;
        set
        {
            if (Establecer(ref _monitoreoSeleccionado, value)
                && MonitoreoHabilitado
                && _motor.Estado == EstadoMotorAudio.Procesando)
            {
                ConfigurarMonitoreo();
            }
        }
    }

    public bool MonitoreoHabilitado
    {
        get => _monitoreoHabilitado;
        set
        {
            if (Establecer(ref _monitoreoHabilitado, value)
                && _motor.Estado == EstadoMotorAudio.Procesando)
            {
                ConfigurarMonitoreo();
            }

            Notificar(nameof(EstadoMonitoreo));
        }
    }

    public string EstadoMonitoreo
        => MonitoreoHabilitado
            ? $"Activo: {MonitoreoSeleccionado?.Nombre ?? "sin dispositivo"}"
            : "Desactivado";

    public string EstadoSalidaVirtual
        => SalidaSeleccionada is null
            ? "VB-CABLE no detectado. Instálalo desde vb-cable.com y actualiza los dispositivos."
            : $"MyVoze → {SalidaSeleccionada.Nombre} → CABLE Output en Discord/Valorant";

    public SonidoSoundboard? SonidoSeleccionado
    {
        get => _sonidoSeleccionado;
        set
        {
            if (Establecer(ref _sonidoSeleccionado, value))
            {
                ComandoReproducirSonido.Actualizar();
                ComandoEliminarSonido.Actualizar();
            }
        }
    }

    public PerfilVoz? PerfilSeleccionado
    {
        get => _perfilSeleccionado;
        set
        {
            if (!Establecer(ref _perfilSeleccionado, value) || value is null)
            {
                return;
            }

            CargarControles(value);
            ComandoEliminarPerfil.Actualizar();
            _configuracion.IdUltimoPerfil = value.Id;
        }
    }

    public string EstadoTexto
    {
        get => _estadoTexto;
        private set => Establecer(ref _estadoTexto, value);
    }

    public string MensajeError
    {
        get => _mensajeError;
        private set => Establecer(ref _mensajeError, value);
    }

    public string NombreNuevoPerfil
    {
        get => _nombreNuevoPerfil;
        set => Establecer(ref _nombreNuevoPerfil, value);
    }

    public double NivelEntrada
    {
        get => _nivelEntrada;
        private set => Establecer(ref _nivelEntrada, value);
    }

    public double NivelSalida
    {
        get => _nivelSalida;
        private set => Establecer(ref _nivelSalida, value);
    }

    public bool Ocupado
    {
        get => _ocupado;
        private set
        {
            if (Establecer(ref _ocupado, value))
            {
                ComandoIniciarDetener.Actualizar();
                ComandoActualizarDispositivos.Actualizar();
            }
        }
    }

    public string TextoBotonPrincipal
        => _motor.Estado == EstadoMotorAudio.Procesando ? "Detener voz" : "Iniciar voz";

    public string TextoBotonSilencio
        => _motor.Silenciado ? "Activar microfono" : "Silenciar microfono";

    public bool EstaSaturando => NivelSalida >= 0.93;

    public double Pitch
    {
        get => _pitch;
        set => CambiarControl(ref _pitch, Math.Clamp(value, 0.5, 2.0));
    }

    public double GananciaDb
    {
        get => _gananciaDb;
        set => CambiarControl(ref _gananciaDb, Math.Clamp(value, -12, 12));
    }

    public double GravesDb
    {
        get => _gravesDb;
        set => CambiarControl(ref _gravesDb, Math.Clamp(value, -12, 12));
    }

    public double MediosDb
    {
        get => _mediosDb;
        set => CambiarControl(ref _mediosDb, Math.Clamp(value, -12, 12));
    }

    public double AgudosDb
    {
        get => _agudosDb;
        set => CambiarControl(ref _agudosDb, Math.Clamp(value, -12, 12));
    }

    public double UmbralRuidoDb
    {
        get => _umbralRuidoDb;
        set => CambiarControl(ref _umbralRuidoDb, Math.Clamp(value, -70, -20));
    }

    public double ReduccionRuido
    {
        get => _reduccionRuido;
        set => CambiarControl(ref _reduccionRuido, Math.Clamp(value, 0, 1));
    }

    public double Feminizacion
    {
        get => _feminizacion;
        set => CambiarControl(ref _feminizacion, Math.Clamp(value, 0, 1));
    }

    public double Distorsion
    {
        get => _distorsion;
        set => CambiarControl(ref _distorsion, Math.Clamp(value, 0, 1));
    }

    public double RobotMezcla
    {
        get => _robotMezcla;
        set => CambiarControl(ref _robotMezcla, Math.Clamp(value, 0, 1));
    }

    public double RobotFrecuenciaHz
    {
        get => _robotFrecuenciaHz;
        set => CambiarControl(ref _robotFrecuenciaHz, Math.Clamp(value, 20, 240));
    }

    public double FiltroTelefonico
    {
        get => _filtroTelefonico;
        set => CambiarControl(ref _filtroTelefonico, Math.Clamp(value, 0, 1));
    }

    public double TremoloMezcla
    {
        get => _tremoloMezcla;
        set => CambiarControl(ref _tremoloMezcla, Math.Clamp(value, 0, 1));
    }

    public double TremoloFrecuenciaHz
    {
        get => _tremoloFrecuenciaHz;
        set => CambiarControl(ref _tremoloFrecuenciaHz, Math.Clamp(value, 0.5, 20));
    }

    public double EcoMezcla
    {
        get => _ecoMezcla;
        set => CambiarControl(ref _ecoMezcla, Math.Clamp(value, 0, 0.75));
    }

    public double EcoMilisegundos
    {
        get => _ecoMilisegundos;
        set => CambiarControl(ref _ecoMilisegundos, Math.Clamp(value, 60, 600));
    }

    public double MezclaEfecto
    {
        get => _mezclaEfecto;
        set => CambiarControl(ref _mezclaEfecto, Math.Clamp(value, 0, 1));
    }

    public double VolumenFinal
    {
        get => _volumenFinal;
        set => CambiarControl(ref _volumenFinal, Math.Clamp(value, 0, 1.5));
    }

    public string AtajoSilencio
    {
        get => _atajoSilencio;
        set => Establecer(ref _atajoSilencio, value);
    }

    public string AtajoMotor
    {
        get => _atajoMotor;
        set => Establecer(ref _atajoMotor, value);
    }

    public string AtajoPerfilSiguiente
    {
        get => _atajoPerfilSiguiente;
        set => Establecer(ref _atajoPerfilSiguiente, value);
    }

    public string AtajoPerfilAnterior
    {
        get => _atajoPerfilAnterior;
        set => Establecer(ref _atajoPerfilAnterior, value);
    }

    public string AtajoDetenerSonidos
    {
        get => _atajoDetenerSonidos;
        set => Establecer(ref _atajoDetenerSonidos, value);
    }

    public async Task InicializarAsync()
    {
        if (_inicializado)
        {
            return;
        }

        _inicializado = true;
        try
        {
            _configuracion = await Task.Run(_servicioConfiguracion.Cargar);
            foreach (var perfil in CatalogoPerfiles.CrearPredeterminados())
            {
                Perfiles.Add(perfil);
            }

            foreach (var perfil in _configuracion.PerfilesPersonalizados)
            {
                perfil.EsPredeterminado = false;
                Perfiles.Add(perfil);
            }

            foreach (var sonido in _configuracion.Sonidos.Where(s => !string.IsNullOrWhiteSpace(s.Ruta)))
            {
                Sonidos.Add(sonido);
            }

            CargarAtajos();
            MonitoreoHabilitado = _configuracion.MonitoreoHabilitado;
            ActualizarDispositivos();
            PerfilSeleccionado = Perfiles.FirstOrDefault(p => p.Id == _configuracion.IdUltimoPerfil)
                ?? Perfiles.FirstOrDefault();
            EstadoTexto = "Listo para iniciar";
            AplicarAtajos();
            if (!string.IsNullOrWhiteSpace(_servicioConfiguracion.UltimaAdvertencia))
            {
                MensajeError = _servicioConfiguracion.UltimaAdvertencia;
            }

            _temporizadorNiveles.Start();
        }
        catch (Exception excepcion)
        {
            MensajeError = $"No se pudo preparar MyVoze: {excepcion.Message}";
            EstadoTexto = "Revisa la configuración";
        }
    }

    public void ConectarVentana(Window ventana)
    {
        _ventana = ventana;
    }

    public void Dispose()
    {
        if (_desechado)
        {
            return;
        }

        _temporizadorNiveles.Stop();
        try
        {
            GuardarConfiguracion();
        }
        catch
        {
            // La ventana ya se está cerrando; no se debe impedir la salida.
        }

        _motor.Dispose();
        _servicioAtajos.Dispose();
        _desechado = true;
    }

    private void ActualizarDispositivos()
    {
        var idEntrada = EntradaSeleccionada?.Id ?? _configuracion.IdDispositivoEntrada;
        var idSalida = SalidaSeleccionada?.Id ?? _configuracion.IdDispositivoSalida;
        var idMonitoreo = MonitoreoSeleccionado?.Id ?? _configuracion.IdDispositivoMonitoreo;

        try
        {
            var entradas = _servicioDispositivos.ObtenerEntradasFisicas();
            var salidasVirtuales = _servicioDispositivos.ObtenerSalidasVirtuales();
            var monitores = _servicioDispositivos.ObtenerSalidasMonitoreo();
            Entradas.Clear();
            SalidasVirtuales.Clear();
            Monitores.Clear();
            foreach (var entrada in entradas)
            {
                Entradas.Add(entrada);
            }

            foreach (var salida in salidasVirtuales)
            {
                SalidasVirtuales.Add(salida);
            }

            foreach (var monitor in monitores)
            {
                Monitores.Add(monitor);
            }

            EntradaSeleccionada = Entradas.FirstOrDefault(e => e.Id == idEntrada)
                ?? Entradas.FirstOrDefault();
            SalidaSeleccionada = SalidasVirtuales.FirstOrDefault(s => s.Id == idSalida)
                ?? SalidasVirtuales.FirstOrDefault();
            MonitoreoSeleccionado = Monitores.FirstOrDefault(s => s.Id == idMonitoreo)
                ?? Monitores.FirstOrDefault();

            MensajeError = Entradas.Count == 0
                ? "Windows no reportó ningún micrófono activo."
                : SalidasVirtuales.Count == 0
                    ? "No se encontró CABLE Input de VB-CABLE. Instala el controlador oficial, reinicia Windows y pulsa Actualizar dispositivos."
                    : MonitoreoHabilitado && Monitores.Count == 0
                        ? "No se encontró un dispositivo físico para monitoreo."
                    : string.Empty;
        }
        catch (Exception excepcion)
        {
            MensajeError = $"No fue posible leer los dispositivos de audio: {excepcion.Message}";
        }
    }

    private async Task IniciarDetenerAsync()
    {
        if (_motor.Estado == EstadoMotorAudio.Procesando)
        {
            Ocupado = true;
            try
            {
                await Task.Yield();
                _motor.Detener();
                EstadoTexto = "Motor detenido";
                NivelEntrada = 0;
                NivelSalida = 0;
            }
            finally
            {
                Ocupado = false;
                ActualizarTextosBotones();
            }

            return;
        }

        if (EntradaSeleccionada is null || SalidaSeleccionada is null)
        {
            MensajeError = EntradaSeleccionada is null
                ? "Selecciona un micrófono físico antes de iniciar."
                : "Selecciona CABLE Input como salida virtual. Si no aparece, instala VB-CABLE y reinicia Windows.";
            return;
        }

        if (PerfilSeleccionado is null)
        {
            MensajeError = "Selecciona un perfil de voz.";
            return;
        }

        Ocupado = true;
        MensajeError = string.Empty;
        EstadoTexto = "Iniciando motor de audio...";
        try
        {
            var entrada = EntradaSeleccionada.Id;
            var salida = SalidaSeleccionada.Id;
            var perfil = ConstruirPerfilActual();
            await Task.Yield();
            _motor.Iniciar(
                entrada,
                salida,
                perfil,
                MonitoreoSeleccionado?.Id,
                MonitoreoHabilitado);
            EstadoTexto = "Voz procesándose en tiempo real";
            GuardarConfiguracion();
        }
        catch (Exception excepcion)
        {
            MensajeError = excepcion.Message;
            EstadoTexto = "No se pudo iniciar";
        }
        finally
        {
            Ocupado = false;
            ActualizarTextosBotones();
        }
    }

    private void CambiarSilencio()
    {
        _motor.EstablecerSilencio(!_motor.Silenciado);
        ActualizarTextosBotones();
    }

    private void GuardarPerfilPersonalizado()
    {
        var nombre = string.IsNullOrWhiteSpace(NombreNuevoPerfil)
            ? $"Mi voz {Perfiles.Count(p => !p.EsPredeterminado) + 1}"
            : NombreNuevoPerfil.Trim();
        var perfil = ConstruirPerfilActual();
        perfil.Id = Guid.NewGuid().ToString("N");
        perfil.Nombre = nombre;
        perfil.EsPredeterminado = false;
        Perfiles.Add(perfil);
        PerfilSeleccionado = perfil;
        NombreNuevoPerfil = "Mi voz";
        GuardarConfiguracion();
    }

    private void EliminarPerfil()
    {
        if (PerfilSeleccionado is null || PerfilSeleccionado.EsPredeterminado)
        {
            return;
        }

        var eliminado = PerfilSeleccionado;
        PerfilSeleccionado = Perfiles.FirstOrDefault(p => p.EsPredeterminado);
        Perfiles.Remove(eliminado);
        GuardarConfiguracion();
        ComandoEliminarPerfil.Actualizar();
    }

    private bool PuedeEliminarPerfil()
        => PerfilSeleccionado is { EsPredeterminado: false };

    private void RestablecerPerfil()
    {
        var original = PerfilSeleccionado?.EsPredeterminado == true
            ? CatalogoPerfiles.CrearPredeterminados().FirstOrDefault(p => p.Id == PerfilSeleccionado.Id)
            : CatalogoPerfiles.CrearPredeterminados().First();

        if (original is not null)
        {
            CargarControles(original);
        }
    }

    private void ImportarSonidos()
    {
        foreach (var ruta in _servicioSeleccionArchivos.SeleccionarSonidos())
        {
            if (Sonidos.Any(s => string.Equals(s.Ruta, ruta, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            Sonidos.Add(new SonidoSoundboard
            {
                Nombre = Path.GetFileNameWithoutExtension(ruta),
                Ruta = ruta,
                Volumen = 0.8f
            });
        }

        SonidoSeleccionado ??= Sonidos.FirstOrDefault();
        GuardarConfiguracion();
    }

    private async Task ReproducirSonidoAsync()
    {
        if (SonidoSeleccionado is null)
        {
            return;
        }

        try
        {
            MensajeError = string.Empty;
            var ruta = SonidoSeleccionado.Ruta;
            var volumen = SonidoSeleccionado.Volumen;
            await Task.Run(() => _motor.ReproducirSonido(ruta, volumen));
        }
        catch (Exception excepcion)
        {
            MensajeError = $"No se pudo reproducir el sonido: {excepcion.Message}";
        }
    }

    private void ReproducirSonido(SonidoSoundboard sonido)
    {
        SonidoSeleccionado = sonido;
        ComandoReproducirSonido.Execute(null);
    }

    private void DetenerSonidos()
    {
        _motor.DetenerSonidos();
    }

    private void EliminarSonido()
    {
        if (SonidoSeleccionado is null)
        {
            return;
        }

        var indice = Sonidos.IndexOf(SonidoSeleccionado);
        Sonidos.Remove(SonidoSeleccionado);
        SonidoSeleccionado = Sonidos.ElementAtOrDefault(Math.Min(indice, Sonidos.Count - 1));
        AplicarAtajos();
        GuardarConfiguracion();
    }

    private void ConfigurarMonitoreo()
    {
        try
        {
            _motor.ConfigurarMonitoreo(MonitoreoSeleccionado?.Id, MonitoreoHabilitado);
            MensajeError = string.Empty;
        }
        catch (Exception excepcion)
        {
            MonitoreoHabilitado = false;
            MensajeError = $"No se pudo activar el monitoreo: {excepcion.Message}";
        }
    }

    private void CargarAtajos()
    {
        var atajos = _configuracion.Atajos.Count > 0
            ? _configuracion.Atajos
            : AtajoGlobal.CrearPredeterminados();
        AtajoSilencio = ObtenerCombinacion(atajos, AccionAtajo.SilenciarMicrofono, "Ctrl+Alt+M");
        AtajoMotor = ObtenerCombinacion(atajos, AccionAtajo.ActivarDesactivarMotor, "Ctrl+Alt+V");
        AtajoPerfilSiguiente = ObtenerCombinacion(atajos, AccionAtajo.PerfilSiguiente, "Ctrl+Alt+Right");
        AtajoPerfilAnterior = ObtenerCombinacion(atajos, AccionAtajo.PerfilAnterior, "Ctrl+Alt+Left");
        AtajoDetenerSonidos = ObtenerCombinacion(atajos, AccionAtajo.DetenerSonidos, "Ctrl+Alt+S");
    }

    private static string ObtenerCombinacion(
        IEnumerable<AtajoGlobal> atajos,
        AccionAtajo accion,
        string predeterminada)
        => atajos.FirstOrDefault(a => a.Accion == accion && string.IsNullOrWhiteSpace(a.IdSonido))
            ?.Combinacion ?? predeterminada;

    private void AplicarAtajos()
    {
        if (_ventana is null)
        {
            return;
        }

        var atajos = ConstruirAtajos();
        var errores = _servicioAtajos.Registrar(_ventana, atajos, EjecutarAtajo);
        if (errores.Count > 0)
        {
            MensajeError = string.Join(Environment.NewLine, errores);
            return;
        }

        _configuracion.Atajos = atajos;
        MensajeError = string.Empty;
        GuardarConfiguracion();
    }

    private List<AtajoGlobal> ConstruirAtajos()
    {
        var atajos = new List<AtajoGlobal>
        {
            new() { Id = "silencio", Accion = AccionAtajo.SilenciarMicrofono, Combinacion = AtajoSilencio },
            new() { Id = "motor", Accion = AccionAtajo.ActivarDesactivarMotor, Combinacion = AtajoMotor },
            new() { Id = "perfil-siguiente", Accion = AccionAtajo.PerfilSiguiente, Combinacion = AtajoPerfilSiguiente },
            new() { Id = "perfil-anterior", Accion = AccionAtajo.PerfilAnterior, Combinacion = AtajoPerfilAnterior },
            new() { Id = "detener-sonidos", Accion = AccionAtajo.DetenerSonidos, Combinacion = AtajoDetenerSonidos }
        };

        atajos.AddRange(Sonidos
            .Where(s => !string.IsNullOrWhiteSpace(s.Atajo))
            .Select(s => new AtajoGlobal
            {
                Id = $"sonido-{s.Id}",
                Accion = AccionAtajo.ReproducirSonido,
                Combinacion = s.Atajo!,
                IdSonido = s.Id
            }));
        return atajos;
    }

    private void EjecutarAtajo(AtajoGlobal atajo)
    {
        switch (atajo.Accion)
        {
            case AccionAtajo.SilenciarMicrofono:
                CambiarSilencio();
                break;
            case AccionAtajo.ActivarDesactivarMotor:
                ComandoIniciarDetener.Execute(null);
                break;
            case AccionAtajo.PerfilSiguiente:
                CambiarPerfil(1);
                break;
            case AccionAtajo.PerfilAnterior:
                CambiarPerfil(-1);
                break;
            case AccionAtajo.DetenerSonidos:
                DetenerSonidos();
                break;
            case AccionAtajo.ReproducirSonido:
                var sonido = Sonidos.FirstOrDefault(s => s.Id == atajo.IdSonido);
                if (sonido is not null)
                {
                    ReproducirSonido(sonido);
                }
                break;
        }
    }

    private void CambiarPerfil(int desplazamiento)
    {
        if (Perfiles.Count == 0)
        {
            return;
        }

        var indice = PerfilSeleccionado is null ? 0 : Perfiles.IndexOf(PerfilSeleccionado);
        indice = (indice + desplazamiento + Perfiles.Count) % Perfiles.Count;
        PerfilSeleccionado = Perfiles[indice];
    }

    private void CargarControles(PerfilVoz perfil)
    {
        _actualizandoControles = true;
        Pitch = perfil.Pitch;
        GananciaDb = perfil.GananciaDb;
        GravesDb = perfil.GravesDb;
        MediosDb = perfil.MediosDb;
        AgudosDb = perfil.AgudosDb;
        UmbralRuidoDb = perfil.UmbralRuidoDb;
        ReduccionRuido = perfil.ReduccionRuido;
        Feminizacion = perfil.Feminizacion;
        Distorsion = perfil.Distorsion;
        RobotMezcla = perfil.RobotMezcla;
        RobotFrecuenciaHz = perfil.RobotFrecuenciaHz;
        FiltroTelefonico = perfil.FiltroTelefonico;
        TremoloMezcla = perfil.TremoloMezcla;
        TremoloFrecuenciaHz = perfil.TremoloFrecuenciaHz;
        EcoMezcla = perfil.EcoMezcla;
        EcoMilisegundos = perfil.EcoMilisegundos;
        MezclaEfecto = perfil.MezclaEfecto;
        VolumenFinal = perfil.VolumenFinal;
        _actualizandoControles = false;
        _motor.ActualizarPerfil(ConstruirPerfilActual());
    }

    private void CambiarControl(ref double campo, double valor, [System.Runtime.CompilerServices.CallerMemberName] string? propiedad = null)
    {
        if (!Establecer(ref campo, valor, propiedad) || _actualizandoControles)
        {
            return;
        }

        _motor.ActualizarPerfil(ConstruirPerfilActual());
    }

    private PerfilVoz ConstruirPerfilActual()
    {
        return new PerfilVoz
        {
            Id = PerfilSeleccionado?.Id ?? Guid.NewGuid().ToString("N"),
            Nombre = PerfilSeleccionado?.Nombre ?? "Personalizado",
            Categoria = PerfilSeleccionado?.Categoria ?? "General",
            EsPredeterminado = PerfilSeleccionado?.EsPredeterminado ?? false,
            Pitch = (float)Pitch,
            GananciaDb = (float)GananciaDb,
            GravesDb = (float)GravesDb,
            MediosDb = (float)MediosDb,
            AgudosDb = (float)AgudosDb,
            UmbralRuidoDb = (float)UmbralRuidoDb,
            ReduccionRuido = (float)ReduccionRuido,
            Feminizacion = (float)Feminizacion,
            Distorsion = (float)Distorsion,
            RobotMezcla = (float)RobotMezcla,
            RobotFrecuenciaHz = (float)RobotFrecuenciaHz,
            FiltroTelefonico = (float)FiltroTelefonico,
            TremoloMezcla = (float)TremoloMezcla,
            TremoloFrecuenciaHz = (float)TremoloFrecuenciaHz,
            EcoMezcla = (float)EcoMezcla,
            EcoMilisegundos = (int)EcoMilisegundos,
            MezclaEfecto = (float)MezclaEfecto,
            VolumenFinal = (float)VolumenFinal
        };
    }

    private void ActualizarIndicadores(object? sender, EventArgs e)
    {
        NivelEntrada = Math.Clamp(_motor.NivelEntrada, 0, 1);
        NivelSalida = Math.Clamp(_motor.NivelSalida, 0, 1);
        Notificar(nameof(EstaSaturando));
        if (!string.IsNullOrWhiteSpace(_motor.UltimoError)
            && _motor.UltimoError != MensajeError)
        {
            MensajeError = _motor.UltimoError;
        }

        if (_motor.Estado == EstadoMotorAudio.Error)
        {
            EstadoTexto = "Dispositivo desconectado";
        }

        ActualizarTextosBotones();
    }

    private void ActualizarTextosBotones()
    {
        Notificar(nameof(TextoBotonPrincipal));
        Notificar(nameof(TextoBotonSilencio));
    }

    private void GuardarConfiguracion()
    {
        try
        {
            GuardarConfiguracionInterna();
        }
        catch (Exception excepcion)
        {
            MensajeError = $"No se pudo guardar la configuracion: {excepcion.Message}";
        }
    }

    private void GuardarConfiguracionInterna()
    {
        if (PerfilSeleccionado is { EsPredeterminado: false })
        {
            CopiarPerfil(ConstruirPerfilActual(), PerfilSeleccionado);
        }

        _configuracion.IdDispositivoEntrada = EntradaSeleccionada?.Id;
        _configuracion.IdDispositivoSalida = SalidaSeleccionada?.Id;
        _configuracion.IdDispositivoMonitoreo = MonitoreoSeleccionado?.Id;
        _configuracion.MonitoreoHabilitado = MonitoreoHabilitado;
        _configuracion.IdUltimoPerfil = PerfilSeleccionado?.Id;
        _configuracion.PerfilesPersonalizados = Perfiles
            .Where(p => !p.EsPredeterminado)
            .Select(p => p.Clonar())
            .ToList();
        _configuracion.Sonidos = Sonidos.Select(s => s.Clonar()).ToList();
        _configuracion.Atajos = ConstruirAtajos();
        _servicioConfiguracion.Guardar(_configuracion);
    }

    private static void CopiarPerfil(PerfilVoz origen, PerfilVoz destino)
    {
        destino.Nombre = origen.Nombre;
        destino.Categoria = origen.Categoria;
        destino.Pitch = origen.Pitch;
        destino.GananciaDb = origen.GananciaDb;
        destino.GravesDb = origen.GravesDb;
        destino.MediosDb = origen.MediosDb;
        destino.AgudosDb = origen.AgudosDb;
        destino.UmbralRuidoDb = origen.UmbralRuidoDb;
        destino.ReduccionRuido = origen.ReduccionRuido;
        destino.Feminizacion = origen.Feminizacion;
        destino.Distorsion = origen.Distorsion;
        destino.RobotMezcla = origen.RobotMezcla;
        destino.RobotFrecuenciaHz = origen.RobotFrecuenciaHz;
        destino.FiltroTelefonico = origen.FiltroTelefonico;
        destino.TremoloMezcla = origen.TremoloMezcla;
        destino.TremoloFrecuenciaHz = origen.TremoloFrecuenciaHz;
        destino.EcoMezcla = origen.EcoMezcla;
        destino.EcoMilisegundos = origen.EcoMilisegundos;
        destino.MezclaEfecto = origen.MezclaEfecto;
        destino.VolumenFinal = origen.VolumenFinal;
    }

}
