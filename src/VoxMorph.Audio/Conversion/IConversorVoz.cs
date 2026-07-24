using NAudio.Wave;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Conversion;

public interface IConversorVoz : ISampleProvider, IDisposable
{
    string NombreMotor { get; }
    float NivelEntrada { get; }
    float NivelSalida { get; }
    void ActualizarPerfil(PerfilVoz perfil);
    void EstablecerSilencio(bool silenciado);
}
