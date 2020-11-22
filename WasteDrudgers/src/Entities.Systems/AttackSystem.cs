namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AttackSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;
            foreach (var e in world.ecs.View<Actor, IntentionAttack, Combat>())
            {
                ref var actor = ref world.ecs.GetRef<Actor>(e);
                ref var intent = ref world.ecs.GetRef<IntentionAttack>(e);
                ref var attacker = ref world.ecs.GetRef<Combat>(e);

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
                    world.WriteToLog(message, pos.coords, LogItem.Actor(e), LogItem.Actor(intent.target));
                }
                else if (result.Item1 != Attempt.Critical && damage - defender.armor <= 0)
                {
                    world.WriteToLog("hit_no_damage", pos.coords, LogItem.Actor(e), LogItem.Actor(intent.target));
                }
                else
                {
                    if (result.Item1 != Attempt.Critical)
                    {
                        damage -= defender.armor;
                    }
                    world.WriteToLog(message, pos.coords, LogItem.Actor(e), LogItem.Actor(intent.target), LogItem.Num(damage));
                    Effects.Create(world, pos.coords);

                    var damageEntity = world.ecs.Create();
                    world.ecs.Assign(damageEntity, new Damage { target = intent.target, damage = damage });

                    // If attack was initiated by player, track it in damage entity
                    if (e == playerData.entity)
                    {
                        world.ecs.Assign(damageEntity, new PlayerInitiated { });
                    }

                    if (world.ecs.TryGet(e, out CastOnStrike castOnStrike))
                    {
                        Spells.CastSpellOn(world, e, intent.target, castOnStrike.spellId);
                    }
                }
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 3 });
            }
        }
    }
}