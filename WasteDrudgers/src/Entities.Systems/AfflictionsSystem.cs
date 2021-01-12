namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AfflictionsSystem(IContext ctx, World world)
        {
            if (world.GameTicks % 10 == 0)
            {
                var (afflictions, stats, positions) = world.ecs.Pools<Afflictions, Stats, Position>();
                foreach (var e in world.ecs.View<Afflictions, Stats, Position>())
                {
                    ref var a = ref afflictions[e];

                    var pos = positions[e];
                    var fortitude = Formulae.Fortitude(stats[e]);

                    if (RNG.ResistanceCheck(fortitude, a.poison) < Attempt.Success)
                    {
                        var damage = RNG.IntInclusive(1, 3);
                        var damageEntity = world.ecs.Create();
                        world.WriteToLog("status_poison_damage", pos.coords, LogArgs.Actor(e), LogArgs.Num(damage));
                        world.ecs.Assign(damageEntity, new Damage { target = e, damage = damage, damageType = DamageType.Poison });
                        if (world.ecs.Has<PlayerInitiated>(e))
                        {
                            world.ecs.Assign<PlayerInitiated>(damageEntity);
                        }
                    }

                    if (--a.poison == 0)
                    {
                        world.WriteToLog("status_affliction_healed", pos.coords);
                        world.ecs.Remove<Afflictions>(e);
                    }
                }
            }
        }
    }
}