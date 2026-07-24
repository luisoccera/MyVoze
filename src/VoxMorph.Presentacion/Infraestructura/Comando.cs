using System.Windows.Input;

namespace VoxMorph.Presentacion.Infraestructura;

public sealed class Comando : ICommand
{
    private readonly Action _ejecutar;
    private readonly Func<bool>? _puedeEjecutar;

    public Comando(Action ejecutar, Func<bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _puedeEjecutar?.Invoke() ?? true;
    public void Execute(object? parameter) => _ejecutar();
    public void Actualizar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
