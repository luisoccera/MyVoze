namespace VoxMorph.Entidades.Modelos;

public sealed class PerfilVoz
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nombre { get; set; } = "Personalizado";
    public string Categoria { get; set; } = "General";
    public bool EsPredeterminado { get; set; }
    public float Pitch { get; set; } = 1.0f;
    public float GananciaDb { get; set; }
    public float GravesDb { get; set; }
    public float MediosDb { get; set; }
    public float AgudosDb { get; set; }
    public float UmbralRuidoDb { get; set; } = -48.0f;
    public float ReduccionRuido { get; set; } = 0.72f;
    public float Feminizacion { get; set; }
    public float Distorsion { get; set; }
    public float RobotMezcla { get; set; }
    public float RobotFrecuenciaHz { get; set; } = 55.0f;
    public float FiltroTelefonico { get; set; }
    public float TremoloMezcla { get; set; }
    public float TremoloFrecuenciaHz { get; set; } = 5.0f;
    public float EcoMezcla { get; set; }
    public int EcoMilisegundos { get; set; } = 180;
    public float MezclaEfecto { get; set; } = 1.0f;
    public float VolumenFinal { get; set; } = 1.0f;

    public PerfilVoz Clonar(string? nuevoId = null, string? nuevoNombre = null)
    {
        return new PerfilVoz
        {
            Id = nuevoId ?? Id,
            Nombre = nuevoNombre ?? Nombre,
            Categoria = Categoria,
            EsPredeterminado = EsPredeterminado,
            Pitch = Pitch,
            GananciaDb = GananciaDb,
            GravesDb = GravesDb,
            MediosDb = MediosDb,
            AgudosDb = AgudosDb,
            UmbralRuidoDb = UmbralRuidoDb,
            ReduccionRuido = ReduccionRuido,
            Feminizacion = Feminizacion,
            Distorsion = Distorsion,
            RobotMezcla = RobotMezcla,
            RobotFrecuenciaHz = RobotFrecuenciaHz,
            FiltroTelefonico = FiltroTelefonico,
            TremoloMezcla = TremoloMezcla,
            TremoloFrecuenciaHz = TremoloFrecuenciaHz,
            EcoMezcla = EcoMezcla,
            EcoMilisegundos = EcoMilisegundos,
            MezclaEfecto = MezclaEfecto,
            VolumenFinal = VolumenFinal
        };
    }
}
