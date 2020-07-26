namespace WasteDrudgers.Entities
{
    public struct Skill
    {
        public SkillType type;
        public int value;

        public override bool Equals(object obj) => !(obj is Skill s) ? false : this.type == s.type;
        public override int GetHashCode() => type.GetHashCode();
    }
}