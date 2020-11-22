using System.Collections.Generic;
using Blaggard.Common;
using Newtonsoft.Json;
using WasteDrudgers.Common;
using WasteDrudgers.Entities;
using WasteDrudgers.Level.Generation;

namespace WasteDrudgers
{
    /// <summary>Base class for each database entity with an id field</summary>
    public class DBEntity
    {
        public string Id { get; set; }
    }

    public class DBMaterial : DBEntity
    {
        public string Name { get; set; }
        [JsonProperty("DL")]
        public int DangerLevelModifier { get; set; }
        [JsonProperty("Damage")]
        public int DamageBonus { get; set; }
        [JsonProperty("Armor")]
        public int ArmorBonus { get; set; }
        public float Quality { get; set; }
        public float WeightMult { get; set; }
        public Color Color { get; set; }
    }

    public class DBItem : DBEntity
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public char Char { get; set; }
        [JsonProperty("DL")]
        public int DangerLevel { get; set; }
        public int Weight { get; set; }
        public int Value { get; set; }
        public string UseSpell { get; set; }
        public string Material { get; set; }
        public List<string> MaterialTags { get; set; }
    }

    public class DBWeapon : DBItem
    {
        public int BaseSkill { get; set; }
        public float Parry { get; set; }
        public Extent Damage { get; set; }
    }

    public class DBApparel : DBItem
    {
        public int Dodge { get; set; }
        public int Armor { get; set; }
        public int SpellDefense { get; set; }
    }

    public class DBLevel : DBEntity
    {
        public ILevelGenerationStrategy Strategy { get; set; }
        public int DangerLevel { get; set; }
        public Extent? Spawns { get; set; }
        public List<string> LevelTags { get; set; }
        public List<DBPortal> Portals { get; set; }
    }

    public class DBFeature : DBEntity
    {
        public string Name { get; set; }
        public char Char { get; set; }
        public Color Color { get; set; }
        public string Description { get; set; }
        public EntryTriggerType Trigger { get; set; }
    }

    public class DBPortal
    {
        public string Origin { get; set; }
        public string Target { get; set; }
        public string Feature { get; set; }
        public Vec2? Coords { get; set; }
    }

    public class DBLogMessage : DBEntity
    {
        public LoggingLevel LoggingLevel { get; set; }
        public string Message { get; set; }
    }

    public class DBSpell : DBEntity
    {
        public string Name { get; set; }
        public Extent Magnitude { get; set; }
        public int Duration { get; set; }
        public SpellEffect Effect { get; set; }
        public string Message { get; set; }
    }

    public class DBCreature : DBEntity
    {
        public string Name { get; set; }
        public char Char { get; set; }
        public Color Color { get; set; }
        public int Strength { get; set; }
        public int Endurance { get; set; }
        public int Finesse { get; set; }
        public int Intellect { get; set; }
        public int Resolve { get; set; }
        public int Awareness { get; set; }
        public List<string> Professions { get; set; }
        public string NaturalAttack { get; set; }
    }

    public class DBNaturalAttack : DBEntity
    {
        public int BaseSkill { get; set; }
        public Extent Damage { get; set; }
        public string CastOnStrike { get; set; }
    }

    public class DBMapData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Tiles { get; set; }
    }
}