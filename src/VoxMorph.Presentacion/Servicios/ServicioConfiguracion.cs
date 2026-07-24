using System.IO;
using System.Text.Json;
using VoxMorph.Entidades.Configuracion;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Presentacion.Servicios;

public sealed class ServicioConfiguracion : IServicioConfiguracion
{
    private readonly string? _rutaConfiguracionAnterior;
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ServicioConfiguracion(string? rutaConfiguracion = null)
    {
        if (!string.IsNullOrWhiteSpace(rutaConfiguracion))
        {
            RutaConfiguracion = rutaConfiguracion;
            return;
        }

        var carpetaBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var carpeta = Path.Combine(
            carpetaBase,
            "MyVoze");
        RutaConfiguracion = Path.Combine(carpeta, "configuracion.json");
        _rutaConfiguracionAnterior = Path.Combine(carpetaBase, "VoxMorph", "configuracion.json");
    }

    public string RutaConfiguracion { get; }
    public string? UltimaAdvertencia { get; private set; }

    public ConfiguracionAplicacion Cargar()
    {
        UltimaAdvertencia = null;
        try
        {
            var rutaLectura = File.Exists(RutaConfiguracion)
                ? RutaConfiguracion
                : _rutaConfiguracionAnterior;
            if (string.IsNullOrWhiteSpace(rutaLectura) || !File.Exists(rutaLectura))
            {
                return new ConfiguracionAplicacion();
            }

            var json = File.ReadAllText(rutaLectura);
            var configuracion = JsonSerializer.Deserialize<ConfiguracionAplicacion>(json, OpcionesJson)
                ?? new ConfiguracionAplicacion();
            configuracion.PerfilesPersonalizados ??= [];
            configuracion.Sonidos ??= [];
            configuracion.Atajos ??= AtajoGlobal.CrearPredeterminados();
            return configuracion;
        }
        catch (JsonException)
        {
            RespaldarConfiguracionDanada();
            UltimaAdvertencia = "La configuracion estaba danada. Se respaldo y se cargaron valores seguros.";
            return new ConfiguracionAplicacion();
        }
        catch (IOException excepcion)
        {
            UltimaAdvertencia = $"No se pudo leer la configuracion: {excepcion.Message}";
            return new ConfiguracionAplicacion();
        }
        catch (UnauthorizedAccessException excepcion)
        {
            UltimaAdvertencia = $"Windows impidio leer la configuracion: {excepcion.Message}";
            return new ConfiguracionAplicacion();
        }
    }

    public void Guardar(ConfiguracionAplicacion configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        var carpeta = Path.GetDirectoryName(RutaConfiguracion)
            ?? throw new InvalidOperationException("No se pudo determinar la carpeta de configuracion.");
        Directory.CreateDirectory(carpeta);

        var temporal = RutaConfiguracion + ".tmp";
        var json = JsonSerializer.Serialize(configuracion, OpcionesJson);
        File.WriteAllText(temporal, json);
        File.Move(temporal, RutaConfiguracion, true);
    }

    private void RespaldarConfiguracionDanada()
    {
        try
        {
            var respaldo = RutaConfiguracion + $".danada-{DateTime.Now:yyyyMMdd-HHmmss}";
            File.Move(RutaConfiguracion, respaldo, true);
        }
        catch
        {
            // Cargar la aplicacion con valores seguros tiene prioridad sobre el respaldo.
        }
    }
}
