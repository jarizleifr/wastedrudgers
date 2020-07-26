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
    }
}