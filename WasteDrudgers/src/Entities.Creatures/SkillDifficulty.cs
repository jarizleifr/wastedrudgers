namespace WasteDrudgers.Entities
{
    public enum SkillDifficulty
    {
        Easy,
        Average,
        Hard,
        VeryHard,
    }

    public static class SkillDifficultyExtensions
    {
        public static string Abbr(this SkillDifficulty type) => type switch
        {
            SkillDifficulty.Easy => Locale.easyAbbr,
            SkillDifficulty.Average => Locale.averageAbbr,
            SkillDifficulty.Hard => Locale.hardAbbr,
            SkillDifficulty.VeryHard => Locale.veryHardAbbr,
            _ => throw new System.Exception("Invalid SkillDifficulty")
        };
    }
}