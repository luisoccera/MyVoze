using System.Windows;
using VoxMorph.Presentacion.ModelosVista;

namespace VoxMorph.Presentacion;

public partial class PrincipalVentana : Window
{
    private readonly PrincipalModeloVista _modeloVista;

    public PrincipalVentana(PrincipalModeloVista modeloVista)
    {
        InitializeComponent();
        _modeloVista = modeloVista;
        DataContext = modeloVista;
        SourceInitialized += AlInicializarOrigen;
        Loaded += AlCargar;
        Closed += AlCerrar;
    }

    private void AlInicializarOrigen(object? sender, EventArgs e)
    {
        SourceInitialized -= AlInicializarOrigen;
        _modeloVista.ConectarVentana(this);
    }

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        Loaded -= AlCargar;
        await _modeloVista.InicializarAsync();
    }

    private void AlCerrar(object? sender, EventArgs e)
    {
        Closed -= AlCerrar;
        _modeloVista.Dispose();
    }
}
