using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Dispositivos;

public interface IServicioDispositivosAudio
{
    IReadOnlyList<DispositivoAudio> ObtenerEntradasFisicas();
    IReadOnlyList<DispositivoAudio> ObtenerSalidasVirtuales();
    IReadOnlyList<DispositivoAudio> ObtenerSalidasMonitoreo();
}
