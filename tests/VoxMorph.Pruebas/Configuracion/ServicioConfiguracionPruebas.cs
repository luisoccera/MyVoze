using VoxMorph.Entidades.Configuracion;
using VoxMorph.Entidades.Modelos;
using VoxMorph.Presentacion.Servicios;

namespace VoxMorph.Pruebas.Configuracion;

public sealed class ServicioConfiguracionPruebas : IDisposable
{
    private readonly string _carpeta = Path.Combine(
        Path.GetTempPath(),
        "VoxMorph.Pruebas",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void GuardarYCargar_ConservaPerfilesSonidosYMonitoreo()
    {
        var ruta = Path.Combine(_carpeta, "configuracion.json");
        var servicio = new ServicioConfiguracion(ruta);
        var configuracion = new ConfiguracionAplicacion
        {
            IdUltimoPerfil = "perfil-1",
            MonitoreoHabilitado = true,
            PerfilesPersonalizados =
            [
                new PerfilVoz
                {
                    Id = "perfil-1",
                    Nombre = "Prueba",
                    Pitch = 1.25f,
                    Feminizacion = 0.85f,
                    RobotFrecuenciaHz = 120,
                    FiltroTelefonico = 0.8f,
                    TremoloMezcla = 0.4f,
                    TremoloFrecuenciaHz = 7
                }
            ],
            Sonidos =
            [
                new SonidoSoundboard { Nombre = "Alerta", Ruta = @"C:\audio\alerta.wav" }
            ]
        };

        servicio.Guardar(configuracion);
        var recuperada = servicio.Cargar();

        Assert.True(recuperada.MonitoreoHabilitado);
        var perfil = Assert.Single(recuperada.PerfilesPersonalizados);
        Assert.Equal(1.25f, perfil.Pitch);
        Assert.Equal(0.85f, perfil.Feminizacion);
        Assert.Equal(120, perfil.RobotFrecuenciaHz);
        Assert.Equal(0.8f, perfil.FiltroTelefonico);
        Assert.Equal(0.4f, perfil.TremoloMezcla);
        Assert.Equal(7, perfil.TremoloFrecuenciaHz);
        Assert.Equal("Alerta", Assert.Single(recuperada.Sonidos).Nombre);
        Assert.Null(servicio.UltimaAdvertencia);
    }

    [Fact]
    public void Cargar_JsonDanadoRecuperaValoresSegurosYRespaldaArchivo()
    {
        Directory.CreateDirectory(_carpeta);
        var ruta = Path.Combine(_carpeta, "configuracion.json");
        File.WriteAllText(ruta, "{ esto no es json");
        var servicio = new ServicioConfiguracion(ruta);

        var recuperada = servicio.Cargar();

        Assert.Empty(recuperada.PerfilesPersonalizados);
        Assert.NotNull(servicio.UltimaAdvertencia);
        Assert.False(File.Exists(ruta));
        Assert.Single(Directory.GetFiles(_carpeta, "configuracion.json.danada-*"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
        {
            Directory.Delete(_carpeta, true);
        }
    }
}
