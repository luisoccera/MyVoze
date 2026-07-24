using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Entidades.Configuracion;

public static class ValidadorAtajos
{
    public static IReadOnlyList<string> Validar(IEnumerable<AtajoGlobal> atajos)
    {
        ArgumentNullException.ThrowIfNull(atajos);
        var errores = new List<string>();
        var habilitados = atajos.Where(a => a.Habilitado).ToList();

        foreach (var atajo in habilitados)
        {
            if (string.IsNullOrWhiteSpace(atajo.Combinacion))
            {
                errores.Add($"El atajo '{atajo.Id}' no tiene una combinacion.");
            }
        }

        var duplicados = habilitados
            .Where(a => !string.IsNullOrWhiteSpace(a.Combinacion))
            .GroupBy(a => Normalizar(a.Combinacion), StringComparer.OrdinalIgnoreCase)
            .Where(grupo => grupo.Count() > 1);

        foreach (var duplicado in duplicados)
        {
            errores.Add($"La combinacion '{duplicado.Key}' esta asignada mas de una vez.");
        }

        return errores;
    }

    public static string Normalizar(string combinacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(combinacion);
        var partes = combinacion
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(parte => parte.ToUpperInvariant())
            .ToArray();
        if (partes.Length < 2)
        {
            return string.Join("+", partes);
        }

        var modificadores = partes[..^1].OrderBy(parte => parte, StringComparer.Ordinal);
        return string.Join("+", modificadores.Append(partes[^1]));
    }
}
