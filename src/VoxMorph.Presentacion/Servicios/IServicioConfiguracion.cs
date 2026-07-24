using VoxMorph.Entidades.Configuracion;

namespace VoxMorph.Presentacion.Servicios;

public interface IServicioConfiguracion
{
    ConfiguracionAplicacion Cargar();
    void Guardar(ConfiguracionAplicacion configuracion);
    string RutaConfiguracion { get; }
    string? UltimaAdvertencia { get; }
}
