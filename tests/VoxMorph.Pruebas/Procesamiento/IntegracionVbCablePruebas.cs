using VoxMorph.Audio.Conversion;
using VoxMorph.Audio.Dispositivos;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Pruebas.Procesamiento;

public sealed class IntegracionVbCablePruebas
{
    [Theory]
    [InlineData("CABLE Input (VB-Audio Virtual Cable)")]
    [InlineData("CABLE-A Input (VB-Audio Cable A)")]
    [InlineData("CABLE-B Input (VB-Audio Cable B)")]
    [InlineData("CABLE-C Input (VB-Audio Cable C)")]
    [InlineData("CABLE-D Input (VB-Audio Cable D)")]
    public void Clasificador_ReconoceEntradasDeReproduccionVbCable(string nombre)
    {
        Assert.True(ClasificadorVbCable.EsDispositivoVbCable(nombre));
        Assert.True(ClasificadorVbCable.EsEntradaDeReproduccion(nombre));
    }

    [Theory]
    [InlineData("CABLE Output (VB-Audio Virtual Cable)")]
    [InlineData("Speakers (Realtek(R) Audio)")]
    [InlineData("Headphones (USB Audio Device)")]
    public void Clasificador_NoConfundeCapturaOMonitoreoConSalidaVirtual(string nombre)
    {
        Assert.False(ClasificadorVbCable.EsEntradaDeReproduccion(nombre));
    }

    [Fact]
    public void ConversorDsp_ImplementaContratoParaMotoresFuturos()
    {
        using IConversorVoz conversor = new ConversorVozDspSampleProvider(
            new ProveedorMuestrasPrueba(Enumerable.Repeat(0.05f, 2048).ToArray()),
            new PerfilVoz { Pitch = 1, ReduccionRuido = 0 });

        conversor.ActualizarPerfil(new PerfilVoz { Pitch = 1.1f, Feminizacion = 0.8f });
        conversor.EstablecerSilencio(false);

        Assert.Equal("DSP local", conversor.NombreMotor);
        Assert.Equal(48_000, conversor.WaveFormat.SampleRate);
    }
}
