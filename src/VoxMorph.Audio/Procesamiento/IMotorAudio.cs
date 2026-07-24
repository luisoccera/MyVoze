using VoxMorph.Entidades.Enumeraciones;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Procesamiento;

public interface IMotorAudio : IDisposable
{
    EstadoMotorAudio Estado { get; }
    string? UltimoError { get; }
    float NivelEntrada { get; }
    float NivelSalida { get; }
    bool Silenciado { get; }

    event EventHandler<EstadoMotorAudio>? EstadoCambiado;

    void Iniciar(
        string idEntrada,
        string idSalida,
        PerfilVoz perfil,
        string? idMonitoreo = null,
        bool monitoreoHabilitado = false);
    void Detener();
    void ActualizarPerfil(PerfilVoz perfil);
    void EstablecerSilencio(bool silenciado);
    void ConfigurarMonitoreo(string? idDispositivo, bool habilitado);
    void ReproducirSonido(string ruta, float volumen);
    void DetenerSonidos();
}
