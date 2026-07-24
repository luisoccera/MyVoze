using NAudio.Dsp;
using NAudio.Wave;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Procesamiento;

public sealed class ProcesadorVozSampleProvider : ISampleProvider
{
    private const float LimiteSalida = 0.95f;
    private readonly ISampleProvider _origen;
    private readonly float[] _bufferEco;
    private readonly ReductorRuidoAdaptativo _reductorRuido;
    private PerfilVoz _perfil;
    private BiQuadFilter _filtroPasaAltas;
    private BiQuadFilter _filtroGraves;
    private BiQuadFilter _filtroMedios;
    private BiQuadFilter _filtroAgudos;
    private BiQuadFilter _filtroFemeninoGraves;
    private BiQuadFilter _filtroFemeninoCuerpo;
    private BiQuadFilter _filtroFemeninoPresencia;
    private BiQuadFilter _filtroFemeninoAire;
    private BiQuadFilter _filtroSibilancia;
    private BiQuadFilter _filtroTelefonoPasaAltas;
    private BiQuadFilter _filtroTelefonoPasaBajas;
    private int _indiceEco;
    private int _versionPerfil;
    private int _versionAplicada = -1;
    private float _faseRobot;
    private float _faseTremolo;
    private float _envolventeSibilancia;
    private float _nivelEntrada;
    private float _nivelSalida;
    private volatile bool _silenciado;

    public ProcesadorVozSampleProvider(ISampleProvider origen, PerfilVoz perfil)
    {
        ArgumentNullException.ThrowIfNull(origen);
        ArgumentNullException.ThrowIfNull(perfil);

        if (origen.WaveFormat.Channels != 1)
        {
            throw new ArgumentException("El procesador de voz requiere audio mono.", nameof(origen));
        }

        _origen = origen;
        _perfil = perfil.Clonar();
        _bufferEco = new float[Math.Max(origen.WaveFormat.SampleRate * 2, 1)];
        _reductorRuido = new ReductorRuidoAdaptativo(origen.WaveFormat.SampleRate);
        _filtroPasaAltas = CrearFiltroPasaAltas();
        _filtroGraves = CrearFiltroGraves(_perfil.GravesDb);
        _filtroMedios = CrearFiltroMedios(_perfil.MediosDb);
        _filtroAgudos = CrearFiltroAgudos(_perfil.AgudosDb);
        _filtroFemeninoGraves = CrearFiltroFemeninoGraves(_perfil.Feminizacion);
        _filtroFemeninoCuerpo = CrearFiltroFemeninoCuerpo(_perfil.Feminizacion);
        _filtroFemeninoPresencia = CrearFiltroFemeninoPresencia(_perfil.Feminizacion);
        _filtroFemeninoAire = CrearFiltroFemeninoAire(_perfil.Feminizacion);
        _filtroSibilancia = CrearFiltroSibilancia();
        _filtroTelefonoPasaAltas = CrearFiltroTelefonoPasaAltas();
        _filtroTelefonoPasaBajas = CrearFiltroTelefonoPasaBajas();
    }

    public WaveFormat WaveFormat => _origen.WaveFormat;
    public float NivelEntrada => Volatile.Read(ref _nivelEntrada);
    public float NivelSalida => Volatile.Read(ref _nivelSalida);

    public void ActualizarPerfil(PerfilVoz perfil)
    {
        ArgumentNullException.ThrowIfNull(perfil);
        Interlocked.Exchange(ref _perfil, perfil.Clonar());
        Interlocked.Increment(ref _versionPerfil);
    }

    public void EstablecerSilencio(bool silenciado) => _silenciado = silenciado;

    public int Read(float[] buffer, int offset, int count)
    {
        var leidos = _origen.Read(buffer, offset, count);
        if (leidos == 0)
        {
            Volatile.Write(ref _nivelEntrada, 0);
            Volatile.Write(ref _nivelSalida, 0);
            return 0;
        }

        var versionActual = Volatile.Read(ref _versionPerfil);
        var perfil = Volatile.Read(ref _perfil);
        if (versionActual != _versionAplicada)
        {
            _filtroPasaAltas = CrearFiltroPasaAltas();
            _filtroGraves = CrearFiltroGraves(perfil.GravesDb);
            _filtroMedios = CrearFiltroMedios(perfil.MediosDb);
            _filtroAgudos = CrearFiltroAgudos(perfil.AgudosDb);
            _filtroFemeninoGraves = CrearFiltroFemeninoGraves(perfil.Feminizacion);
            _filtroFemeninoCuerpo = CrearFiltroFemeninoCuerpo(perfil.Feminizacion);
            _filtroFemeninoPresencia = CrearFiltroFemeninoPresencia(perfil.Feminizacion);
            _filtroFemeninoAire = CrearFiltroFemeninoAire(perfil.Feminizacion);
            _filtroSibilancia = CrearFiltroSibilancia();
            _envolventeSibilancia = 0;
            _filtroTelefonoPasaAltas = CrearFiltroTelefonoPasaAltas();
            _filtroTelefonoPasaBajas = CrearFiltroTelefonoPasaBajas();
            _versionAplicada = versionActual;
        }

        var sumaEntrada = 0.0;
        var sumaSalida = 0.0;
        var ganancia = DbALineal(perfil.GananciaDb);
        var mezcla = Math.Clamp(perfil.MezclaEfecto, 0, 1);
        var mezclaRobot = Math.Clamp(perfil.RobotMezcla, 0, 1);
        var filtroTelefonico = Math.Clamp(perfil.FiltroTelefonico, 0, 1);
        var mezclaTremolo = Math.Clamp(perfil.TremoloMezcla, 0, 1);
        var feminizacion = Math.Clamp(perfil.Feminizacion, 0, 1);
        var mezclaDistorsion = Math.Clamp(perfil.Distorsion, 0, 1);
        var mezclaEco = Math.Clamp(perfil.EcoMezcla, 0, 0.75f);
        var volumenFinal = Math.Clamp(perfil.VolumenFinal, 0, 1.5f);
        var demoraMuestras = Math.Clamp(
            WaveFormat.SampleRate * perfil.EcoMilisegundos / 1000,
            1,
            _bufferEco.Length - 1);
        var frecuenciaRobot = Math.Clamp(perfil.RobotFrecuenciaHz, 20, 240);
        var frecuenciaTremolo = Math.Clamp(perfil.TremoloFrecuenciaHz, 0.5f, 20);
        var incrementoRobot = 2.0f * MathF.PI * frecuenciaRobot / WaveFormat.SampleRate;
        var incrementoTremolo = 2.0f * MathF.PI * frecuenciaTremolo / WaveFormat.SampleRate;

        for (var i = offset; i < offset + leidos; i++)
        {
            var original = buffer[i];
            sumaEntrada += original * original;

            var originalLimpia = _reductorRuido.Procesar(
                original,
                perfil.UmbralRuidoDb,
                perfil.ReduccionRuido * 0.30f);
            var procesada = originalLimpia;
            procesada *= ganancia;
            procesada = _filtroPasaAltas.Transform(procesada);
            procesada = _filtroGraves.Transform(procesada);
            procesada = _filtroMedios.Transform(procesada);
            procesada = _filtroAgudos.Transform(procesada);
            procesada = _filtroFemeninoGraves.Transform(procesada);
            procesada = _filtroFemeninoCuerpo.Transform(procesada);
            procesada = _filtroFemeninoPresencia.Transform(procesada);
            procesada = _filtroFemeninoAire.Transform(procesada);
            if (feminizacion > 0)
            {
                procesada = AplicarDeEsser(procesada, feminizacion);
            }

            if (filtroTelefonico > 0)
            {
                var telefonica = _filtroTelefonoPasaAltas.Transform(procesada);
                telefonica = _filtroTelefonoPasaBajas.Transform(telefonica);
                procesada = Mezclar(procesada, telefonica, filtroTelefonico);
            }

            if (mezclaDistorsion > 0)
            {
                var impulso = 1.0f + mezclaDistorsion * 12.0f;
                var distorsionada = MathF.Tanh(procesada * impulso) / MathF.Tanh(impulso);
                procesada = Mezclar(procesada, distorsionada, mezclaDistorsion);
            }

            if (mezclaRobot > 0)
            {
                var modulada = procesada * MathF.Sin(_faseRobot);
                procesada = Mezclar(procesada, modulada, mezclaRobot);
                _faseRobot += incrementoRobot;
                if (_faseRobot >= 2.0f * MathF.PI)
                {
                    _faseRobot -= 2.0f * MathF.PI;
                }
            }

            if (mezclaTremolo > 0)
            {
                var modulador = 0.5f + 0.5f * MathF.Sin(_faseTremolo);
                procesada = Mezclar(procesada, procesada * modulador, mezclaTremolo);
                _faseTremolo += incrementoTremolo;
                if (_faseTremolo >= 2.0f * MathF.PI)
                {
                    _faseTremolo -= 2.0f * MathF.PI;
                }
            }

            if (mezclaEco > 0)
            {
                var indiceDemorado = _indiceEco - demoraMuestras;
                if (indiceDemorado < 0)
                {
                    indiceDemorado += _bufferEco.Length;
                }

                var eco = _bufferEco[indiceDemorado];
                _bufferEco[_indiceEco] = procesada + eco * 0.28f;
                _indiceEco = (_indiceEco + 1) % _bufferEco.Length;
                procesada = Mezclar(procesada, eco, mezclaEco);
            }

            procesada = Comprimir(procesada);
            var salida = Mezclar(originalLimpia, procesada, mezcla);
            salida *= volumenFinal;
            salida = _silenciado ? 0 : Math.Clamp(salida, -LimiteSalida, LimiteSalida);
            buffer[i] = salida;
            sumaSalida += salida * salida;
        }

        Volatile.Write(ref _nivelEntrada, (float)Math.Sqrt(sumaEntrada / leidos));
        Volatile.Write(ref _nivelSalida, (float)Math.Sqrt(sumaSalida / leidos));
        return leidos;
    }

    private BiQuadFilter CrearFiltroPasaAltas()
        => BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, 75, 0.707f);

    private BiQuadFilter CrearFiltroGraves(float gananciaDb)
        => BiQuadFilter.LowShelf(WaveFormat.SampleRate, 140, 0.75f, gananciaDb);

    private BiQuadFilter CrearFiltroMedios(float gananciaDb)
        => BiQuadFilter.PeakingEQ(WaveFormat.SampleRate, 1200, 0.9f, gananciaDb);

    private BiQuadFilter CrearFiltroAgudos(float gananciaDb)
        => BiQuadFilter.HighShelf(WaveFormat.SampleRate, 4200, 0.75f, gananciaDb);

    private BiQuadFilter CrearFiltroTelefonoPasaAltas()
        => BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, 300, 0.707f);

    private BiQuadFilter CrearFiltroTelefonoPasaBajas()
        => BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, 3400, 0.707f);

    private BiQuadFilter CrearFiltroFemeninoGraves(float feminizacion)
        => BiQuadFilter.LowShelf(
            WaveFormat.SampleRate,
            190,
            0.75f,
            -8.0f * Math.Clamp(feminizacion, 0, 1));

    private BiQuadFilter CrearFiltroFemeninoCuerpo(float feminizacion)
        => BiQuadFilter.PeakingEQ(
            WaveFormat.SampleRate,
            420,
            0.9f,
            -2.8f * Math.Clamp(feminizacion, 0, 1));

    private BiQuadFilter CrearFiltroFemeninoPresencia(float feminizacion)
        => BiQuadFilter.PeakingEQ(
            WaveFormat.SampleRate,
            2400,
            0.85f,
            3.6f * Math.Clamp(feminizacion, 0, 1));

    private BiQuadFilter CrearFiltroFemeninoAire(float feminizacion)
        => BiQuadFilter.HighShelf(
            WaveFormat.SampleRate,
            6200,
            0.75f,
            1.8f * Math.Clamp(feminizacion, 0, 1));

    private BiQuadFilter CrearFiltroSibilancia()
        => BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, 5600, 0.707f);

    private float AplicarDeEsser(float muestra, float feminizacion)
    {
        var sibilancia = _filtroSibilancia.Transform(muestra);
        var magnitud = MathF.Abs(sibilancia);
        var coeficiente = magnitud > _envolventeSibilancia ? 0.16f : 0.0035f;
        _envolventeSibilancia += (magnitud - _envolventeSibilancia) * coeficiente;
        var exceso = Math.Clamp((_envolventeSibilancia - 0.012f) / 0.05f, 0, 1);
        return muestra - sibilancia * exceso * feminizacion * 0.42f;
    }

    private static float DbALineal(float db) => MathF.Pow(10, db / 20.0f);

    private static float Mezclar(float seco, float procesado, float cantidad)
        => seco * (1.0f - cantidad) + procesado * cantidad;

    private static float Comprimir(float muestra)
    {
        const float umbral = 0.55f;
        const float relacion = 4.0f;
        var magnitud = MathF.Abs(muestra);
        if (magnitud <= umbral)
        {
            return muestra;
        }

        var comprimida = umbral + (magnitud - umbral) / relacion;
        return MathF.CopySign(comprimida, muestra);
    }
}
