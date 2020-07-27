using System.Collections.Generic;
using System.Linq;

using WasteDrudgers.Data;

namespace WasteDrudgers.Entities
{
    public static class ComponentUtils
    {
        public static Skills CreateSkillsFromProfessions(DBCreature creature, int finesse, int intellect)
        {
            var skills = new Skills { set = new List<Skill>() };

            foreach (var p in creature.Professions)
            {
                var finesseSkills = p.Item1.Where(s => s.IsFinesseSkill()).ToList();
                var knowledgeSkills = p.Item1.Where(s => s.IsKnowledgeSkill()).ToList();

                for (int i = 0; i < p.Item2; i++)
                {
                    var finessePoints = finesse;
                    while (finessePoints > 0)
                    {
                        skills.Increment(finesseSkills[RNG.Int(finesseSkills.Count)]);
                        finessePoints--;
                    }

                    var intellectPoints = intellect;
                    while (intellectPoints > 0)
                    {
                        skills.Increment(finesseSkills[RNG.Int(knowledgeSkills.Count)]);
                        finessePoints--;
                        intellectPoints--;
                    }
                }
            }
            return skills;
        }

        public static int GetLevel(DBCreature creature) => creature.Professions
            .Select((p) => p.Item2)
            .Aggregate((a, c) => a + c);

        public static int GetExperience(int level, Stats stats, Skills skills) => skills.set.
            Select((s) => GetSkillWeight(s))
            .Aggregate((a, c) => a + c);

        private static int GetSkillWeight(Skill skill)
        {
            float v = skill.value;
            switch (skill.type)
            {
                case SkillType.LongBlade:
                case SkillType.AxeMace:
                case SkillType.Polearm:
                case SkillType.Shield:
                default:
                    v = v switch
                    {
                        var _ when v < 25 => v * 0.6f,  // Untrained
                        var _ when v < 50 => v * 0.8f,  // Novice
                        var _ when v < 75 => v * 1.0f,  // Regular
                        var _ when v < 100 => v * 1.2f, // Expert
                        _ => v * 1.4f                   // Master
                    };
                    break;

                case SkillType.ShortBlade:
                case SkillType.Fencing:
                case SkillType.Whip:
                case SkillType.MartialArts:
                case SkillType.Dodge:
                case SkillType.Observation:
                    v = v switch
                    {
                        var _ when v < 25 => v * 0.5f,   // Untrained
                        var _ when v < 50 => v * 0.75f,  // Novice
                        var _ when v < 75 => v * 1.0f,   // Regular
                        var _ when v < 100 => v * 1.25f, // Expert
                        _ => v * 1.5f                    // Master
                    };
                    break;
            }
            return (int)v;
        }
    }
}