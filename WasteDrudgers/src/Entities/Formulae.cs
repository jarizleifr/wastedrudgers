using System;

namespace WasteDrudgers.Entities
{
    public static class Formulae
    {
        private const int BASE_ATTRIBUTE_VALUE = 10;
        private const int BASE_EXPERIENCE = 10;

        private const int BASE_FINESSE_POINTS = 5;
        private const int BASE_KNOWLEDGE_POINTS = 3;

        public static int Vigor(int level, Stats stats) => BaseVigor(stats) + VigorPerLevel(stats) * level;

        public static int BaseVigor(Stats stats) => Math.Max(1, (stats.endurance + stats.resolve) / 2);
        public static int VigorPerLevel(Stats stats) => Math.Max(1, stats.resolve / 5);

        public static float VigorHealingRate(Stats stats)
        {
            var baseRegen = 500 - ((stats.endurance + stats.resolve) * 10);
            return 1f / baseRegen;
        }

        public static int Health(int level, Stats stats) => BaseHealth(stats) + HealthPerLevel(stats) * level;

        public static int BaseHealth(Stats stats) => Math.Max(1, (stats.strength + stats.endurance) / 2);
        public static int HealthPerLevel(Stats stats) => Math.Max(1, stats.endurance / 5);

        public static int MeleeDamage(Stats stats) => stats.strength - BASE_ATTRIBUTE_VALUE;

        public static int Speed(Stats stats) => 100 + (stats.finesse - BASE_ATTRIBUTE_VALUE);

        public static int Fortitude(Stats stats) => (stats.endurance * 5 + stats.strength * 5) / 2;

        // Skill base
        public static int BaseSkill(SkillType type, Stats stats) => type switch
        {
            SkillType.MartialArts => Easy(stats.strength),
            SkillType.ShortBlade => Easy(stats.finesse),
            SkillType.LongBlade => Average(stats.strength),
            SkillType.AxeMace => Easy(stats.strength),
            SkillType.Polearm => Average(stats.strength),
            SkillType.Fencing => Hard(stats.finesse),
            SkillType.Whip => Hard(stats.finesse),

            SkillType.Shield => Average(stats.endurance),
            SkillType.Observation => Average(stats.awareness),
            _ => 0
        };

        private static int Easy(int governingStat) => governingStat * 3;
        private static int Average(int governingStat) => governingStat * 2;
        private static int Hard(int governingStat) => governingStat * 1;

        public static int ExperienceNeededForLevel(int level) => (level * (level - 1) / 2) * BASE_EXPERIENCE;

        public static int GetExperienceValue(int characterPoints) =>
            Math.Max(10, characterPoints / 100);

        public static int GetStatCost(StatType type) => type switch
        {
            StatType.Strength => 10,
            StatType.Endurance => 10,
            StatType.Finesse => 20,
            StatType.Intellect => 20,
            StatType.Resolve => 10,
            StatType.Awareness => 10,
            _ => 5
        };
    }
}