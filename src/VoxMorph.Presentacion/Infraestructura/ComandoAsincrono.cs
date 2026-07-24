using System.Windows.Input;

namespace VoxMorph.Presentacion.Infraestructura;

public sealed class ComandoAsincrono : ICommand
{
    private readonly Func<Task> _ejecutar;
    private readonly Func<bool>? _puedeEjecutar;
    private bool _ejecutando;

    public ComandoAsincrono(Func<Task> ejecutar, Func<bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
        => !_ejecutando && (_puedeEjecutar?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        _ejecutando = true;
        Actualizar();
        try
        {
            await _ejecutar();
        }
        finally
        {
            _ejecutando = false;
            Actualizar();
        }
    }

    public void Actualizar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
