using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using VoxMorph.Audio.Procesamiento;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Conversion;

public sealed class ConversorVozDspSampleProvider : IConversorVoz
{
    private readonly SmbPitchShiftingSampleProvider _cambiadorTono;
    private readonly ProcesadorVozSampleProvider _procesador;
    private bool _desechado;

    public ConversorVozDspSampleProvider(ISampleProvider origen, PerfilVoz perfil)
    {
        ArgumentNullException.ThrowIfNull(origen);
        ArgumentNullException.ThrowIfNull(perfil);
        _cambiadorTono = new SmbPitchShiftingSampleProvider(origen)
        {
            PitchFactor = Math.Clamp(perfil.Pitch, 0.5f, 2.0f)
        };
        _procesador = new ProcesadorVozSampleProvider(_cambiadorTono, perfil);
    }

    public string NombreMotor => "DSP local";
    public WaveFormat WaveFormat => _procesador.WaveFormat;
    public float NivelEntrada => _procesador.NivelEntrada;
    public float NivelSalida => _procesador.NivelSalida;

    public int Read(float[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_desechado, this);
        return _procesador.Read(buffer, offset, count);
    }

    public void ActualizarPerfil(PerfilVoz perfil)
    {
        ObjectDisposedException.ThrowIf(_desechado, this);
        ArgumentNullException.ThrowIfNull(perfil);
        _cambiadorTono.PitchFactor = Math.Clamp(perfil.Pitch, 0.5f, 2.0f);
        _procesador.ActualizarPerfil(perfil);
    }

    public void EstablecerSilencio(bool silenciado)
    {
        ObjectDisposedException.ThrowIf(_desechado, this);
        _procesador.EstablecerSilencio(silenciado);
    }

    public void Dispose()
    {
        _desechado = true;
        GC.SuppressFinalize(this);
    }
}
