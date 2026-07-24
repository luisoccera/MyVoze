using VoxMorph.Audio.Procesamiento;

namespace VoxMorph.Pruebas.Procesamiento;

public sealed class ReductorRuidoNeuronalPruebas
{
    [Fact]
    public void IntensidadCero_ConservaLasMuestras()
    {
        var muestras = Enumerable.Range(0, 1440)
            .Select(indice => 0.1f * MathF.Sin(2 * MathF.PI * 440 * indice / 48_000))
            .ToArray();
        using var reductor = new ReductorRuidoNeuronalSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            0);
        var salida = new float[muestras.Length];

        var leidos = reductor.Read(salida, 0, salida.Length);

        Assert.Equal(muestras.Length, leidos);
        Assert.Equal(muestras, salida);
    }

    [Fact]
    public void LimpiezaIa_ProcesaAudioConElModeloIntegrado()
    {
        var aleatorio = new Random(73);
        var muestras = Enumerable.Range(0, 48_000)
            .Select(indice =>
                0.07f * MathF.Sin(2 * MathF.PI * 210 * indice / 48_000)
                + (float)(aleatorio.NextDouble() * 2 - 1) * 0.025f)
            .ToArray();
        using var reductor = new ReductorRuidoNeuronalSampleProvider(
            new ProveedorMuestrasPrueba(muestras),
            1);
        var salida = new float[muestras.Length];

        var leidos = reductor.Read(salida, 0, salida.Length);

        Assert.Equal(muestras.Length, leidos);
        Assert.All(salida, muestra => Assert.True(float.IsFinite(muestra)));
        Assert.True(salida.Zip(muestras, (limpia, original) => MathF.Abs(limpia - original)).Average() > 0.0001);
        Assert.True(Rms(salida.Skip(12_000)) < Rms(muestras.Skip(12_000)) * 0.98);
    }

    private static double Rms(IEnumerable<float> muestras)
        => Math.Sqrt(muestras.Average(valor => valor * valor));
}
