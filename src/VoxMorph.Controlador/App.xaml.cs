using System.IO;
using System.Windows;
using NAudio.Wave.SampleProviders;
using VoxMorph.Audio.Dispositivos;
using VoxMorph.Audio.Procesamiento;
using VoxMorph.Presentacion;
using VoxMorph.Presentacion.ModelosVista;
using VoxMorph.Presentacion.Servicios;

namespace VoxMorph.Controlador;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (!Environment.Is64BitProcess || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            MessageBox.Show(
                "MyVoze requiere Windows 11 x64.",
                "Sistema no compatible",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Environment.ExitCode = 1;
            Shutdown(1);
            return;
        }

        if (e.Args.Contains("--autodiagnostico-audio", StringComparer.OrdinalIgnoreCase))
        {
            EjecutarAutodiagnosticoAudio();
            return;
        }

        var dispositivos = new ServicioDispositivosAudio();
        var motor = new MotorAudio();
        var configuracion = new ServicioConfiguracion();
        var selectorArchivos = new ServicioSeleccionArchivos();
        var atajos = new ServicioAtajosGlobales();
        var modeloVista = new PrincipalModeloVista(
            dispositivos,
            motor,
            configuracion,
            selectorArchivos,
            atajos);
        var ventana = new PrincipalVentana(modeloVista);

        MainWindow = ventana;
        ventana.Show();
    }

    private void EjecutarAutodiagnosticoAudio()
    {
        var rutaResultado = Path.Combine(Path.GetTempPath(), "MyVoze-autodiagnostico-audio.txt");
        try
        {
            var origen = new SignalGenerator(48_000, 1)
            {
                Frequency = 220,
                Gain = 0.08,
                Type = SignalGeneratorType.Sin
            };
            using var reductor = new ReductorRuidoNeuronalSampleProvider(origen, 1);
            var salida = new float[4_800];
            var leidos = reductor.Read(salida, 0, salida.Length);
            if (leidos != salida.Length || salida.Any(muestra => !float.IsFinite(muestra)))
            {
                throw new InvalidOperationException("RNNoise entregó una salida inválida.");
            }

            File.WriteAllText(
                rutaResultado,
                $"RNNOISE_OK{Environment.NewLine}MUESTRAS={leidos}{Environment.NewLine}");
            Environment.ExitCode = 0;
            Shutdown(0);
        }
        catch (Exception excepcion)
        {
            File.WriteAllText(
                rutaResultado,
                $"RNNOISE_ERROR{Environment.NewLine}{excepcion}{Environment.NewLine}");
            Environment.ExitCode = 1;
            Shutdown(1);
        }
    }
}
