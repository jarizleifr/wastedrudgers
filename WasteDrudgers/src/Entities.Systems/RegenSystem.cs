namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void RegenSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Health, Stats>())
            {
                ref var health = ref world.ecs.GetRef<Health>(e);
                ref var stats = ref world.ecs.GetRef<Stats>(e);

                if (health.fatigued) continue;

                if (health.vigor.Damage > 0)
                {
                    health.vigorRegen += Formulae.VigorHealingRate(stats);
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
