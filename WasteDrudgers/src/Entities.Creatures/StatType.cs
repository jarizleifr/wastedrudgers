namespace WasteDrudgers.Entities
{
    public enum StatType
    {
        Strength,
        Endurance,
        Finesse,
        Intellect,
        Resolve,
        Awareness,
    }

    public static class StatTypeExtensions
    {
        public static string Abbr(this StatType type) => type switch
        {
            StatType.Strength => Locale.strengthAbbr,
            StatType.Endurance => Locale.enduranceAbbr,
            StatType.Finesse => Locale.finesseAbbr,
            StatType.Intellect => Locale.intellectAbbr,
            StatType.Resolve => Locale.resolveAbbr,
            StatType.Awareness => Locale.awarenessAbbr,
            _ => throw new System.Exception("Invalid StatType")
        };
    }
}