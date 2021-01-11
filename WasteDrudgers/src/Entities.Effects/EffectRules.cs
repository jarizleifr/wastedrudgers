using System;
using System.Collections.Generic;
using ManulECS;
using WasteDrudgers.Common;

namespace WasteDrudgers.Entities
{
    ///<summary>
    /// Contains all spell effect logic, i.e. what stats are changed.
    ///</summary>
    public static class EffectRules
    {
        private static Dictionary<EffectType, Action<World, Entity, int>> actions;

        static EffectRules()
        {
            actions = new Dictionary<EffectType, Action<World, Entity, int>>();

            actions.Add(EffectType.PermanentStrength, (w, e, i) =>
            {
                ref var stats = ref w.ecs.GetRef<Stats>(e);
                stats.strength.Base += i;
            });

            actions.Add(EffectType.PermanentEndurance, (w, e, i) =>
            {
                ref var stats = ref w.ecs.GetRef<Stats>(e);
                stats.endurance.Base += i;
            });

            actions.Add(EffectType.PermanentFinesse, (w, e, i) =>
            {
                ref var stats = ref w.ecs.GetRef<Stats>(e);
                stats.finesse.Base += i;
            });

            actions.Add(EffectType.PermanentIntellect, (w, e, i) =>
            {
                ref var stats = ref w.ecs.GetRef<Stats>(e);
                stats.intellect.Base += i;
            });

            actions.Add(EffectType.PermanentResolve, (w, e, i) =>
            {
                ref var stats = ref w.ecs.GetRef<Stats>(e);
                stats.resolve.Base += i;
            });

            actions.Add(EffectType.PermanentAwareness, (w, e, i) =>
            {
                ref var stats = ref w.ecs.GetRef<Stats>(e);
                stats.awareness.Base += i;
            });

            actions.Add(EffectType.Identify, (w, e, i) =>
                Items.IdentifyInventory(w));

            actions.Add(EffectType.HealVigor, (w, e, i) =>
            {
                ref var pools = ref w.ecs.GetRef<Pools>(e);
                pools.vigor.Damage -= i;
            });

            actions.Add(EffectType.HealHealth, (w, e, i) =>
            {
                ref var pools = ref w.ecs.GetRef<Pools>(e);
                pools.health.Damage -= i;
            });

            actions.Add(EffectType.ModArmor, (w, e, i) =>
            {
                ref var combat = ref w.ecs.GetRef<Combat>(e);
                combat.armor += i;
            });

            actions.Add(EffectType.MeleeHitChanceMod, (w, e, i) =>
            {
                ref var combat = ref w.ecs.GetRef<Combat>(e);
                combat.hitChance += i;
            });

            actions.Add(EffectType.SizeMod, (w, e, i) =>
            {
                ref var combat = ref w.ecs.GetRef<Combat>(e);
                combat.damage = new Extent(
                    Math.Max(1, combat.damage.min + i),
                    Math.Max(2, combat.damage.max + i)
                );
                combat.dodge -= i * 5;
            });

            actions.Add(EffectType.InflictPoison, (w, e, i) =>
            {
                if (w.ecs.Has<Afflictions>(e))
                {
                    ref var affl = ref w.ecs.GetRef<Afflictions>(e);
                    affl.poison = (byte)(affl.poison + i > 255
                        ? 255
                        : affl.poison + i
                    );
                }
                else
                {
                    w.ecs.Assign<Afflictions>(e, new Afflictions { poison = (byte)i });
                }
            });
        }

        public static void ApplyEffect(World world, Entity entity, EffectType type, int value)
        {
            if (actions.ContainsKey(type))
            {
                actions[type].Invoke(world, entity, value);
            }
        }
    }
}
