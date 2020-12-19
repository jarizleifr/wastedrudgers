namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void StatusSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Stats, HungerClock, EventStatusUpdated>())
            {
                ref var stats = ref world.ecs.GetRef<Stats>(e);
                ref var clock = ref world.ecs.GetRef<HungerClock>(e);

                if (clock.State == HungerState.Hungry)
                {
                    stats.SetMods(-2);
                }
                else
                {
                    stats.SetMods(0);
                }
                world.ecs.Assign<EventStatsUpdated>(e);
            }

            if (world.GameTicks % 10 == 0)
            {
                foreach (var e in world.ecs.View<ActiveEffect>())
                {
                    ref var a = ref world.ecs.GetRef<ActiveEffect>(e);

                    if (!world.ecs.IsAlive(a.target) || world.ecs.Has<Death>(a.target))
                    {
                        world.ecs.Remove(e);
                        continue;
                    }

                    if (a.effect.Type.IsIncremental())
                    {
                        var pos = world.ecs.GetRef<Position>(a.target);
                        var stats = world.ecs.GetRef<Stats>(a.target);
                        var fortitude = Formulae.Fortitude(stats);
                        if (RNG.ResistanceCheck(fortitude, a.effect.Power) < Attempt.Success)
                        {
                            var damage = RNG.IntInclusive(1, 3);
                            var damageEntity = world.ecs.Create();
                            world.WriteToLog("status_poison_damage", pos.coords, LogArgs.Actor(a.target), LogArgs.Num(damage));
                            world.ecs.Assign(damageEntity, new Damage { target = a.target, damage = damage, damageType = DamageType.Poison });
                            if (world.ecs.Has<PlayerInitiated>(e))
                            {
                                world.ecs.Assign<PlayerInitiated>(damageEntity);
                            }
                        }

                        if (--a.effect.Power == 0)
                        {
                            world.WriteToLog("status_affliction_healed", pos.coords);
                            world.ecs.Remove(e);
                        }
                    }
                }
            }
        }
    }
}