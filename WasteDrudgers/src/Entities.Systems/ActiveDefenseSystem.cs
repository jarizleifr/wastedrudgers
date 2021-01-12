namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void ActiveDefenseSystem(IContext ctx, World world)
        {
            var (attacks, defenses) = world.ecs.Pools<Attack, Defense>();
            foreach (var e in world.ecs.View<Turn, Attack, Defense>())
            {
                var attack = attacks[e];
                ref var defense = ref defenses[e];
                defense.parry = attack.parry;
            }
        }
    }
}