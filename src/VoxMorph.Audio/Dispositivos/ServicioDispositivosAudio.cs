using NAudio.CoreAudioApi;
using VoxMorph.Entidades.Enumeraciones;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Audio.Dispositivos;

public sealed class ServicioDispositivosAudio : IServicioDispositivosAudio
{
    public IReadOnlyList<DispositivoAudio> ObtenerEntradasFisicas()
        => ObtenerDispositivos(DataFlow.Capture, TipoDispositivoAudio.Entrada)
            .Where(dispositivo => !dispositivo.EsVbCable)
            .ToList();

    public IReadOnlyList<DispositivoAudio> ObtenerSalidasVirtuales()
        => ObtenerDispositivos(DataFlow.Render, TipoDispositivoAudio.Salida)
            .Where(dispositivo => dispositivo.EsEntradaVirtualVbCable)
            .ToList();

    public IReadOnlyList<DispositivoAudio> ObtenerSalidasMonitoreo()
        => ObtenerDispositivos(DataFlow.Render, TipoDispositivoAudio.Salida)
            .Where(dispositivo => !dispositivo.EsVbCable)
            .ToList();

    private static IReadOnlyList<DispositivoAudio> ObtenerDispositivos(
        DataFlow flujo,
        TipoDispositivoAudio tipo)
    {
        using var enumerador = new MMDeviceEnumerator();
        var resultado = new List<DispositivoAudio>();
        var dispositivos = enumerador.EnumerateAudioEndPoints(flujo, DeviceState.Active);

        foreach (var dispositivo in dispositivos)
        {
            var identificador = $"{dispositivo.FriendlyName} {dispositivo.DeviceFriendlyName}";
            resultado.Add(new DispositivoAudio
            {
                Id = dispositivo.ID,
                Nombre = dispositivo.FriendlyName,
                Tipo = tipo,
                EsVbCable = ClasificadorVbCable.EsDispositivoVbCable(identificador),
                EsEntradaVirtualVbCable = flujo == DataFlow.Render
                    && ClasificadorVbCable.EsEntradaDeReproduccion(identificador)
            });
        }

        return resultado
            .OrderBy(d => d.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
