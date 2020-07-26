using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void RegenSystem(IContext ctx, World world)
        {
            world.ecs.Loop((Entity entity, ref Health health, ref Stats stats) =>
            {
                if (health.vigor.Damage > 0)
                {
                    health.vigorRegen += Formulae.VigorHealingRate(stats);
                    if (health.vigorRegen >= 1)
                    {
                        health.vigorRegen -= 1;
                        health.vigor.Damage--;
                    }
                }
            });
        }
    }
}