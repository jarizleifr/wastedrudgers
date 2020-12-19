using System.Collections.Generic;
using WasteDrudgers.Entities;

namespace WasteDrudgers.UI
{
    public class ChargenData
    {
        public Stats stats = new Stats { };
        public Skills skills = new Skills { set = new List<Skill>() };

        public int statsSpent;
        public int skillsSpent;
    }
}