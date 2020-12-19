using ManulECS;

namespace WasteDrudgers.Entities
{
    public static class Spells
    {
        // Get rid of spell specific stuff from effect application
        // caster is only needed for spells
        // for potions, talents, have separate function

        // for potions thrown on others, wands etc. use cast spell, since they have "caster" or "source"

        public static void CastSpellOn(World world, Entity? caster, Entity target, string spellId)
        {
            var rawSpell = Data.GetSpell(spellId);
            foreach (var effect in rawSpell.Effects)
            {
                if (effect.type.IsIncremental())
                {
                    ApplyIncrementalEffect(world, caster, target, effect);
                }
                else if (effect.type.IsFireAndForget())
                {
                    ApplySpellEffect(world, target, effect);
                }
                else
                {
                    ApplyDurationSpellEffect(world, caster, target, spellId, effect);
                }
            }
            var pos = world.ecs.GetRef<Position>(target);
            world.WriteToLog(rawSpell.Message, pos.coords);

            world.ecs.Assign<EventStatsUpdated>(target);
        }

        // Incremental effects like poison, disease, stun
        public static void ApplyIncrementalEffect(World world, Entity? caster, Entity target, DBEffect effect)
        {
            var level = RNG.Extent(effect.Magnitude);
            if (TryGetActiveEffect(world, target, effect.type, out Entity e))
            {
                ref var a = ref world.ecs.GetRef<ActiveEffect>(e);
                a.effect.Power += level;
            }
            else
            {
                var effectEntity = world.ecs.Create();
                world.ecs.Assign(effectEntity, new ActiveEffect
                {
                    target = target,
                    effect = new Effect
                    {
                        Type = effect.type,
                        Power = RNG.Extent(effect.Magnitude)
                    },
                });

                if (caster.HasValue && world.ecs.Has<Player>(caster.Value))
                {
                    world.ecs.Assign<PlayerInitiated>(effectEntity);
                }

                if (world.ecs.Has<Player>(target))
                {
                    world.ecs.Assign<PlayerMarker>(effectEntity);
                }
            }
        }

        // Fire-and-forget spell effects
        public static void ApplySpellEffect(World world, Entity target, DBEffect effect)
        {
            var playerData = world.PlayerData;
            ref var stats = ref world.ecs.GetRef<Stats>(target);
            ref var health = ref world.ecs.GetRef<Pools>(target);

            var spellStrength = RNG.Extent(effect.Magnitude);
            switch (effect.type)
            {
                case EffectType.PermanentStrength:
                    stats.strength.Base += spellStrength;
                    break;
                case EffectType.PermanentEndurance:
                    stats.endurance.Base += spellStrength;
                    break;
                case EffectType.PermanentFinesse:
                    stats.finesse.Base += spellStrength;
                    break;
                case EffectType.PermanentIntellect:
                    stats.intellect.Base += spellStrength;
                    break;
                case EffectType.PermanentResolve:
                    stats.resolve.Base += spellStrength;
                    break;
                case EffectType.PermanentAwareness:
                    stats.awareness.Base += spellStrength;
                    break;
                case EffectType.Identify:
                    Items.IdentifyInventory(world);
                    break;
                case EffectType.HealHealth:
                    health.health.Damage -= spellStrength;
                    break;
                case EffectType.HealVigor:
                    health.vigor.Damage -= spellStrength;
                    break;
            }
        }

        private static void ApplyDurationSpellEffect(World world, Entity? caster, Entity target, string spellId, DBEffect effect)
        {
            var level = RNG.Extent(effect.Magnitude);
            var spellEntity = world.ecs.Create();
            world.ecs.Assign(spellEntity, new ActiveEffect
            {
                target = target,
                effect = new Effect
                {
                    Type = effect.type,
                    Power = RNG.Extent(effect.Magnitude)
                },
            });
            world.ecs.Assign(spellEntity, new Duration
            {
                duration = effect.Duration,
            });

            if (caster.HasValue && world.ecs.Has<Player>(caster.Value))
            {
                world.ecs.Assign<PlayerInitiated>(spellEntity);
            }

            if (world.ecs.Has<Player>(target))
            {
                world.ecs.Assign<PlayerMarker>(spellEntity);
            }
        }

        public static bool TryGetActiveEffect(World world, Entity target, EffectType spellEffect, out Entity activeEffect)
        {
            foreach (var e in world.ecs.View<ActiveEffect>())
            {
                ref var active = ref world.ecs.GetRef<ActiveEffect>(e);

                if (active.target == target)
                {
                    activeEffect = e;
                    return true;
                }
            }

            activeEffect = default(Entity);
            return false;
        }
    }
}