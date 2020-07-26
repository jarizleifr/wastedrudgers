using ManulECS;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AttackSystem(IContext ctx, World world)
        {
            var playerData = world.ecs.FetchResource<PlayerData>();
            world.ecs.Loop<IntentionAttack, Combat>((Entity entity, ref IntentionAttack intent, ref Combat attacker) =>
            {
                if (world.ecs.IsAlive(intent.target))
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
                        Attempt.Special => (attacker.SpecialDamage, "hit"),
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
                        world.ecs.Assign<Damage>(damageEntity, new Damage { damage = damage, target = intent.target });

                        if (world.ecs.TryGet(entity, out CastOnStrike castOnStrike))
                        {
                            Spells.CastSpellOn(world, intent.target, castOnStrike.spellId);
                        }
                    }
                }
            });
            world.ecs.Clear<IntentionAttack>();
        }
    }
}