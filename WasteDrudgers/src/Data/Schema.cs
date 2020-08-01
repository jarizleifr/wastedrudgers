using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

using Blaggard.Common;
using WasteDrudgers.Entities;
using WasteDrudgers.Level.Generation;

namespace WasteDrudgers.Data
{
    /// <summary>Base class for each database entity with an id field</summary>
    public class DBEntity
    {
        public string Id { get; set; }
    }

    public class DBMaterial : DBEntity
    {
        public string Name { get; set; }
        public int DangerLevelModifier { get; set; }
        public int DamageBonus { get; set; }
        public int ArmorBonus { get; set; }
        public float Quality { get; set; }
        public float WeightMult { get; set; }

        [JsonIgnore]
        public Color Color { get; set; }
    }

    public class DBItem : DBEntity
    {
        public string Name { get; set; }
        public string UnidentifiedName { get; set; }
        public bool Obfuscated { get; set; }

        public ItemType Type { get; set; }
        public char Character { get; set; }
        public char Glyph { get; set; }

        public int DangerLevel { get; set; }
        public int Weight { get; set; }
        public int Value { get; set; }

        // Weapon fields
        public int BaseSkill { get; set; }
        public float Parry { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }

        // Armor fields
        public int Dodge { get; set; }
        public int Armor { get; set; }
        public int SpellDefense { get; set; }

        [JsonIgnore]
        public DBSpell UseSpell { get; set; }

        [JsonIgnore]
        public DBMaterial BaseMaterial { get; set; }

        [JsonIgnore]
        public DBMaterialGroup MaterialGroup { get; set; }
    }

    public class DBMaterialGroup
    {
        [JsonIgnore]
        public List<(DBMaterial material, int probability)> Materials { get; set; }
    }

    public class DBLootList : DBEntity
    {
        [JsonIgnore]
        public DBItem[] Items { get; set; }
    }

    public class DBLevel : DBEntity
    {
        public ILevelGenerationStrategy Strategy { get; set; }
        public int DangerLevel { get; set; }
        public int MinSpawn { get; set; }
        public int MaxSpawn { get; set; }

        [JsonIgnore]
        public List<DBCreatureList> Creatures { get; set; }

        [JsonIgnore]
        public List<DBLootList> Loot { get; set; }

        [JsonIgnore]
        public List<DBPortal> Portals { get; set; }
    }

    public class DBFeature : DBEntity
    {
        public string Name { get; set; }
        public char Character { get; set; }

        [JsonIgnore]
        public Color Color { get; set; }

        public string Description { get; set; }
        public EntryTriggerType EntryTrigger { get; set; }
    }

    public class DBPortal
    {
        public string TargetLevelId { get; set; }
        public DBFeature Feature { get; set; }
    }

    public class DBLogMessage : DBEntity
    {
        public LoggingLevel LoggingLevel { get; set; }
        public string Message { get; set; }
    }

    public class DBRace : DBEntity
    {
        public string Name { get; set; }

        [JsonIgnore]
        public Color Color { get; set; }

        public int StrengthModifier { get; set; }
        public int EnduranceModifier { get; set; }
        public int FinesseModifier { get; set; }
        public int IntellectModifier { get; set; }
        public int ResolveModifier { get; set; }
        public int AwarenessModifier { get; set; }
    }

    public class DBSpell : DBEntity
    {
        public string Name { get; set; }

        [JsonIgnore]
        public int MinMagnitude { get; set; }
        [JsonIgnore]
        public int MaxMagnitude { get; set; }

        public int Duration { get; set; }
        public SpellEffect Effect { get; set; }

        [JsonIgnore]
        public DBLogMessage Message { get; set; }
    }

    public class DBCreature : DBEntity
    {
        public string Name { get; set; }

        [JsonIgnore]
        public Color Color { get; set; }

        public char Character { get; set; }
        public char Glyph { get; set; }
        public string Sprite { get; set; }

        [JsonIgnore]
        public DBRace Race { get; set; }

        [JsonIgnore]
        public List<(List<SkillType>, int)> Professions { get; set; }

        [JsonIgnore]
        public DBNaturalAttack NaturalAttack { get; set; }
    }

    public class DBNaturalAttack : DBEntity
    {
        public int BaseSkill { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }

        [JsonIgnore]
        public DBSpell CastOnStrike { get; set; }
    }

    public class DBCreatureList : DBEntity
    {
        [JsonIgnore]
        public DBCreature[] Creatures { get; set; }
    }

    public class DBMapData
    {
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonIgnore]
        public byte[] Tiles { get; set; }
    }

    public class DBObfuscatedNames
    {
        public ItemType Type { get; set; }
        public List<string> Names { get; set; }
    }
}