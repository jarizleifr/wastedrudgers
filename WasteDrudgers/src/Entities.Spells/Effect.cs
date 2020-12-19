namespace WasteDrudgers.Entities
{
    public struct Effect
    {
        public string Id { get; set; }
        public EffectType Type { get; set; }
        public int Power { get; set; }
    }
}