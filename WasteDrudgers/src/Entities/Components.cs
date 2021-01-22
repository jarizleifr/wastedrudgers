using System;
using System.Collections.Generic;
using Blaggard.Common;
using Blaggard.Graphics;
using ManulECS;
using Newtonsoft.Json;
using WasteDrudgers.Common;
using WasteDrudgers.Entities;

namespace WasteDrudgers
{
    public struct Position : IComponent
    {
        public Vec2 coords;
    }

    public struct Renderable : IComponent
    {
        public char character;
        public Color color;
        public TextureData texture;
    }

    [NeverSerializeComponent]
    public struct IntentionMove : IComponent
    {
        public Vec2 transform;
    }

    [NeverSerializeComponent]
    public struct IntentionOperate : ITag { }

    [NeverSerializeComponent]
    public struct IntentionGetItem : ITag { }

    [NeverSerializeComponent]
    public struct IntentionAttack : IComponent
    {
        public Entity attacker;
        public Entity target;
    }

    [NeverSerializeComponent]
    public struct IntentionUseItem : IComponent
    {
        public Entity item;
    }

    public struct Actor : IComponent
    {
        public int energy;
        public int speed;
    }

    public struct Feature : IComponent
    {
        public string description;
    }

    // Triggers
    public enum EntryTriggerType
    {
        None,
        Thorns,
    }

    public struct EntryTrigger : IComponent
    {
        public EntryTriggerType type;
    }

    public struct EventMoved : ITag { }
    public struct EventActed : IComponent
    {
        public int energyLoss;
        public int nutritionLoss;
    }

    public struct EventStatsUpdated : ITag { }
    public struct EventInventoryUpdated : ITag { }
    public struct Turn : ITag { }
    public struct AI : ITag { }

    public struct Faction : IComponent
    {
        FactionType type;
    }

    [SerializationProfile("global")]
    public struct Player : ITag { }

    public struct InBackpack : IComponent
    {
        public Entity entity;
    }

    public struct Equipped : IComponent
    {
        public Entity entity;
        public Slot slot;
    }

    [SerializationProfile("global")]
    public struct PlayerMarker : ITag { }

    [NeverSerializeEntity]
    public struct VisualEffect : IComponent
    {
        public string characters;
        public Color color;
        public float delta;
    }

    [NeverSerializeEntity]
    public struct Damage : IComponent
    {
        public Entity target;
        public DamageType damageType;
        public int damage;
    }

    public struct PlayerInitiated : ITag { }

    public struct Identity : IComponent
    {
        public string name;
        public string rawName;
        public string description;
    }

    [NeverSerializeEntity]
    public struct Death : ITag { }

    public struct Pools : IComponent
    {
        public Stat vigor;
        public Stat health;

        public float vigorRegen;
        public bool fatigued;
    }

    public struct Experience : IComponent
    {
        public int level;
        public int experience;
        public int characterPoints;
    }

    public enum HungerState
    {
        Sated,
        LowFood,
        Hungry,
    }

    public struct HungerClock : IComponent
    {
        public int nutrition;
        public int food;

        [JsonIgnore]
        public int Total => nutrition + food;

        [JsonIgnore]
        public HungerState State => Total switch
        {
            var _ when Total == 0 => HungerState.Hungry,
            var _ when Total < 500 => HungerState.LowFood,
            _ => HungerState.Sated,
        };
    }

    public struct Afflictions : IComponent
    {
        public byte poison;
    }

    public struct Stats : IComponent
    {
        public Attrib strength;
        public Attrib endurance;
        public Attrib finesse;
        public Attrib intellect;
        public Attrib resolve;
        public Attrib awareness;

        public Attrib this[StatType type]
        {
            get => type switch
            {
                StatType.Strength => strength,
                StatType.Endurance => endurance,
                StatType.Finesse => finesse,
                StatType.Intellect => intellect,
                StatType.Resolve => resolve,
                StatType.Awareness => awareness,
            };
            set
            {
                switch (type)
                {
                    case StatType.Strength:
                        strength.Base = value;
                        break;
                    case StatType.Endurance:
                        endurance.Base = value;
                        break;
                    case StatType.Finesse:
                        finesse.Base = value;
                        break;
                    case StatType.Intellect:
                        intellect.Base = value;
                        break;
                    case StatType.Resolve:
                        resolve.Base = value;
                        break;
                    case StatType.Awareness:
                        awareness.Base = value;
                        break;
                }
            }
        }
    }

    public struct Item : IComponent
    {
        public ItemType type;
        public IdentificationStatus status;
        public string material;
        public int weight;
        public int count;
    }

    public struct Obfuscated : ITag { }

    [NeverSerializeEntity]
    public struct Portal : IComponent
    {
        public string targetLevel;
    }

    public struct IsTalent : ITag { }
    public struct IsTrait : ITag { }

    public struct ActiveEffect : IComponent
    {
        public Entity target;
        public Effect effect;
    }

    public struct Duration : IComponent
    {
        public int duration;
    }

    public struct CastOnUse : IComponent
    {
        public string spellId;
    }

    public struct Skills : IComponent
    {
        public List<Skill> set;

        public int GetRank(SkillType type)
        {
            var i = set.FindIndex(s => s.type == type);
            return i != -1 ? set[i].value : 0;
        }

        public void Add(SkillType type, int value)
        {
            var i = set.FindIndex(s => s.type == type);
            if (i != -1)
            {
                var skill = set[i];
                skill.value += value;
                set[i] = skill;
            }
            else
            {
                set.Add(new Skill { type = type, value = value });
                set.Sort((a, b) => a.type.CompareTo(b.type));
            }
        }

        public void Increment(SkillType type) => Add(type, 1);

        public void Decrement(SkillType type)
        {
            var i = set.FindIndex(s => s.type == type);
            if (i != -1)
            {
                var skill = set[i];
                if (skill.value - 1 == 0)
                {
                    set.Remove(skill);
                }
                else
                {
                    skill.value--;
                    set[i] = skill;
                }
            }
        }
    }

    public enum WieldingType
    {
        Unarmed,
        SingleWeapon,
        DualWield,
    }

    public struct NaturalAttack : IComponent
    {
        public int baseSkill;
        public Extent damage;
        public string castOnStrike;
    }

    public struct CastOnStrike : IComponent
    {
        public string spellId;
    }

    public struct Shield : IComponent
    {
        public int baseBlock;
    }

    public struct Attack : IComponent
    {
        public int hitChance;
        public int minDamage;
        public int maxDamage;
        public int parry;

        [JsonIgnore]
        public int SpecialDamage => Damage + Damage;
        [JsonIgnore]
        public int CriticalDamage => maxDamage;
        [JsonIgnore]
        public int Damage => RNG.IntInclusive(minDamage, maxDamage);
        [JsonIgnore]
        public float Average => (minDamage + maxDamage) / 2;
        [JsonIgnore]
        public string DamageString => $"{minDamage}─{maxDamage}";
    }

    public struct Defense : IComponent
    {
        public int parry;
        public int armor;
        public int evasion;
        public int fortitude;
        public int mental;
    }

    public struct Armor : IComponent
    {
        public int evasionPenalty;
        public int armor;
    }
}