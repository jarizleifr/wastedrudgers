namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void RegenSystem(IContext ctx, World world)
        {
            var (pools, stats) = world.ecs.Pools<Pools, Stats>();
            foreach (var e in world.ecs.View<Pools, Stats>())
            {
                ref var health = ref pools[e];
                ref var st = ref stats[e];

                if (health.fatigued) continue;

                if (health.vigor.Damage > 0)
                {
                    health.vigorRegen += Formulae.VigorHealingRate(st);
                    if (health.vigorRegen >= 1)
                    {
                        health.vigorRegen -= 1;
                        health.vigor.Damage--;
                    }
                }
            }
        }
    }
}
