using System.Windows;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Presentacion.Servicios;

public interface IServicioAtajosGlobales : IDisposable
{
    IReadOnlyList<string> Registrar(
        Window ventana,
        IEnumerable<AtajoGlobal> atajos,
        Action<AtajoGlobal> alActivar);
    void Desregistrar();
}
