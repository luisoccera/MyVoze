using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoxMorph.Presentacion.Infraestructura;

public abstract class ObservableBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool Establecer<T>(ref T campo, T valor, [CallerMemberName] string? propiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor))
        {
            return false;
        }

        campo = valor;
        Notificar(propiedad);
        return true;
    }

    protected void Notificar([CallerMemberName] string? propiedad = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
}
