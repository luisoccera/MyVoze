using NAudio.Wave;
using VoxMorph.Audio.Mezcla;

namespace VoxMorph.Pruebas.Procesamiento;

public sealed class MezcladorSoundboardPruebas : IDisposable
{
    private readonly string _ruta = Path.Combine(
        Path.GetTempPath(),
        $"voxmorph-sonido-{Guid.NewGuid():N}.wav");

    [Fact]
    public void Reproducir_MezclaWavYRespetaLimitador()
    {
        CrearWav();
        var voz = new ProveedorMuestrasPrueba(Enumerable.Repeat(0.8f, 512).ToArray());
        var mezclador = new MezcladorSoundboardSampleProvider(voz);
        mezclador.Reproducir(_ruta, 1);
        var salida = new float[512];

        mezclador.Read(salida, 0, salida.Length);

        Assert.Contains(salida, muestra => muestra > 0.8f);
        Assert.All(salida, muestra => Assert.InRange(muestra, -0.95f, 0.95f));
    }

    private void CrearWav()
    {
        using var escritor = new WaveFileWriter(
            _ruta,
            WaveFormat.CreateIeeeFloatWaveFormat(48_000, 1));
        var muestras = Enumerable.Repeat(0.4f, 512).ToArray();
        escritor.WriteSamples(muestras, 0, muestras.Length);
    }

    public void Dispose()
    {
        if (File.Exists(_ruta))
        {
            File.Delete(_ruta);
        }
    }
}
