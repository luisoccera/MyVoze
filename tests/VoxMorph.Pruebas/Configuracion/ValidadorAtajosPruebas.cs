using VoxMorph.Entidades.Configuracion;
using VoxMorph.Entidades.Enumeraciones;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Pruebas.Configuracion;

public sealed class ValidadorAtajosPruebas
{
    [Fact]
    public void Validar_DetectaCombinacionesDuplicadasSinImportarMayusculas()
    {
        var atajos = new[]
        {
            new AtajoGlobal { Id = "uno", Accion = AccionAtajo.SilenciarMicrofono, Combinacion = "Ctrl+Alt+M" },
            new AtajoGlobal { Id = "dos", Accion = AccionAtajo.DetenerSonidos, Combinacion = "alt + ctrl + m" }
        };

        var errores = ValidadorAtajos.Validar(atajos);

        Assert.Single(errores);
        Assert.Contains("asignada mas de una vez", errores[0]);
    }

    [Fact]
    public void Validar_AceptaCombinacionesUnicas()
    {
        var errores = ValidadorAtajos.Validar(AtajoGlobal.CrearPredeterminados());

        Assert.Empty(errores);
    }
}
