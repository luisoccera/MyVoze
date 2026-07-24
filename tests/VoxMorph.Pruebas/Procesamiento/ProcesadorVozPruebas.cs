using VoxMorph.Audio.Procesamiento;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Pruebas.Procesamiento;

public sealed class ProcesadorVozPruebas
{
    [Fact]
    public void Limitador_NoPermiteMuestrasMayoresAlLimite()
    {
        var origen = new ProveedorMuestrasPrueba(4.0f, -4.0f, 2.0f, -2.0f);
        var perfil = PerfilNeutro();
        perfil.GananciaDb = 12;
        var procesador = new ProcesadorVozSampleProvider(origen, perfil);
        var salida = new float[4];

        var leidos = procesador.Read(salida, 0, salida.Length);

        Assert.Equal(4, leidos);
        Assert.All(salida, muestra => Assert.InRange(muestra, -0.95f, 0.95f));
    }

    [Fact]
    public void Silencio_EntregaSoloCeros()
    {
        var origen = new ProveedorMuestrasPrueba(0.5f, -0.5f, 0.25f);
        var procesador = new ProcesadorVozSampleProvider(origen, PerfilNeutro());
        procesador.EstablecerSilencio(true);
        var salida = new float[3];

        procesador.Read(salida, 0, salida.Length);

        Assert.All(salida, muestra => Assert.Equal(0f, muestra));
        Assert.Equal(0f, procesador.NivelSalida);
    }

    [Fact]
    public void PuertaRuido_AtenuaSinCortarBruscamenteUnaSenalBaja()
    {
        var origen = new ProveedorMuestrasPrueba(
            Enumerable.Range(0, 4096)
                .Select(indice => 0.0001f * MathF.Sin(2 * MathF.PI * 1000 * indice / 48_000))
                .ToArray());
        var perfil = PerfilNeutro();
        perfil.UmbralRuidoDb = -40;
        perfil.ReduccionRuido = 1;
        perfil.MezclaEfecto = 1;
        var procesador = new ProcesadorVozSampleProvider(origen, perfil);
        var salida = new float[4096];

        procesador.Read(salida, 0, salida.Length);

        var rmsEntrada = Rms(Enumerable.Range(2048, 1800)
            .Select(indice => 0.0001f * MathF.Sin(2 * MathF.PI * 1000 * indice / 48_000)));
        var rmsSalida = Rms(salida.Skip(2048).Take(1800));
        Assert.InRange(rmsSalida / rmsEntrada, 0.15, 0.45);
    }

    [Fact]
    public void ReduccionRuido_AtenuaFondoYPreservaVoz()
    {
        var ruido = Enumerable.Range(0, 4800)
            .Select(indice => 0.0025f * MathF.Sin(2 * MathF.PI * 900 * indice / 48_000));
        var voz = Enumerable.Range(0, 4800)
            .Select(indice => 0.08f * MathF.Sin(2 * MathF.PI * 220 * indice / 48_000));
        var muestras = ruido.Concat(voz).ToArray();
        var perfilReferencia = PerfilNeutro();
        var perfilFiltrado = PerfilNeutro();
        perfilFiltrado.UmbralRuidoDb = -46;
        perfilFiltrado.ReduccionRuido = 0.9f;
        var salidaReferencia = new float[muestras.Length];
        var salidaFiltrada = new float[muestras.Length];

        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilReferencia).Read(salidaReferencia, 0, salidaReferencia.Length);
        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilFiltrado).Read(salidaFiltrada, 0, salidaFiltrada.Length);

        var rmsRuidoReferencia = Rms(salidaReferencia.Skip(2400).Take(2000));
        var rmsRuidoFiltrado = Rms(salidaFiltrada.Skip(2400).Take(2000));
        var rmsVozReferencia = Rms(salidaReferencia.Skip(7200).Take(2000));
        var rmsVozFiltrada = Rms(salidaFiltrada.Skip(7200).Take(2000));
        Assert.True(rmsRuidoFiltrado < rmsRuidoReferencia * 0.35);
        Assert.True(rmsVozFiltrada > rmsVozReferencia * 0.75);
    }

    [Fact]
    public void Ganancia_AumentaElNivelDeUnaSenal()
    {
        var muestras = Enumerable.Range(0, 2048)
            .Select(indice => 0.05f * MathF.Sin(2 * MathF.PI * 1000 * indice / 48_000))
            .ToArray();
        var perfilNeutro = PerfilNeutro();
        var perfilGanancia = PerfilNeutro();
        perfilGanancia.GananciaDb = 6;
        var procesadorNeutro = new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilNeutro);
        var procesadorGanancia = new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilGanancia);
        var salidaNeutra = new float[muestras.Length];
        var salidaGanancia = new float[muestras.Length];

        procesadorNeutro.Read(salidaNeutra, 0, salidaNeutra.Length);
        procesadorGanancia.Read(salidaGanancia, 0, salidaGanancia.Length);

        var rmsNeutro = Math.Sqrt(salidaNeutra.Skip(128).Average(valor => valor * valor));
        var rmsGanancia = Math.Sqrt(salidaGanancia.Skip(128).Average(valor => valor * valor));
        Assert.True(rmsGanancia > rmsNeutro * 1.8);
    }

    [Fact]
    public void Feminizacion_ReduceCuerpoGraveYAumentaPresencia()
    {
        var graves = Enumerable.Range(0, 8192)
            .Select(indice => 0.08f * MathF.Sin(2 * MathF.PI * 170 * indice / 48_000))
            .ToArray();
        var presencia = Enumerable.Range(0, 8192)
            .Select(indice => 0.04f * MathF.Sin(2 * MathF.PI * 2400 * indice / 48_000))
            .ToArray();
        var perfilNormal = PerfilNeutro();
        var perfilFemenino = PerfilNeutro();
        perfilFemenino.Feminizacion = 1;

        var gravesNormales = Procesar(graves, perfilNormal);
        var gravesFemeninos = Procesar(graves, perfilFemenino);
        var presenciaNormal = Procesar(presencia, perfilNormal);
        var presenciaFemenina = Procesar(presencia, perfilFemenino);

        var relacionGraves = Rms(gravesFemeninos.Skip(1024)) / Rms(gravesNormales.Skip(1024));
        var relacionPresencia = Rms(presenciaFemenina.Skip(1024)) / Rms(presenciaNormal.Skip(1024));
        Assert.True(relacionGraves < 0.55, $"Relación de graves: {relacionGraves:F3}");
        Assert.True(relacionPresencia > 1.25, $"Relación de presencia: {relacionPresencia:F3}");
    }

    [Fact]
    public void MezclaSeca_CeroConservaLaMuestraOriginal()
    {
        var perfil = PerfilNeutro();
        perfil.MezclaEfecto = 0;
        perfil.Distorsion = 1;
        var procesador = new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(0.2f, -0.3f),
            perfil);
        var salida = new float[2];

        procesador.Read(salida, 0, salida.Length);

        Assert.Equal(0.2f, salida[0], 5);
        Assert.Equal(-0.3f, salida[1], 5);
    }

    [Fact]
    public void Robot_ModificaUnaSenalConstante()
    {
        var perfil = PerfilNeutro();
        perfil.RobotMezcla = 1;
        var procesador = new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(Enumerable.Repeat(0.4f, 256).ToArray()),
            perfil);
        var salida = new float[256];

        procesador.Read(salida, 0, salida.Length);

        Assert.True(salida.Distinct().Count() > 20);
    }

    [Fact]
    public void FiltroTelefonico_AtenuaFrecuenciasAltas()
    {
        var muestras = Enumerable.Range(0, 8192)
            .Select(indice => 0.2f * MathF.Sin(2 * MathF.PI * 9000 * indice / 48_000))
            .ToArray();
        var perfilNormal = PerfilNeutro();
        var perfilTelefonico = PerfilNeutro();
        perfilTelefonico.FiltroTelefonico = 1;
        var salidaNormal = new float[muestras.Length];
        var salidaTelefonica = new float[muestras.Length];

        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilNormal).Read(salidaNormal, 0, salidaNormal.Length);
        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilTelefonico).Read(salidaTelefonica, 0, salidaTelefonica.Length);

        Assert.True(Rms(salidaTelefonica.Skip(1024)) < Rms(salidaNormal.Skip(1024)) * 0.2);
    }

    [Fact]
    public void Tremolo_ModulaElNivelDeLaVoz()
    {
        var muestras = Enumerable.Range(0, 9600)
            .Select(indice => 0.2f * MathF.Sin(2 * MathF.PI * 440 * indice / 48_000))
            .ToArray();
        var perfilNormal = PerfilNeutro();
        var perfilTremolo = PerfilNeutro();
        perfilTremolo.TremoloMezcla = 1;
        perfilTremolo.TremoloFrecuenciaHz = 5;
        var salidaNormal = new float[muestras.Length];
        var salidaTremolo = new float[muestras.Length];

        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilNormal).Read(salidaNormal, 0, salidaNormal.Length);
        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfilTremolo).Read(salidaTremolo, 0, salidaTremolo.Length);

        var nivelNormal = Rms(salidaNormal.Skip(1024));
        var nivelTremolo = Rms(salidaTremolo.Skip(1024));
        Assert.InRange(nivelTremolo / nivelNormal, 0.45, 0.75);
    }

    [Fact]
    public void Eco_ProduceColaDespuesDelImpulso()
    {
        var muestras = new float[4000];
        muestras[0] = 0.5f;
        var perfil = PerfilNeutro();
        perfil.EcoMezcla = 0.6f;
        perfil.EcoMilisegundos = 60;
        var procesador = new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfil);
        var salida = new float[muestras.Length];

        procesador.Read(salida, 0, salida.Length);

        Assert.True(MathF.Abs(salida[2880]) > 0.05f);
    }

    private static PerfilVoz PerfilNeutro()
    {
        return new PerfilVoz
        {
            Nombre = "Prueba",
            Pitch = 1,
            GananciaDb = 0,
            GravesDb = 0,
            AgudosDb = 0,
            UmbralRuidoDb = -70,
            ReduccionRuido = 0,
            MezclaEfecto = 1
        };
    }

    private static float[] Procesar(float[] muestras, PerfilVoz perfil)
    {
        var salida = new float[muestras.Length];
        new ProcesadorVozSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            perfil).Read(salida, 0, salida.Length);
        return salida;
    }

    private static double Rms(IEnumerable<float> muestras)
        => Math.Sqrt(muestras.Average(valor => valor * valor));
}
