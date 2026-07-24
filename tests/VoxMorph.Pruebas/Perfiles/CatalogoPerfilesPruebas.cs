using VoxMorph.Entidades.Perfiles;

namespace VoxMorph.Pruebas.Perfiles;

public sealed class CatalogoPerfilesPruebas
{
    [Fact]
    public void Catalogo_TieneCuarentaYOchoPerfilesConIdsUnicos()
    {
        var perfiles = CatalogoPerfiles.CrearPredeterminados();

        Assert.Equal(48, perfiles.Count);
        Assert.Equal(perfiles.Count, perfiles.Select(p => p.Id).Distinct().Count());
        Assert.All(perfiles, perfil => Assert.True(perfil.EsPredeterminado));
    }

    [Fact]
    public void Catalogo_TieneQuinceFemeninosYCincoMasculinos()
    {
        var perfiles = CatalogoPerfiles.CrearPredeterminados();
        var femeninos = perfiles.Where(perfil => perfil.Categoria == "Femeninas").ToList();
        var masculinos = perfiles.Where(perfil => perfil.Categoria == "Masculinas").ToList();

        Assert.Equal(15, femeninos.Count);
        Assert.Equal(5, masculinos.Count);
        Assert.All(femeninos, perfil => Assert.InRange(perfil.Pitch, 1.04f, 1.18f));
        Assert.All(masculinos, perfil => Assert.InRange(perfil.Pitch, 0.85f, 1.0f));
        Assert.All(femeninos, perfil =>
        {
            Assert.InRange(perfil.Feminizacion, 0.55f, 1.0f);
            Assert.InRange(perfil.ReduccionRuido, 0.8f, 1.0f);
            Assert.True(perfil.GravesDb <= 0);
        });
        Assert.All(femeninos.Concat(masculinos), perfil =>
        {
            Assert.Equal(0, perfil.Distorsion);
            Assert.Equal(0, perfil.RobotMezcla);
            Assert.InRange(perfil.ReduccionRuido, 0.6f, 1.0f);
        });
    }

    [Fact]
    public void Catalogo_TieneVeintePersonajesOriginalesConEfectosDistintos()
    {
        var personajes = CatalogoPerfiles.CrearPredeterminados()
            .Where(perfil => perfil.Categoria == "Personajes originales")
            .ToList();

        var firmas = personajes
            .Select(perfil => (
                perfil.Pitch,
                perfil.Distorsion,
                perfil.RobotMezcla,
                perfil.RobotFrecuenciaHz,
                perfil.FiltroTelefonico,
                perfil.TremoloMezcla,
                perfil.TremoloFrecuenciaHz,
                perfil.EcoMezcla))
            .Distinct()
            .Count();

        Assert.Equal(20, personajes.Count);
        Assert.Equal(20, firmas);
    }

    [Fact]
    public void Clonar_CreaUnaInstanciaIndependiente()
    {
        var original = CatalogoPerfiles.CrearPredeterminados().First();
        var copia = original.Clonar("copia", "Mi copia");
        copia.GananciaDb = 8;
        copia.FiltroTelefonico = 1;
        copia.Feminizacion = 1;

        Assert.NotSame(original, copia);
        Assert.Equal("copia", copia.Id);
        Assert.NotEqual(original.GananciaDb, copia.GananciaDb);
        Assert.NotEqual(original.FiltroTelefonico, copia.FiltroTelefonico);
        Assert.NotEqual(original.Feminizacion, copia.Feminizacion);
    }
}
