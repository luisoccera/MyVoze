namespace VoxMorph.Audio.Procesamiento;

internal sealed class ReductorRuidoAdaptativo
{
    private readonly float _coeficienteAtaqueEnvolvente;
    private readonly float _coeficienteLiberacionEnvolvente;
    private readonly float _coeficienteAprendizajeRuido;
    private readonly float _coeficienteApertura;
    private readonly float _coeficienteCierre;
    private float _envolvente;
    private float _nivelRuido;
    private float _ganancia;

    public ReductorRuidoAdaptativo(int frecuenciaMuestreo)
    {
        if (frecuenciaMuestreo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frecuenciaMuestreo));
        }

        _coeficienteAtaqueEnvolvente = CrearCoeficiente(frecuenciaMuestreo, 3);
        _coeficienteLiberacionEnvolvente = CrearCoeficiente(frecuenciaMuestreo, 90);
        _coeficienteAprendizajeRuido = CrearCoeficiente(frecuenciaMuestreo, 1200);
        _coeficienteApertura = CrearCoeficiente(frecuenciaMuestreo, 4);
        _coeficienteCierre = CrearCoeficiente(frecuenciaMuestreo, 180);
    }

    public float Procesar(float muestra, float umbralDb, float intensidad)
    {
        intensidad = Math.Clamp(intensidad, 0, 1);
        if (intensidad <= 0)
        {
            _ganancia = 1;
            return muestra;
        }

        var magnitud = MathF.Abs(muestra);
        var coeficienteEnvolvente = magnitud > _envolvente
            ? _coeficienteAtaqueEnvolvente
            : _coeficienteLiberacionEnvolvente;
        _envolvente += (magnitud - _envolvente) * coeficienteEnvolvente;

        var umbralBase = DbALineal(Math.Clamp(umbralDb, -90, -12));
        if (_envolvente <= umbralBase * 1.8f)
        {
            _nivelRuido += (_envolvente - _nivelRuido) * _coeficienteAprendizajeRuido;
        }

        var umbralAdaptativo = MathF.Max(umbralBase, _nivelRuido * 2.6f);
        var inicioApertura = umbralAdaptativo * 0.72f;
        var aperturaCompleta = umbralAdaptativo * 2.2f;
        var posicion = Math.Clamp(
            (_envolvente - inicioApertura) / MathF.Max(aperturaCompleta - inicioApertura, 0.000001f),
            0,
            1);
        var transicionSuave = posicion * posicion * (3 - 2 * posicion);
        var gananciaMinima = DbALineal(-42 * intensidad);
        var gananciaObjetivo = gananciaMinima + (1 - gananciaMinima) * transicionSuave;
        var coeficienteGanancia = gananciaObjetivo > _ganancia
            ? _coeficienteApertura
            : _coeficienteCierre;
        _ganancia += (gananciaObjetivo - _ganancia) * coeficienteGanancia;

        return muestra * _ganancia;
    }

    private static float CrearCoeficiente(int frecuenciaMuestreo, float milisegundos)
        => 1 - MathF.Exp(-1 / (frecuenciaMuestreo * milisegundos / 1000));

    private static float DbALineal(float db)
        => MathF.Pow(10, db / 20);
}
