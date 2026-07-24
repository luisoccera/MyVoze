using Microsoft.Win32;

namespace VoxMorph.Presentacion.Servicios;

public sealed class ServicioSeleccionArchivos : IServicioSeleccionArchivos
{
    public IReadOnlyList<string> SeleccionarSonidos()
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Importar sonidos al soundboard",
            Filter = "Audio compatible|*.wav;*.mp3|WAV|*.wav|MP3|*.mp3",
            Multiselect = true,
            CheckFileExists = true
        };

        return dialogo.ShowDialog() == true ? dialogo.FileNames : [];
    }
}
