using System.Collections.Generic;
using System.Linq;
using Blaggard.Common;
using Blaggard.Graphics;
using ManulECS;
using Newtonsoft.Json;
using WasteDrudgers.Entities;

namespace WasteDrudgers
{
    public struct Position
    {
        public Vec2 coords;
    }

    public struct Renderable
    {
        public char character;
        public char glyph;
        public Color color;
        public TextureData texture;
    }

    [NeverSerializeComponent]
    public struct IntentionMove
    {
        public Vec2 transform;
    }

    [NeverSerializeComponent]
    public struct IntentionOperate { }

    [NeverSerializeComponent]
    public struct IntentionGetItem { }

    [NeverSerializeComponent]
    public struct IntentionAttack
    {
        public Entity attacker;
        public Entity target;
    }

    [NeverSerializeComponent]
    public struct IntentionUseItem
    {
        public Entity item;
    }

    public struct Actor
    {
        public int energy;
        public int speed;
    }

    public struct Feature
    {
        public string description;
    }

    // Triggers
    public enum EntryTriggerType
    {
        None,
        Thorns,
    }

    public struct EntryTrigger
    {
        public EntryTriggerType type;
    }

    public struct EventMoved { }


    public struct Turn { }

    public struct AI { }

    public struct Faction
    {
        FactionType type;
    }

    [SerializationProfile("global")]
    public struct Player
    {
        int experience;
    }

    public struct InBackpack
    {
        public Entity entity;
    }

    public struct Equipped
    {
        public Entity entity;
        public Slot slot;
    }

    [SerializationProfile("global")]
    public struct PlayerMarker { }

    [NeverSerializeEntity]
    public struct Effect
    {
        public string characters;
        public Color color;
        public float delta;
    }

    [NeverSerializeEntity]
    public struct Damage
    {
        public Entity target;
        public DamageType damageType;
        public int damage;
    }

    public struct Identity
    {
        public string name;
        public string rawName;
        public string description;
    }

    [NeverSerializeEntity]
    public struct Death { }

    public struct Health
    {
        public Stat vigor;
        public Stat health;

        public float vigorRegen;
    }

    public struct Stats
    {
        public Stat strength;
        public Stat endurance;
        public Stat finesse;
        public Stat intellect;
        public Stat resolve;
        public Stat awareness;
    }

    public struct Item
    {
        public ItemType type;
        public IdentificationStatus status;
        public Material material;
        public int weight;
        public int count;
    }

    public struct Obfuscated { }

    [NeverSerializeEntity]
    public struct Portal
    {
        public string targetLevel;
    }

    public struct ActiveEffect
    {
        public Entity target;
        public SpellEffect effect;
        public int duration;
        public int magnitude;
    }

    public struct CastOnUse
    {
        public string spellId;
    }

    public struct Skills
    {
        public List<Skill> set;

        public int GetRank(SkillType type)
        {
            var i = set.FindIndex(s => s.type == type);
            return i != -1 ? set[i].value : 0;
        }

        public void Increment(SkillType type)
        {
            var i = set.FindIndex(s => s.type == type);
            if (i != -1)
            {
                var skill = set[i];
                skill.value++;
                set[i] = skill;
            }
            else
            {
                set.Add(new Skill { type = type, value = 1 });
                set.Sort((a, b) => a.type.CompareTo(b.type));
            }
        }

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

    public enum Wielding
    {
        Unarmed,
        SingleWeapon,
        DualWield,
    }

    public struct NaturalAttack
    {
        public int baseSkill;
        public int minDamage;
        public int maxDamage;
        public string castOnStrike;
    }

    public struct CastOnStrike
    {
        public string spellId;
    }

    public struct Combat
    {
        public Wielding wielding;
        public int hitChance;
        public int minDamage;
        public int maxDamage;
        public int dodge;
        public int armor;

        [JsonIgnore]
        public int SpecialDamage => Damage + Damage;
        [JsonIgnore]
        public int CriticalDamage => maxDamage;
        [JsonIgnore]
        public int Damage => RNG.IntInclusive(minDamage, maxDamage);
        [JsonIgnore]
        public float Average => (minDamage + maxDamage) / 2;
    }

    public struct Shield
    {
        public int baseBlock;
    }

    public struct Weapon
    {
        public int chance;
        public int min;
        public int max;
        public float parry;
        public WeaponStyle style;

        [JsonIgnore]
        public float Average => (min + max) / 2;
    }

    public struct Defense
    {
        public int dodge;
        public int armor;
    }
}