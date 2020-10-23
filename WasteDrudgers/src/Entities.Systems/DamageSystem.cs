using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void DamageSystem(IContext ctx, World world)
        {
            world.ecs.Loop((Entity entity, ref Damage damage) =>
            {
                if (world.ecs.Has<Death>(damage.target)) return;

                // FIXME: We've ended up here without a Position component
                var pos = world.ecs.GetRef<Position>(damage.target);
                ref var hlt = ref world.ecs.GetRef<Health>(damage.target);

                // Only physical damage targets vigor
                if (hlt.vigor.Current > 0 && damage.damageType == DamageType.Physical)
                {
                    hlt.vigor.Damage += damage.damage;
                    // If damage is greater than max vigor and max health combined, just kill the entity
                    if (damage.damage >= hlt.vigor.Max + hlt.health.Max)
                    {
                        if (world.ecs.Has<PlayerInitiated>(entity))
                        {
                            var playerData = world.PlayerData;
                            Creatures.AwardKillExperience(world, playerData.entity, damage.target);
                        }

                        world.WriteToLog("death_instant", pos.coords, LogItem.Actor(damage.target));
                        Creatures.KillCreature(world, damage.target);
                    }
                }
                else
                {
                    hlt.health.Damage += damage.damage;
                    if (hlt.health.Current <= 0)
                    {
                        if (world.ecs.Has<PlayerInitiated>(entity))
                        {
                            var playerData = world.PlayerData;
                            Creatures.AwardKillExperience(world, playerData.entity, damage.target);
                        }

                        switch (damage.damageType)
                        {
                            case DamageType.Poison:
                                world.WriteToLog("death_affliction", pos.coords, LogItem.Actor(damage.target));
                                break;
                            case DamageType.Physical:
                                world.WriteToLog("death", pos.coords, LogItem.Actor(damage.target));
                                break;
                        }

                        Creatures.KillCreature(world, damage.target);
                    }
                }
                world.ecs.Remove(entity);
            });
        }
    }
}