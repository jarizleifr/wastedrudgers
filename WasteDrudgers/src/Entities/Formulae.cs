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
            SkillType.MartialArts => 25,
            SkillType.Dodge => stats.finesse * 2,
            _ => 0
        };

        public static int ExperienceNeededForLevel(int level) => (level * (level - 1) / 2) * BASE_EXPERIENCE;

        public static int FinessePointsPerLevel(int finesse) => Math.Max(1, BASE_FINESSE_POINTS + (finesse - BASE_ATTRIBUTE_VALUE) / 2);
        public static int KnowledgePointsPerLevel(int intellect) => Math.Max(1, BASE_KNOWLEDGE_POINTS + (intellect - BASE_ATTRIBUTE_VALUE) / 2);
    }
}