using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using VoxMorph.Entidades.Configuracion;
using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Presentacion.Servicios;

public sealed class ServicioAtajosGlobales : IServicioAtajosGlobales
{
    private const int MensajeHotkey = 0x0312;
    private readonly Dictionary<int, AtajoGlobal> _registrados = [];
    private HwndSource? _fuente;
    private nint _ventana;
    private Action<AtajoGlobal>? _alActivar;

    public IReadOnlyList<string> Registrar(
        Window ventana,
        IEnumerable<AtajoGlobal> atajos,
        Action<AtajoGlobal> alActivar)
    {
        ArgumentNullException.ThrowIfNull(ventana);
        ArgumentNullException.ThrowIfNull(atajos);
        ArgumentNullException.ThrowIfNull(alActivar);
        Desregistrar();

        var lista = atajos.Where(a => a.Habilitado).ToList();
        var errores = ValidadorAtajos.Validar(lista).ToList();
        if (errores.Count > 0)
        {
            return errores;
        }

        _ventana = new WindowInteropHelper(ventana).Handle;
        _fuente = HwndSource.FromHwnd(_ventana);
        if (_ventana == 0 || _fuente is null)
        {
            return ["No se pudo conectar el servicio de atajos con la ventana."];
        }

        _alActivar = alActivar;
        _fuente.AddHook(ProcesarMensaje);
        var identificador = 0x5100;
        foreach (var atajo in lista)
        {
            try
            {
                var (modificadores, tecla) = Interpretar(atajo.Combinacion);
                if (!RegisterHotKey(_ventana, identificador, modificadores, tecla))
                {
                    errores.Add($"Windows ya usa la combinacion '{atajo.Combinacion}'.");
                    continue;
                }

                _registrados[identificador] = atajo;
                identificador++;
            }
            catch (ArgumentException excepcion)
            {
                errores.Add(excepcion.Message);
            }
        }

        if (errores.Count > 0)
        {
            Desregistrar();
        }

        return errores;
    }

    public void Desregistrar()
    {
        foreach (var identificador in _registrados.Keys)
        {
            UnregisterHotKey(_ventana, identificador);
        }

        _registrados.Clear();
        if (_fuente is not null)
        {
            _fuente.RemoveHook(ProcesarMensaje);
        }

        _fuente = null;
        _ventana = 0;
        _alActivar = null;
    }

    public void Dispose()
    {
        Desregistrar();
        GC.SuppressFinalize(this);
    }

    private nint ProcesarMensaje(
        nint hwnd,
        int mensaje,
        nint wParam,
        nint lParam,
        ref bool controlado)
    {
        if (mensaje == MensajeHotkey
            && _registrados.TryGetValue(wParam.ToInt32(), out var atajo))
        {
            controlado = true;
            _alActivar?.Invoke(atajo);
        }

        return 0;
    }

    private static (uint Modificadores, uint Tecla) Interpretar(string combinacion)
    {
        var partes = combinacion.Split(
            '+',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length < 2)
        {
            throw new ArgumentException($"El atajo '{combinacion}' debe incluir un modificador y una tecla.");
        }

        uint modificadores = 0;
        for (var indice = 0; indice < partes.Length - 1; indice++)
        {
            modificadores |= partes[indice].ToUpperInvariant() switch
            {
                "ALT" => 0x0001u,
                "CTRL" or "CONTROL" => 0x0002u,
                "SHIFT" => 0x0004u,
                "WIN" or "WINDOWS" => 0x0008u,
                _ => throw new ArgumentException($"El modificador '{partes[indice]}' no es valido.")
            };
        }

        var nombreTecla = partes[^1].Length == 1 && char.IsDigit(partes[^1][0])
            ? $"D{partes[^1]}"
            : partes[^1];
        if (!Enum.TryParse<Key>(nombreTecla, true, out var tecla)
            || tecla == Key.None)
        {
            throw new ArgumentException($"La tecla '{partes[^1]}' no es valida.");
        }

        return (modificadores, (uint)KeyInterop.VirtualKeyFromKey(tecla));
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);
}
