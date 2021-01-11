using System;
using System.Collections.Generic;
using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Effects
    {
        private enum AbilityType
        {
            Trait,
            Talent,
        }

        public static void AddTalent(World world, string talentId, Entity target) =>
            Add(world, talentId, target, AbilityType.Talent);

        public static void RemoveTalent(World world, string talentId, Entity target) =>
            Remove(world, talentId, target, AbilityType.Talent);

        public static bool HasTalent(World world, Entity owner, string talentId) =>
            Has(world, talentId, owner, AbilityType.Talent);

        public static List<string> GetOwnedTalentIds(World world, Entity owner) =>
            GetOwned(world, owner, AbilityType.Talent);

        public static void AddTrait(World world, string traitId, Entity target) =>
            Add(world, traitId, target, AbilityType.Trait);

        public static void RemoveTrait(World world, string traitId, Entity target) =>
            Remove(world, traitId, target, AbilityType.Trait);

        public static bool HasTrait(World world, Entity owner, string traitId) =>
            Has(world, traitId, owner, AbilityType.Trait);

        public static List<string> GetOwnedTraitIds(World world, Entity owner) =>
            GetOwned(world, owner, AbilityType.Trait);

        private static bool Has(World world, string id, Entity owner, AbilityType type)
        {
            var activeEffects = world.ecs.Pools<ActiveEffect>();
            var view = type == AbilityType.Trait
                ? world.ecs.View<ActiveEffect, IsTrait>()
                : world.ecs.View<ActiveEffect, IsTalent>();
            foreach (var e in view)
            {
                var a = activeEffects[e];
                if (a.target == owner && a.effect.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        private static void Add(World world, string id, Entity target, AbilityType type)
        {
            DBPermanentAbility ability = type == AbilityType.Trait
                ? Data.GetTrait(id)
                : Data.GetTalent(id);

            if (!Has(world, id, target, type))
            {
                foreach (var eff in ability.Effects)
                {
                    var entity = world.ecs.Create();
                    world.ecs.Assign<ActiveEffect>(entity, new ActiveEffect
                    {
                        target = target,
                        effect = new Effect
                        {
                            Id = ability.Id,
                            Type = eff.type,
                            // Permanent abilities have only one set value
                            Power = eff.Magnitude.min,
                        },
                    });
                    if (type == AbilityType.Trait)
                    {
                        world.ecs.Assign<IsTrait>(entity);
                    }
                    else
                    {
                        world.ecs.Assign<IsTalent>(entity);
                    }

                    if (target == world.PlayerData.entity)
                    {
                        world.ecs.Assign<PlayerMarker>(entity);
                    }
                }
            }
        }

        private static void Remove(World world, string id, Entity target, AbilityType type)
        {
            var activeEffects = world.ecs.Pools<ActiveEffect>();
            var view = type == AbilityType.Trait
                ? world.ecs.View<ActiveEffect, IsTrait>()
                : world.ecs.View<ActiveEffect, IsTalent>();
            foreach (var e in view)
            {
                var a = activeEffects[e];
                if (a.target == target && a.effect.Id == id)
                {
                    world.ecs.Remove(e);
                }
            }
        }

        private static List<string> GetOwned(World world, Entity owner, AbilityType type)
        {
            var list = new List<string>();
            var activeEffects = world.ecs.Pools<ActiveEffect>();
            var view = type == AbilityType.Trait
                ? world.ecs.View<ActiveEffect, IsTrait>()
                : world.ecs.View<ActiveEffect, IsTalent>();
            foreach (var e in view)
            {
                var a = activeEffects[e];
                if (a.target == owner)
                {
                    list.Add(a.effect.Id);
                }
            }
            return list;
        }
    }
}