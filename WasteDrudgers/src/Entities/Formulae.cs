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

        public static int Evasion(Stats stats) => (stats.finesse * 5 + stats.awareness * 5) / 2;
        public static int Fortitude(Stats stats) => (stats.endurance * 5 + stats.strength * 5) / 2;
        public static int Mental(Stats stats) => (stats.intellect * 5 + stats.resolve * 5) / 2;

        // Skill base
        public static int BaseSkill(SkillType type, Stats stats)
        {
            var stat = stats[GetGoverningStat(type)];
            switch (type)
            {
                // Easy skills
                case SkillType.MartialArts:
                case SkillType.ShortBlade:
                case SkillType.AxeMace:
                    return BaseSkill(SkillDifficulty.Easy, stat);

                // Average skills
                case SkillType.LongBlade:
                case SkillType.Polearm:
                case SkillType.Shield:
                case SkillType.Observation:
                    return BaseSkill(SkillDifficulty.Hard, stat);

                // Hard skills
                case SkillType.Fencing:
                case SkillType.Whip:
                    return BaseSkill(SkillDifficulty.Hard, stat);
            }
            return 0;
        }

        private static int BaseSkill(SkillDifficulty diff, int stat) => diff switch
        {
            SkillDifficulty.Easy => stat * 3,
            SkillDifficulty.Average => stat * 2,
            SkillDifficulty.Hard => stat * 1,
            _ => 0
        };

        public static int ExperienceNeededForLevel(int level) => (level * (level - 1) / 2) * BASE_EXPERIENCE;

        public static int GetExperienceValue(int characterPoints) =>
            Math.Max(10, characterPoints / 100);

        public static SkillDifficulty GetSkillDifficulty(SkillType type) => type switch
        {
            SkillType.MartialArts => SkillDifficulty.Easy,
            SkillType.ShortBlade => SkillDifficulty.Easy,
            SkillType.LongBlade => SkillDifficulty.Average,
            SkillType.AxeMace => SkillDifficulty.Easy,
            SkillType.Polearm => SkillDifficulty.Average,
            SkillType.Fencing => SkillDifficulty.Hard,
            SkillType.Whip => SkillDifficulty.Hard,

            SkillType.Shield => SkillDifficulty.Average,
            SkillType.Observation => SkillDifficulty.Average,

            _ => SkillDifficulty.VeryHard
        };

        public static StatType GetGoverningStat(SkillType type) => type switch
        {
            SkillType.MartialArts => StatType.Strength,
            SkillType.ShortBlade => StatType.Finesse,
            SkillType.LongBlade => StatType.Strength,
            SkillType.AxeMace => StatType.Strength,
            SkillType.Polearm => StatType.Strength,
            SkillType.Fencing => StatType.Finesse,
            SkillType.Whip => StatType.Finesse,

            SkillType.Shield => StatType.Endurance,
            SkillType.Observation => StatType.Awareness,

            _ => StatType.Finesse
        };

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