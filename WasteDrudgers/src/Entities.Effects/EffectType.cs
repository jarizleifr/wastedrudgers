namespace WasteDrudgers.Entities
{
    public enum EffectProcType
    {
        Once,
        Repeated,
        Sustained,
    }

    public enum EffectType
    {
        PermanentStrength,
        PermanentEndurance,
        PermanentFinesse,
        PermanentIntellect,
        PermanentResolve,
        PermanentAwareness,
        Identify,
        HealVigor,
        HealHealth,
        InflictPoison,
        ModArmor,
        MeleeHitChanceMod,
        MeleeHitChanceModShortBlade,
        SizeMod,
    }

    public static class EffectTypeExtensions
    {
        public static EffectProcType GetProcType(this EffectType type)
        {
            switch (type)
            {
                case EffectType.PermanentStrength:
                case EffectType.PermanentEndurance:
                case EffectType.PermanentFinesse:
                case EffectType.PermanentIntellect:
                case EffectType.PermanentResolve:
                case EffectType.PermanentAwareness:
                case EffectType.Identify:
                    return EffectProcType.Once;
                case EffectType.HealHealth:
                case EffectType.HealVigor:
                    return EffectProcType.Repeated;
                case EffectType.MeleeHitChanceMod:
                case EffectType.MeleeHitChanceModShortBlade:
                case EffectType.SizeMod:
                case EffectType.ModArmor:
                    return EffectProcType.Sustained;
            }
            return EffectProcType.Once;
        }

        public static bool IsFireAndForget(this EffectType type)
        {
            switch (type)
            {
                case EffectType.PermanentStrength:
                case EffectType.PermanentEndurance:
                case EffectType.PermanentFinesse:
                case EffectType.PermanentIntellect:
                case EffectType.PermanentResolve:
                case EffectType.PermanentAwareness:
                case EffectType.Identify:
                case EffectType.HealHealth:
                case EffectType.HealVigor:
                    return true;
            }
            return false;
        }

        public static bool IsIncremental(this EffectType type)
        {
            switch (type)
            {
                case EffectType.InflictPoison:
                    return true;
            }
            return false;
        }
    }
}