using System;
using System.Collections.Generic;
using Blaggard.Common;
using ManulECS;

namespace WasteDrudgers.Entities
{
    public static class Creatures
    {
        public static Entity Create(World world, string id, Vec2 pos)
        {
            var creature = Data.GetCreature(id);
            return Create(world, creature, pos);
        }

        public static Entity Create(World world, DBCreature creature, Vec2 pos)
        {
            var entity = world.ecs.Create();

            world.ecs.Assign(entity, new Identity { name = creature.Name, rawName = creature.Id });
            world.ecs.Assign(entity, new Position { coords = pos });
            world.ecs.Assign(entity, new Renderable
            {
                character = creature.Char,
                color = creature.Color,
            });
            world.ecs.Assign<AI>(entity);

            var stats = new Stats
            {
                strength = creature.Strength,
                endurance = creature.Endurance,
                finesse = creature.Finesse,
                intellect = creature.Intellect,
                resolve = creature.Resolve,
                awareness = creature.Awareness
            };

            world.ecs.Assign(entity, stats);
            world.ecs.Assign(entity, new Skills { set = new List<Skill>() });
            // These are recalculated on creature stats update
            world.ecs.Assign(entity, new Actor { });
            world.ecs.Assign(entity, new Pools { });
            world.ecs.Assign(entity, new Attack { });
            world.ecs.Assign(entity, new Defense { });

            ComponentUtils.ApplyProfession(world, entity, creature);

            world.ecs.Assign(entity, new Experience
            {
                level = ComponentUtils.GetLevel(world, entity),
                experience = GetSpentCharacterPoints(world, entity) / 100
            });

            if (creature.NaturalAttack != null)
            {
                var rawNaturalAttack = Data.GetNaturalAttack(creature.NaturalAttack);
                var naturalAttack = new NaturalAttack
                {
                    baseSkill = rawNaturalAttack.BaseSkill,
                    damage = rawNaturalAttack.Damage,
                };
                if (rawNaturalAttack.CastOnStrike != null)
                {
                    naturalAttack.castOnStrike = rawNaturalAttack.CastOnStrike;
                }
                world.ecs.Assign(entity, naturalAttack);
            }

            world.spatial.SetCreature(pos, entity);
            Creatures.UpdateCreature(world, entity);

            return entity;
        }

        public static void KillCreature(World world, Entity entity)
        {
            var pos = world.ecs.GetRef<Position>(entity);
            world.spatial.ClearCreatureAt(pos.coords);
            world.ecs.Remove<Position>(entity);
            world.ecs.Assign<Death>(entity);
        }

        public static void AwardKillExperience(World world, Entity awardee, Entity killed)
        {
            AwardExperience(world, awardee, Formulae.GetExperienceValue(GetSpentCharacterPoints(world, killed)));
        }

        public static void AwardExperience(World world, Entity entity, int experience)
        {
            ref var exp = ref world.ecs.GetRef<Experience>(entity);
            exp.experience += experience;
        }

        public static int GetSpentCharacterPoints(World world, Entity entity)
        {
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            int characterPoints = 0;
            for (int i = 0; i < 6; i++)
            {
                var type = (StatType)i;
                characterPoints += stats[type].Base * Formulae.GetStatCost(type);
            }

            foreach (var s in skills.set)
            {
                characterPoints += s.value * 2;
            }

            return characterPoints;
        }

        public static void UpdateCreature(World world, Entity entity)
        {
            ref var stats = ref world.ecs.GetRef<Stats>(entity);
            ResetStats(ref stats);

            ref var attack = ref world.ecs.GetRef<Attack>(entity);
            ResetAttack(world, entity, ref attack, out var primary, out var hasShield);

            ref var defense = ref world.ecs.GetRef<Defense>(entity);
            ResetDefense(world, entity, ref defense);

            // Apply effects
            var activeEffects = world.ecs.Pools<ActiveEffect>();
            foreach (var e in world.ecs.View<ActiveEffect>())
            {
                var a = activeEffects[e];
                if (a.target == entity)
                {
                    EffectRules.ApplyEffect(world, entity, a.effect.Type, a.effect.Power);
                }
            }
            ApplyHunger(world, entity, ref stats);

            ref var pools = ref world.ecs.GetRef<Pools>(entity);
            pools.vigor.Base = Formulae.BaseVigor(stats);
            pools.health.Base = Formulae.BaseHealth(stats);

            ref var actor = ref world.ecs.GetRef<Actor>(entity);
            actor.speed = Formulae.Speed(stats);

            var skills = world.ecs.GetRef<Skills>(entity);
            var skill = Formulae.BaseSkill(primary, stats) + skills.GetRank(primary);
            attack.hitChance += skill;
            attack.minDamage = Math.Max(1, attack.minDamage + Formulae.MeleeDamage(stats));
            attack.maxDamage = Math.Max(2, attack.maxDamage + Formulae.MeleeDamage(stats));

            attack.parry += hasShield
                ? Formulae.BaseSkill(SkillType.Shield, stats) + skills.GetRank(SkillType.Shield)
                : skill / 2;
        }

        private static void ResetStats(ref Stats stats)
        {
            stats.strength.Mod = 0;
            stats.endurance.Mod = 0;
            stats.finesse.Mod = 0;
            stats.intellect.Mod = 0;
            stats.resolve.Mod = 0;
            stats.awareness.Mod = 0;
        }

        private static void ResetAttack(World world, Entity entity, ref Attack attack, out SkillType primary, out bool hasShield)
        {
            // Default values
            primary = SkillType.MartialArts;
            attack.hitChance = 0;
            attack.minDamage = 1;
            attack.maxDamage = 2;
            attack.parry = 20;

            // If creature has natural attack, override
            if (world.ecs.TryGet(entity, out NaturalAttack naturalAttack))
            {
                attack.minDamage = naturalAttack.damage.min;
                attack.maxDamage = naturalAttack.damage.max;
                // TODO: Natural attack parry?

                if (naturalAttack.castOnStrike != null)
                {
                    world.ecs.AssignOrReplace(entity, new CastOnStrike { spellId = naturalAttack.castOnStrike });
                }
            }

            var (equippeds, shields, items, attacks) =
                world.ecs.Pools<Equipped, Shield, Item, Attack>();

            // Check if shield equipped
            hasShield = false;
            foreach (var e in world.ecs.View<Equipped, Shield>())
            {
                ref var equipped = ref equippeds[e];
                ref var shield = ref shields[e];

                if (equipped.entity == entity)
                {
                    hasShield = true;
                    attack.parry = shield.baseBlock;
                    break;
                }
            }

            // Check if equipped with weapon, and set attack accordingly
            foreach (var e in world.ecs.View<Equipped, Item, Attack>())
            {
                ref var equipped = ref equippeds[e];
                ref var item = ref items[e];
                ref var weapon = ref attacks[e];

                if (equipped.entity == entity && equipped.slot == Slot.MainHand)
                {
                    primary = item.type.GetWeaponSkill();
                    attack.hitChance = weapon.hitChance;
                    attack.minDamage = weapon.minDamage;
                    attack.maxDamage = weapon.maxDamage;

                    // If no shield, use weapon parry instead
                    if (!hasShield)
                    {
                        attack.parry = weapon.parry;
                        // TODO: Reimplement Versatile with flags
                        /*
                        // If versatile and without shield, add +2 to damage rolls
                        if (attack.style == WeaponStyle.Versatile)
                        {
                            combat.damage = new Extent(combat.damage.min + 2, combat.damage.max + 2);
                        }*/

                    }
                    break;
                }
            }
        }

        private static void ResetDefense(World world, Entity entity, ref Defense defense)
        {
            defense.armor = 0;
            defense.evasion = 0;
            defense.fortitude = 0;
            defense.mental = 0;

            // Check equipped armor
            foreach (var e in world.ecs.View<Equipped, Armor>())
            {
                ref var equipped = ref world.ecs.GetRef<Equipped>(e);
                ref var armor = ref world.ecs.GetRef<Armor>(e);

                if (equipped.entity == entity)
                {
                    defense.armor += armor.armor;
                }
            }
        }

        private static void ApplyHunger(World world, Entity entity, ref Stats stats)
        {
            if (world.ecs.TryGet<HungerClock>(entity, out var hungerClock))
            {
                if (hungerClock.State == HungerState.Hungry)
                {
                    stats.strength.Mod -= 2;
                    stats.endurance.Mod -= 2;
                    stats.finesse.Mod -= 2;
                    stats.intellect.Mod -= 2;
                    stats.resolve.Mod -= 2;
                    stats.awareness.Mod -= 2;
                }
            }
        }
    }
}