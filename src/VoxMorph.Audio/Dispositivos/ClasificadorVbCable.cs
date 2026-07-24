namespace VoxMorph.Audio.Dispositivos;

public static class ClasificadorVbCable
{
    public static bool EsDispositivoVbCable(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return false;
        }

        return nombre.Contains("VB-Audio", StringComparison.OrdinalIgnoreCase)
            && nombre.Contains("CABLE", StringComparison.OrdinalIgnoreCase);
    }

    public static bool EsEntradaDeReproduccion(string nombre)
        => EsDispositivoVbCable(nombre)
            && nombre.Contains("Input", StringComparison.OrdinalIgnoreCase);
}
