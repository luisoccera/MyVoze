using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Entidades.Configuracion;

public sealed class ConfiguracionAplicacion
{
    public string? IdDispositivoEntrada { get; set; }
    public string? IdDispositivoSalida { get; set; }
    public string? IdDispositivoMonitoreo { get; set; }
    public string? IdUltimoPerfil { get; set; }
    public bool MonitoreoHabilitado { get; set; }
    public List<PerfilVoz> PerfilesPersonalizados { get; set; } = [];
    public List<SonidoSoundboard> Sonidos { get; set; } = [];
    public List<AtajoGlobal> Atajos { get; set; } = AtajoGlobal.CrearPredeterminados();
}
