using System.Collections.ObjectModel;

namespace ValleQuebrado.Characters;

public enum CharacterId
{
    Abigrund, Haelyndra, Karrulka, Lyrawen, Pennix, Emmelith,
    Marunna, Brunhelda, Rurikka, Sahandra, Harveld, Sebathiel,
    Samzael, Elliothan, Alekthor, Shaenvar, Lewthar, Morgrana,
    Pyerrin, Gusgrok, Wylgar, Marlorn, Gilgrath, Clintorix,
    Demetrixx, Merlundor, Krocus, Lynquirón, Durnak, Qixaroth
}

public enum CharacterRole
{
    Pretendienta, Pretendiente, AutoridadComercio, OficioMisterio, AldeanoFamilia, Villano
}

/// <summary>Perfil de un personaje no jugador.</summary>
public sealed record CharacterDef(
    CharacterId Id,
    string Name,
    string OriginalNameReference,
    CharacterRole Role,
    string Description,
    bool IsRomanceOption = false);

/// <summary>Estado de amistad y romance del jugador con un personaje.</summary>
public sealed class PlayerRelationship
{
    public const int PointsPerHeart = 250;
    public const int MaximumHearts = 10;
    public const int MaximumFriendshipPoints = PointsPerHeart * MaximumHearts;

    public int FriendshipPoints { get; private set; }
    public int Hearts => FriendshipPoints / PointsPerHeart;
    public bool IsRomanceOption { get; }

    internal PlayerRelationship(bool isRomanceOption) => IsRomanceOption = isRomanceOption;

    internal int ChangeFriendship(int points)
    {
        int oldPoints = FriendshipPoints;
        FriendshipPoints = Math.Clamp(FriendshipPoints + points, 0, MaximumFriendshipPoints);
        return FriendshipPoints - oldPoints;
    }
}

/// <summary>Catálogo de personajes conocidos de Valle Quebrado.</summary>
public static class CastDatabase
{
    private static readonly Dictionary<CharacterId, CharacterDef> CharacterData = new()
    {
        [CharacterId.Abigrund] = new(CharacterId.Abigrund, "Abigrund", "Abigail", CharacterRole.Pretendienta,
            "Gigante guerrera, campeona del gremio y adicta a las pociones picantes.", true),
        [CharacterId.Haelyndra] = new(CharacterId.Haelyndra, "Haelyndra", "Haley", CharacterRole.Pretendienta,
            "Elfa de alta sociedad, estilista y coleccionista de gemas exiliada de la Casa Aurelion.", true),
        [CharacterId.Karrulka] = new(CharacterId.Karrulka, "Karrulka", "Caroline", CharacterRole.Pretendienta,
            "Goblin herbalista que cuida un invernadero de té y remedios contra maldiciones.", true),
        [CharacterId.Lyrawen] = new(CharacterId.Lyrawen, "Lyrawen", "Leah", CharacterRole.Pretendienta,
            "Dríade escultora, antiguo espíritu de bosque que tomó forma humana.", true),
        [CharacterId.Pennix] = new(CharacterId.Pennix, "Pennix", "Penny", CharacterRole.Pretendienta,
            "Diablilla archivista y profesora, hija de Pamdora, renunció a sus cuernos de fuego por la luz.", true),
        [CharacterId.Emmelith] = new(CharacterId.Emmelith, "Emmelith", "Emily", CharacterRole.Pretendienta,
            "Hada pura, camarera y costurera, hermana adoptiva de Haelyndra que lee cristales.", true),
        [CharacterId.Marunna] = new(CharacterId.Marunna, "Maru", "Maru", CharacterRole.Pretendienta,
            "Enana artífice rúnica, experta en ingeniería y obsesionada con el Núcleo del Portal.", true),
        [CharacterId.Brunhelda] = new(CharacterId.Brunhelda, "Brunhelda", "Marnie", CharacterRole.Pretendienta,
            "Enana herrera de la Fragua del Cráter, dueña de la espada consciente Gerardo.", true),
        [CharacterId.Rurikka] = new(CharacterId.Rurikka, "Robin", "Robin", CharacterRole.Pretendienta,
            "Enana carpintera experta y constructora con su sierra de vapor Bertha.", true),
        [CharacterId.Sahandra] = new(CharacterId.Sahandra, "Sandy", "Sandy", CharacterRole.Pretendienta,
            "Djinn del desierto liberada de su lámpara, vende objetos de otros mundos (ruta secreta).", true),

        [CharacterId.Lewthar] = new(CharacterId.Lewthar, "Lewthar", "Lewis", CharacterRole.AutoridadComercio,
            "Archimago alcalde y jefe del gremio de aventureros."),
        [CharacterId.Morgrana] = new(CharacterId.Morgrana, "Morgrana", "Marnie", CharacterRole.AutoridadComercio,
            "Mujer-bestia domadora de grifos, wyverns y jabalíes de guerra."),
        [CharacterId.Pyerrin] = new(CharacterId.Pyerrin, "Pyerrin", "Pierre", CharacterRole.AutoridadComercio,
            "Mercader halfling, dueño de Pyerrin e Hijos del Botín."),
        [CharacterId.Gusgrok] = new(CharacterId.Gusgrok, "Gus", "Gus", CharacterRole.AutoridadComercio,
            "Ogro cocinero dueño de la taberna El Caldero Ardiente."),
        [CharacterId.Wylgar] = new(CharacterId.Wylgar, "Wylgar", "Willy", CharacterRole.AutoridadComercio,
            "Sirénido viejo, pescador y guía de las cuevas submarinas."),
        [CharacterId.Marlorn] = new(CharacterId.Marlorn, "Marlorn", "Marlon", CharacterRole.AutoridadComercio,
            "Guerrero tuerto, líder del Gremio de Aventureros."),
        [CharacterId.Qixaroth] = new(CharacterId.Qixaroth, "Qixaroth", "Sr. Qi", CharacterRole.Villano,
            "Señor Oscuro de las Almas Cautivas, creador de los calabozos como granjas de almas y fachada del Consorcio Yoyo.")
    };

    static CastDatabase()
    {
        // Conserva disponibles todas las identidades del elenco aunque aún no tengan
        // una descripción narrativa detallada.
        foreach (CharacterId id in Enum.GetValues<CharacterId>())
        {
            CharacterData.TryAdd(id, new CharacterDef(
                id, id.ToString(), string.Empty, CharacterRole.AldeanoFamilia,
                "Perfil pendiente de completar."));
        }
    }

    public static IReadOnlyDictionary<CharacterId, CharacterDef> Characters { get; } =
        new ReadOnlyDictionary<CharacterId, CharacterDef>(CharacterData);

    public static bool TryGet(CharacterId id, out CharacterDef? character) =>
        CharacterData.TryGetValue(id, out character);
}

/// <summary>Gestiona relaciones del jugador con los personajes registrados en la biblia.</summary>
public sealed class CharacterManager
{
    private readonly Dictionary<CharacterId, PlayerRelationship> _relationships = new();

    public CharacterManager()
    {
        foreach (CharacterDef character in CastDatabase.Characters.Values)
            _relationships.Add(character.Id, new PlayerRelationship(character.IsRomanceOption));
    }

    public IReadOnlyDictionary<CharacterId, PlayerRelationship> Relationships =>
        new ReadOnlyDictionary<CharacterId, PlayerRelationship>(_relationships);

    public bool TryGetRelationship(CharacterId id, out PlayerRelationship? relationship) =>
        _relationships.TryGetValue(id, out relationship);

    /// <summary>Añade o resta puntos de amistad; devuelve el cambio aplicado tras limitar el rango.</summary>
    public int ChangeFriendship(CharacterId id, int points)
    {
        if (!_relationships.TryGetValue(id, out PlayerRelationship? relationship))
            return 0;
        return relationship.ChangeFriendship(points);
    }
}
