using System;

namespace WasteDrudgers.Entities
{
    public enum ProfessionFocus
    {
        MartialArts,
        ShortBlade,
        LongBlade,
        AxeMace,
        Polearm,
        Fencing,
        Whip,
        Shield,
        Observation,

        Vigor,
        Health,
        Magic,

        Strength,
        Endurance,
        Resolve,
        Awareness,

        Finesse,
        Intellect,
    }


    public static class ProfessionFocusExtensions
    {
        public static int GetCost(this ProfessionFocus focus)
        {
            switch (focus)
            {
                case ProfessionFocus.Finesse:
                case ProfessionFocus.Intellect:
                    return 20;
                case ProfessionFocus.Strength:
                case ProfessionFocus.Endurance:
                case ProfessionFocus.Resolve:
                case ProfessionFocus.Awareness:
                    return 10;
                case ProfessionFocus.Vigor:
                case ProfessionFocus.Health:
                case ProfessionFocus.Magic:
                    return 5;
                // If skill
                default:
                    return 2;
            }
        }

        public static bool IsStat(this ProfessionFocus focus) => focus switch
        {
            ProfessionFocus.Strength => true,
            ProfessionFocus.Endurance => true,
            ProfessionFocus.Finesse => true,
            ProfessionFocus.Intellect => true,
            ProfessionFocus.Resolve => true,
            ProfessionFocus.Awareness => true,
            _ => false
        };

        public static StatType GetStat(this ProfessionFocus focus) => focus switch
        {
            ProfessionFocus.Strength => StatType.Strength,
            ProfessionFocus.Endurance => StatType.Endurance,
            ProfessionFocus.Finesse => StatType.Finesse,
            ProfessionFocus.Intellect => StatType.Intellect,
            ProfessionFocus.Resolve => StatType.Resolve,
            ProfessionFocus.Awareness => StatType.Awareness,
            _ => throw new Exception("Not a StatType"),
        };

        public static bool IsPool(this ProfessionFocus focus) => focus switch
        {
            ProfessionFocus.Vigor => true,
            ProfessionFocus.Health => true,
            ProfessionFocus.Magic => true,
            _ => false
        };

        public static PoolType GetPool(this ProfessionFocus focus) => focus switch
        {
            ProfessionFocus.Vigor => PoolType.Vigor,
            ProfessionFocus.Health => PoolType.Health,
            ProfessionFocus.Magic => PoolType.Magic,
            _ => throw new Exception("Not a PoolType"),
        };

        public static bool IsSkill(this ProfessionFocus focus) => focus switch
        {
            var _ when focus.IsPool() => false,
            var _ when focus.IsStat() => false,
            _ => true
        };

        public static SkillType GetSkill(this ProfessionFocus focus) => (SkillType)focus;
    }
}