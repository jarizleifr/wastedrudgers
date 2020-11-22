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
                world.ecs.Assign(e, new EventStatsUpdated { });
            }

            if (world.GameTicks % 10 == 0)
            {
                foreach (var e in world.ecs.View<ActiveEffect>())
                {
                    ref var activeEffect = ref world.ecs.GetRef<ActiveEffect>(e);

                    if (!world.ecs.IsAlive(activeEffect.target) || world.ecs.Has<Death>(activeEffect.target))
                    {
                        world.ecs.Remove(e);
                        continue;
                    }

                    var pos = world.ecs.GetRef<Position>(activeEffect.target);
                    if (Spells.IsIncremental(activeEffect.effect))
                    {
                        var stats = world.ecs.GetRef<Stats>(activeEffect.target);
                        var fortitude = Formulae.Fortitude(stats);
                        if (RNG.ResistanceCheck(fortitude, activeEffect.magnitude) < Attempt.Success)
                        {
                            var damage = RNG.IntInclusive(1, 3);
                            var damageEntity = world.ecs.Create();
                            world.WriteToLog("status_poison_damage", pos.coords, LogItem.Actor(activeEffect.target), LogItem.Num(damage));
                            world.ecs.Assign(damageEntity, new Damage { target = activeEffect.target, damage = damage, damageType = DamageType.Poison });
                            if (world.ecs.TryGet(e, out PlayerInitiated p))
                            {
                                world.ecs.Assign(damageEntity, p);
                            }
                        }
                        activeEffect.magnitude--;

                        if (activeEffect.magnitude == 0)
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