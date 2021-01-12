using System;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AttackSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            var (intents, attacks, defenses, positions) =
                world.ecs.Pools<IntentionAttack, Attack, Defense, Position>();

            foreach (var e in world.ecs.View<Actor, IntentionAttack, Attack>())
            {
                ref var intent = ref intents[e];

                if (intent.attacker == playerData.entity)
                {
                    playerData.lastTarget = intent.target;
                }

                ref var attack = ref attacks[e];
                ref var defense = ref defenses[intent.target];

                var isParrying = (defense.parry >= defense.evasion);
                var result = RNG.OpposingCheck(attack.hitChance, isParrying ? defense.parry : defense.evasion);
                // Each subsequent parry reduces ability to parry incoming attacks by 20%
                if (isParrying)
                {
                    defense.parry = Math.Max(0, (int)(defense.parry * 0.8f));
                }

                (var damage, var message) = result.Item1 switch
                {
                    Attempt.Critical => (attack.CriticalDamage, "hit_critical"),
                    Attempt.Special => (attack.SpecialDamage, "hit_special"),
                    Attempt.Success => (attack.Damage, "hit"),
                    Attempt.Fumble => (0, "miss_critical"),
                    Attempt.Failure => (0, "miss")
                };

                var pos = positions[intent.target];
                if (result.Item1 == Attempt.Failure || result.Item1 == Attempt.Fumble)
                {
                    world.WriteToLog(message, pos.coords, LogArgs.Actor(e), LogArgs.Actor(intent.target));
                }
                else if (result.Item1 != Attempt.Critical && damage - defense.armor <= 0)
                {
                    world.WriteToLog("hit_no_damage", pos.coords, LogArgs.Actor(e), LogArgs.Actor(intent.target));
                }
                else
                {
                    if (result.Item1 != Attempt.Critical)
                    {
                        damage -= defense.armor;
                    }
                    world.WriteToLog(message, pos.coords, LogArgs.Actor(e), LogArgs.Actor(intent.target), LogArgs.Num(damage));
                    VisualEffects.Create(world, pos.coords);

                    var damageEntity = world.ecs.Create();
                    world.ecs.Assign(damageEntity, new Damage { target = intent.target, damage = damage });

                    // If attack was initiated by player, track it in damage entity
                    if (e == playerData.entity)
                    {
                        world.ecs.Assign<PlayerInitiated>(damageEntity);
                    }

                    if (world.ecs.TryGet(e, out CastOnStrike castOnStrike))
                    {
                        Effects.CastSpell(world, intent.target, castOnStrike.spellId, e);
                    }
                }
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 3 });
            }
        }
    }
}