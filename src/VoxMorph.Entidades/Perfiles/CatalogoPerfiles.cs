using VoxMorph.Entidades.Modelos;

namespace VoxMorph.Entidades.Perfiles;

public static class CatalogoPerfiles
{
    public static IReadOnlyList<PerfilVoz> CrearPredeterminados()
    {
        return
        [
            Crear("natural", "Voz natural", 1.00f, 0, 0, 0, 0, 0, 0, 180, 1.00f),
            Crear("grave", "Voz grave", 0.72f, 2, 5, -2, 0.05f, 0, 0.04f, 190, 0.95f),
            Crear("ardilla", "Ardilla", 1.62f, -1, -3, 5, 0, 0, 0, 150, 1.00f),
            Crear("monstruo", "Monstruo", 0.58f, 3, 7, -5, 0.20f, 0.04f, 0.12f, 230, 1.00f),
            Crear("robot", "Robot", 1.00f, 0, -2, 3, 0.12f, 0.88f, 0.04f, 120, 1.00f, robotHz: 72),
            Crear("demonio", "Demonio", 0.67f, 3, 6, -4, 0.34f, 0.16f, 0.15f, 260, 1.00f),
            Crear("radio", "Radio", 1.00f, 1, -8, -3, 0.24f, 0.06f, 0, 100, 0.92f, filtroTelefonico: 0.92f),
            Crear("personalizado", "Personalizado", 1.00f, 0, 0, 0, 0, 0, 0, 180, 1.00f),
            Crear("femenina-suave", "Femenina - Suave", 1.10f, 0, -2, 1.2f, 0, 0, 0, 150, 1.00f, "Femeninas", medios: 0.5f, feminizacion: 0.70f),
            Crear("femenina-brillante", "Femenina - Clara", 1.14f, -0.5f, -3, 2, 0, 0, 0, 130, 1.00f, "Femeninas", medios: 1.2f, feminizacion: 0.85f),
            Crear("femenina-pop", "Femenina - Cálida", 1.08f, 0.5f, -1.5f, 0.5f, 0, 0, 0.02f, 170, 1.00f, "Femeninas", medios: 1, feminizacion: 0.65f),
            Crear("femenina-radio", "Femenina - Presencia", 1.16f, 0, -3, 1.5f, 0, 0, 0, 110, 1.00f, "Femeninas", medios: 2, feminizacion: 0.90f),
            Crear("femenina-robot", "Femenina - Estudio", 1.12f, 0.5f, -2, 1, 0, 0, 0, 150, 1.00f, "Femeninas", medios: 1, feminizacion: 0.85f),
            Crear("masculina-suave", "Masculina - Suave", 0.95f, 0, 1, 0.5f, 0, 0, 0, 170, 1.00f, "Masculinas"),
            Crear("masculina-profunda", "Masculina - Profunda", 0.86f, 1, 2.5f, -0.5f, 0, 0, 0.02f, 210, 1.00f, "Masculinas"),
            Crear("masculina-cine", "Masculina - Cine", 0.90f, 0.5f, 2, 0.5f, 0, 0, 0.03f, 220, 1.00f, "Masculinas"),
            Crear("masculina-radio", "Masculina - Clara", 0.97f, 0.5f, 0, 1.5f, 0, 0, 0, 105, 1.00f, "Masculinas"),
            Crear("masculina-robot", "Masculina - Estudio", 0.92f, 0.5f, 1.5f, 1, 0, 0, 0, 160, 1.00f, "Masculinas"),
            Crear("femenina-aire", "Femenina - Aire", 1.16f, -0.5f, -4, 2.5f, 0, 0, 0, 140, 1.00f, "Femeninas", medios: 0.5f, feminizacion: 0.95f),
            Crear("femenina-cristal", "Femenina - Cristal", 1.18f, -1, -4.5f, 3, 0, 0, 0, 130, 1.00f, "Femeninas", medios: 0.5f, feminizacion: 1),
            Crear("femenina-aterciopelada", "Femenina - Aterciopelada", 1.07f, 1, -1, -0.5f, 0, 0, 0, 180, 1.00f, "Femeninas", medios: 1, feminizacion: 0.60f),
            Crear("femenina-juvenil", "Femenina - Juvenil", 1.17f, 0, -3, 2, 0, 0, 0, 130, 1.00f, "Femeninas", medios: 1, feminizacion: 1),
            Crear("femenina-elegante", "Femenina - Elegante", 1.11f, 0.5f, -2, 1, 0, 0, 0, 170, 1.00f, "Femeninas", medios: 2, feminizacion: 0.80f),
            Crear("femenina-contralto", "Femenina - Contralto", 1.04f, 1, -0.5f, 0, 0, 0, 0, 190, 1.00f, "Femeninas", medios: 1.5f, feminizacion: 0.55f),
            Crear("femenina-dulce", "Femenina - Dulce", 1.14f, 0, -2, 2, 0, 0, 0.02f, 180, 1.00f, "Femeninas", medios: 0.5f, feminizacion: 0.90f),
            Crear("femenina-energetica", "Femenina - Energética", 1.18f, 1, -3, 2.5f, 0, 0, 0, 120, 1.00f, "Femeninas", medios: 2, feminizacion: 1),
            Crear("femenina-nocturna", "Femenina - Nocturna", 1.06f, 1, 0, -1.5f, 0, 0, 0.05f, 240, 1.00f, "Femeninas", medios: -0.5f, feminizacion: 0.55f),
            Crear("femenina-narradora", "Femenina - Narradora", 1.10f, 1, -1, 1, 0, 0, 0, 180, 1.00f, "Femeninas", medios: 2.5f, feminizacion: 0.75f),
            Crear("original-oraculo", "Original - Oráculo", 0.80f, 1, 3, 1, 0.02f, 0.06f, 0.30f, 340, 1.00f, "Personajes originales", medios: 3, robotHz: 34, tremolo: 0.18f, tremoloHz: 2.2f),
            Crear("original-guardian", "Original - Guardián de piedra", 0.68f, 2, 6, -2, 0.18f, 0, 0.08f, 220, 1.00f, "Personajes originales", medios: 1, tremolo: 0.08f, tremoloHz: 1.2f),
            Crear("original-hechicera", "Original - Hechicera lunar", 1.30f, 0, -1, 4, 0, 0, 0.28f, 260, 1.00f, "Personajes originales", medios: 2, tremolo: 0.15f, tremoloHz: 6.2f),
            Crear("original-duende", "Original - Duende travieso", 1.55f, -1, -4, 6, 0.05f, 0.04f, 0.02f, 120, 1.00f, "Personajes originales", medios: 1, robotHz: 140, tremolo: 0.12f, tremoloHz: 9),
            Crear("original-gigante", "Original - Gigante ancestral", 0.55f, 3, 8, -6, 0.28f, 0.03f, 0.14f, 300, 1.00f, "Personajes originales", medios: -2, robotHz: 28, tremolo: 0.10f, tremoloHz: 1),
            Crear("original-espectro", "Original - Espectro", 1.15f, 0, -3, 3, 0, 0.10f, 0.45f, 420, 1.00f, "Personajes originales", medios: -2, robotHz: 26, tremolo: 0.38f, tremoloHz: 4.2f),
            Crear("original-androide", "Original - Androide industrial", 1.00f, 0, -2, 3, 0.12f, 0.92f, 0.03f, 90, 1.00f, "Personajes originales", robotHz: 72, filtroTelefonico: 0.20f, tremolo: 0.10f, tremoloHz: 8),
            Crear("original-droide", "Original - Droide agudo", 1.38f, -1, -4, 5, 0.08f, 0.82f, 0, 100, 1.00f, "Personajes originales", robotHz: 170, filtroTelefonico: 0.35f, tremolo: 0.18f, tremoloHz: 12),
            Crear("original-radio-tactica", "Original - Radio táctica", 0.90f, 1, -5, -5, 0.22f, 0.06f, 0, 100, 1.00f, "Personajes originales", medios: 4, robotHz: 48, filtroTelefonico: 1),
            Crear("original-intercom", "Original - Intercom orbital", 1.05f, 1, -6, -4, 0.10f, 0.12f, 0.08f, 80, 1.00f, "Personajes originales", medios: 5, robotHz: 110, filtroTelefonico: 0.92f, tremolo: 0.08f, tremoloHz: 14),
            Crear("original-villano", "Original - Villano abisal", 0.62f, 3, 7, -5, 0.40f, 0.18f, 0.24f, 280, 1.00f, "Personajes originales", medios: -3, robotHz: 38, tremolo: 0.16f, tremoloHz: 2.8f),
            Crear("original-heroe", "Original - Héroe épico", 0.84f, 2, 4, 2, 0.05f, 0, 0.16f, 230, 1.00f, "Personajes originales", medios: 3, tremolo: 0.04f, tremoloHz: 1.5f),
            Crear("original-cavernaria", "Original - Criatura cavernaria", 0.53f, 3, 9, -8, 0.36f, 0.08f, 0.38f, 360, 1.00f, "Personajes originales", medios: -5, robotHz: 24, tremolo: 0.22f, tremoloHz: 1.3f),
            Crear("original-bosque", "Original - Espíritu del bosque", 1.26f, 0, -2, 5, 0, 0.02f, 0.22f, 310, 1.00f, "Personajes originales", medios: 2, robotHz: 90, tremolo: 0.30f, tremoloHz: 5.5f),
            Crear("original-alien", "Original - Alienígena resonante", 1.18f, 0, -3, 4, 0.08f, 0.55f, 0.16f, 180, 1.00f, "Personajes originales", medios: -1, robotHz: 33, tremolo: 0.42f, tremoloHz: 7.3f),
            Crear("original-narrador", "Original - Narrador antiguo", 0.80f, 2, 5, -1, 0.03f, 0, 0.08f, 240, 1.00f, "Personajes originales", medios: 3),
            Crear("original-goblin", "Original - Goblin eléctrico", 1.65f, -2, -5, 7, 0.25f, 0.18f, 0.05f, 110, 1.00f, "Personajes originales", medios: 2, robotHz: 210, filtroTelefonico: 0.15f, tremolo: 0.35f, tremoloHz: 13),
            Crear("original-ia", "Original - Inteligencia serena", 1.08f, 0, -1, 3, 0, 0.28f, 0.04f, 140, 1.00f, "Personajes originales", medios: 2, robotHz: 96, filtroTelefonico: 0.20f, tremolo: 0.06f, tremoloHz: 6),
            Crear("original-mensajero", "Original - Mensajero galáctico", 0.96f, 1, -4, -1, 0.12f, 0.14f, 0.20f, 190, 1.00f, "Personajes originales", medios: 4, robotHz: 64, filtroTelefonico: 0.75f, tremolo: 0.10f, tremoloHz: 4),
            Crear("original-enmascarado", "Original - Enmascarado", 0.73f, 2, 4, -4, 0.20f, 0.04f, 0.12f, 160, 1.00f, "Personajes originales", medios: -1, robotHz: 44, filtroTelefonico: 0.82f, tremolo: 0.08f, tremoloHz: 3.3f)
        ];
    }

    private static PerfilVoz Crear(
        string id,
        string nombre,
        float pitch,
        float ganancia,
        float graves,
        float agudos,
        float distorsion,
        float robot,
        float eco,
        int ecoMs,
        float mezcla,
        string categoria = "General",
        float medios = 0,
        float robotHz = 55,
        float filtroTelefonico = 0,
        float tremolo = 0,
        float tremoloHz = 5,
        float feminizacion = 0)
    {
        return new PerfilVoz
        {
            Id = $"predeterminado-{id}",
            Nombre = nombre,
            Categoria = categoria,
            EsPredeterminado = true,
            Pitch = pitch,
            GananciaDb = ganancia,
            GravesDb = graves,
            MediosDb = medios,
            AgudosDb = agudos,
            UmbralRuidoDb = categoria == "Femeninas" ? -55 : -48,
            ReduccionRuido = categoria == "Femeninas" ? 0.82f : 0.72f,
            Feminizacion = feminizacion,
            Distorsion = distorsion,
            RobotMezcla = robot,
            RobotFrecuenciaHz = robotHz,
            FiltroTelefonico = filtroTelefonico,
            TremoloMezcla = tremolo,
            TremoloFrecuenciaHz = tremoloHz,
            EcoMezcla = eco,
            EcoMilisegundos = ecoMs,
            MezclaEfecto = mezcla
        };
    }
}
