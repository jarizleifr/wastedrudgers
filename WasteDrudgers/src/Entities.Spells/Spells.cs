using ManulECS;

namespace WasteDrudgers.Entities
{
    public static class Spells
    {
        public static void CastSpellOn(World world, Entity? caster, Entity target, string spellId)
        {
            var rawSpell = Data.GetSpell(spellId);
            if (Spells.IsIncremental(rawSpell.Effect))
            {
                ApplyIncrementalEffect(world, caster, target, rawSpell);
            }
            else
            {
                ApplySpellEffect(world, target, rawSpell);
            }
        }

        // Incremental effects like poison, disease, stun
        public static void ApplyIncrementalEffect(World world, Entity? caster, Entity target, DBSpell rawSpell)
        {
            var pos = world.ecs.GetRef<Position>(target);
            world.WriteToLog(rawSpell.Message, pos.coords);

            var level = RNG.Extent(rawSpell.Magnitude);
            if (TryGetActiveEffect(world, target, rawSpell.Effect, out Entity activeEffect))
            {
                ref var effect = ref world.ecs.GetRef<ActiveEffect>(activeEffect);
                effect.magnitude += level;
            }
            else
            {
                var effectEntity = world.ecs.Create();
                world.ecs.Assign(effectEntity, new ActiveEffect
                {
                    target = target,
                    effect = rawSpell.Effect,
                    duration = -1,
                    magnitude = level
                });

                if (caster.HasValue && world.ecs.Has<Player>(caster.Value))
                {
                    world.ecs.Assign(effectEntity, new PlayerInitiated { });
                }

                if (world.ecs.Has<Player>(target))
                {
                    world.ecs.Assign(effectEntity, new PlayerMarker { });
                }
            }
        }

        // Fire-and-forget spell effects
        public static void ApplySpellEffect(World world, Entity target, DBSpell rawSpell)
        {
            var playerData = world.PlayerData;
            ref var stats = ref world.ecs.GetRef<Stats>(target);
            ref var health = ref world.ecs.GetRef<Health>(target);
            var message = rawSpell.Message ?? "";
            var spellStrength = RNG.Extent(rawSpell.Magnitude);
            switch (rawSpell.Effect)
            {
                case SpellEffect.StrengthPermanent:
                    stats.strength.Base += spellStrength;
                    break;
                case SpellEffect.EndurancePermanent:
                    stats.endurance.Base += spellStrength;
                    break;
                case SpellEffect.FinessePermanent:
                    stats.finesse.Base += spellStrength;
                    break;
                case SpellEffect.IntellectPermanent:
                    stats.intellect.Base += spellStrength;
                    break;
                case SpellEffect.ResolvePermanent:
                    stats.resolve.Base += spellStrength;
                    break;
                case SpellEffect.AwarenessPermanent:
                    stats.awareness.Base += spellStrength;
                    break;
                case SpellEffect.Identify:
                    Items.IdentifyInventory(world);
                    break;
                case SpellEffect.HealthHeal:
                    if (health.health.Damage > 0)
                    {
                        health.health.Damage -= spellStrength;
                    }
                    else
                    {
                        message = "nothing_happens";
                    }
                    break;
                case SpellEffect.VigorHeal:
                    if (health.vigor.Damage > 0)
                    {
                        health.vigor.Damage -= spellStrength;
                    }
                    else
                    {
                        message = "nothing_happens";
                    }
                    break;
            }
            if (message != "")
            {
                world.WriteToLog(message, playerData.coords);
            }
        }

        public static bool TryGetActiveEffect(World world, Entity target, SpellEffect spellEffect, out Entity activeEffect)
        {
            bool found = false;
            Entity tempEntity = default(Entity);
            foreach (var e in world.ecs.View<ActiveEffect>())
            {
                ref var active = ref world.ecs.GetRef<ActiveEffect>(e);
                if (found) break;

                if (active.target == target)
                {
                    tempEntity = e;
                    found = true;
                    break;
                }
            }

            activeEffect = tempEntity;
            return found;
        }

        public static bool IsFireAndForget(SpellEffect spellEffect)
        {
            switch (spellEffect)
            {
                case SpellEffect.StrengthPermanent:
                case SpellEffect.EndurancePermanent:
                case SpellEffect.FinessePermanent:
                case SpellEffect.IntellectPermanent:
                case SpellEffect.ResolvePermanent:
                case SpellEffect.AwarenessPermanent:
                case SpellEffect.Identify:
                case SpellEffect.HealthHeal:
                case SpellEffect.VigorHeal:
                    return true;
                default: return false;
            }
        }

        public static bool IsIncremental(SpellEffect spellEffect)
        {
            switch (spellEffect)
            {
                case SpellEffect.InflictPoison:
                    return true;
                default: return false;
            }
        }
    }
}