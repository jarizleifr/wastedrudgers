using System;
using Blaggard.Common;
using ManulECS;

using WasteDrudgers.Data;

namespace WasteDrudgers.Entities
{
    public static class Creatures
    {
        public static Entity Create(World world, string id, Vec2 pos)
        {
            var creature = world.database.GetCreature(id);
            return Create(world, creature, pos);
        }

        public static Entity Create(World world, DBCreature creature, Vec2 pos)
        {
            var entity = world.ecs.Create();

            world.ecs.Assign(entity, new Identity { name = creature.Name, rawName = creature.Id });
            world.ecs.Assign(entity, new Position { coords = pos });
            world.ecs.Assign(entity, new Renderable
            {
                character = creature.Character,
                color = creature.Race.Color,
            });
            world.ecs.Assign(entity, new AI { });

            var stats = new Stats
            {
                strength = 10 + creature.Race.StrengthModifier,
                endurance = 10 + creature.Race.EnduranceModifier,
                finesse = 10 + creature.Race.FinesseModifier,
                intellect = 10 + creature.Race.IntellectModifier,
                resolve = 10 + creature.Race.ResolveModifier,
                awareness = 10 + creature.Race.AwarenessModifier
            };

            world.ecs.Assign(entity, stats);
            world.ecs.Assign(entity, new Actor
            {
                energy = 0,
                speed = 50 + RNG.IntInclusive(0, 50),
            });
            world.ecs.Assign(entity, new Health
            {
                vigor = Formulae.BaseVigor(stats),
                health = Formulae.BaseHealth(stats)
            });

            world.ecs.Assign(entity, ComponentUtils.CreateSkillsFromProfessions(creature, stats.finesse, stats.intellect));

            if (creature.NaturalAttack != null)
            {
                var naturalAttack = new NaturalAttack
                {
                    baseSkill = creature.NaturalAttack.BaseSkill,
                    minDamage = creature.NaturalAttack.MinDamage,
                    maxDamage = creature.NaturalAttack.MaxDamage
                };
                if (creature.NaturalAttack.CastOnStrike != null)
                {
                    naturalAttack.castOnStrike = creature.NaturalAttack.CastOnStrike.Id;
                }
                world.ecs.Assign(entity, naturalAttack);
            }

            world.spatial.SetCreature(pos, entity);
            Creatures.UpdateCreature(world, entity);

            return entity;
        }

        public static void UpdateCreature(World world, Entity entity)
        {
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            ref var health = ref world.ecs.GetRef<Health>(entity);
            ref var actor = ref world.ecs.GetRef<Actor>(entity);

            health.vigor.Base = Formulae.BaseVigor(stats);
            health.health.Base = Formulae.BaseHealth(stats);
            actor.speed = Formulae.Speed(stats);

            // Default combat stats for any creature
            var unarmed = Formulae.BaseSkill(SkillType.MartialArts, stats) + skills.GetRank(SkillType.MartialArts);
            var combat = new Combat
            {
                hitChance = unarmed,
                minDamage = 1 + Formulae.MeleeDamage(stats),
                maxDamage = 2 + Formulae.MeleeDamage(stats),
                dodge = Formulae.BaseSkill(SkillType.Dodge, stats) + skills.GetRank(SkillType.Dodge) + (int)(unarmed * .33f),
                armor = 0
            };

            // If creature has natural attack, override
            if (world.ecs.TryGet(entity, out NaturalAttack naturalAttack))
            {
                unarmed = naturalAttack.baseSkill + skills.GetRank(SkillType.MartialArts);
                combat.minDamage = Math.Max(1, naturalAttack.minDamage + Formulae.MeleeDamage(stats));
                combat.maxDamage = Math.Max(2, naturalAttack.maxDamage + Formulae.MeleeDamage(stats));
                combat.dodge = Formulae.BaseSkill(SkillType.Dodge, stats) + skills.GetRank(SkillType.Dodge) + (int)(unarmed * .33f);

                if (naturalAttack.castOnStrike != null)
                {
                    world.ecs.AssignOrReplace(entity, new CastOnStrike { spellId = naturalAttack.castOnStrike });
                }
            }

            // TODO: Not all creatures can have equipment, should have some flag to check for that, so we don't loop through equipment unnecessarily

            // Check equipped shield
            bool isShieldEquipped = false;
            world.ecs.Loop((Entity itemEntity, ref Equipped equipped, ref Shield shield) =>
            {
                if (equipped.entity == entity)
                {
                    combat.dodge += shield.baseBlock + skills.GetRank(SkillType.Shield);
                    isShieldEquipped = true;
                }
            });

            // Check equipped armor
            world.ecs.Loop((Entity itemEntity, ref Equipped equipped, ref Defense defense) =>
            {
                if (equipped.entity == entity)
                {
                    combat.dodge += defense.dodge;
                    combat.armor += defense.armor;
                }
            });

            // Check if equipped with weapon, and set attack accordingly
            world.ecs.Loop((Entity itemEntity, ref Equipped equipped, ref Item item, ref Weapon attack) =>
            {
                if (equipped.entity == entity && equipped.slot == Slot.MainHand)
                {
                    var skill = attack.chance + skills.GetRank(item.type.GetWeaponSkill());
                    combat.wielding = Wielding.SingleWeapon;
                    combat.hitChance = skill;
                    combat.minDamage = attack.min + Formulae.MeleeDamage(stats);
                    combat.maxDamage = attack.max + Formulae.MeleeDamage(stats);

                    // If no shield, add weapon parry to dodge
                    if (!isShieldEquipped)
                    {
                        combat.dodge += (int)(skill * attack.parry);

                        // If versatile and without shield, add +2 to damage rolls
                        if (attack.style == WeaponStyle.Versatile)
                        {
                            combat.minDamage += 2;
                            combat.maxDamage += 2;
                        }
                    }
                }
            });
            world.ecs.AssignOrReplace(entity, combat);
        }

        public static void KillCreature(World world, Entity entity)
        {
            var pos = world.ecs.GetRef<Position>(entity);
            world.spatial.ClearCreatureAt(pos.coords);
            world.ecs.Remove<Position>(entity);
            world.ecs.Assign<Death>(entity, new Death { });
        }
    }
}