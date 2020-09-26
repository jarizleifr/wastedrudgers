using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AttackSystem(IContext ctx, World world)
        {
            var playerData = world.ecs.FetchResource<PlayerData>();
            world.ecs.Loop<Actor, IntentionAttack, Combat>((Entity entity, ref Actor actor, ref IntentionAttack intent, ref Combat attacker) =>
            {
                if (intent.attacker == playerData.entity)
                {
                    playerData.lastTarget = intent.target;
                }

                var defender = world.ecs.GetRef<Combat>(intent.target);

                var result = RNG.OpposingCheck(attacker.hitChance, defender.dodge);
                (var damage, var message) = result.Item1 switch
                {
                    Attempt.Critical => (attacker.CriticalDamage, "hit_critical"),
                    Attempt.Special => (attacker.SpecialDamage, "hit_special"),
                    Attempt.Success => (attacker.Damage, "hit"),
                    Attempt.Fumble => (0, "miss_critical"),
                    Attempt.Failure => (0, "miss")
                };

                var pos = world.ecs.GetRef<Position>(intent.target);
                if (result.Item1 == Attempt.Failure || result.Item1 == Attempt.Fumble)
                {
                    world.WriteToLog(message, pos.coords, LogItem.Actor(entity), LogItem.Actor(intent.target));
                }
                else if (result.Item1 != Attempt.Critical && damage - defender.armor <= 0)
                {
                    world.WriteToLog("hit_no_damage", pos.coords, LogItem.Actor(entity), LogItem.Actor(intent.target));
                }
                else
                {
                    if (result.Item1 != Attempt.Critical)
                    {
                        damage -= defender.armor;
                    }
                    world.WriteToLog(message, pos.coords, LogItem.Actor(entity), LogItem.Actor(intent.target), LogItem.Num(damage));
                    Effects.Create(world, pos.coords);

                    var damageEntity = world.ecs.Create();
                    world.ecs.Assign(damageEntity, new Damage { target = intent.target, damage = damage });

                    // If attack was initiated by player, track it in damage entity
                    if (entity == playerData.entity)
                    {
                        world.ecs.Assign(damageEntity, new PlayerInitiated { });
                    }

                    if (world.ecs.TryGet(entity, out CastOnStrike castOnStrike))
                    {
                        Spells.CastSpellOn(world, entity, intent.target, castOnStrike.spellId);
                    }
                }
                world.ecs.Assign<EventActed>(entity, new EventActed { energyLoss = 1000, nutritionLoss = 3 });
            });
        }
    }
}