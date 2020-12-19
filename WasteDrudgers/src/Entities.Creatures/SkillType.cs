namespace WasteDrudgers.Entities
{
    public enum SkillType
    {
        MartialArts,
        ShortBlade,
        LongBlade,
        AxeMace,
        Polearm,
        Fencing,
        Whip,
        Shield,
        Dodge,
        Observation,
    }

    public static class SkillTypeExtensions
    {
        public static bool IsFinesseSkill(this SkillType type) => type switch
        {
            SkillType.MartialArts => true,
            SkillType.ShortBlade => true,
            SkillType.LongBlade => true,
            SkillType.AxeMace => true,
            SkillType.Polearm => true,
            SkillType.Fencing => true,
            SkillType.Whip => true,
            SkillType.Dodge => true,
            SkillType.Shield => true,
            _ => false
        };

        public static bool IsKnowledgeSkill(this SkillType type) => type switch
        {
            SkillType.Observation => true,
            _ => false,
        };
    }
}