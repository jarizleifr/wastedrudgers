using System;
using System.Collections.Generic;
using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Effects
    {
        public static void CastSpell(World world, Entity target, string spellId, Entity? caster)
        {
            var rawSpell = Data.GetSpell(spellId);
            foreach (var effect in rawSpell.Effects)
            {
                if (effect.type.GetProcType() == EffectProcType.Once)
                {
                    EffectRules.ApplyEffect(world, target, effect.type, RNG.Extent(effect.Magnitude));
                }
                else
                {
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

                    if (effect.Duration > 1)
                    {
                        world.ecs.Assign(spellEntity, new Duration
                        {
                            duration = effect.Duration,
                        });
                    }

                    if (caster.HasValue && world.ecs.Has<Player>(caster.Value))
                    {
                        world.ecs.Assign<PlayerInitiated>(spellEntity);
                    }

                    if (world.ecs.Has<Player>(target))
                    {
                        world.ecs.Assign<PlayerMarker>(spellEntity);
                    }
                }
            }

            var pos = world.ecs.GetRef<Position>(target);
            world.WriteToLog(rawSpell.Message, pos.coords);

            world.ecs.Assign<EventStatsUpdated>(target);
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