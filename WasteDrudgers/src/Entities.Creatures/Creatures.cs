using System;
using System.Collections.Generic;
using Blaggard.Common;
using ManulECS;

using WasteDrudgers.Common;

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
            world.ecs.Assign(entity, new AI { });

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
            world.ecs.Assign(entity, new Skills { set = new List<Skill>() });

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

        public static void UpdateCreature(World world, Entity entity)
        {
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            var exp = world.ecs.GetRef<Experience>(entity);

            ref var health = ref world.ecs.GetRef<Health>(entity);
            ref var actor = ref world.ecs.GetRef<Actor>(entity);

            health.vigor.Base = Formulae.BaseVigor(stats) + (exp.level - 1) * Formulae.VigorPerLevel(stats);
            health.health.Base = Formulae.BaseHealth(stats) + (exp.level - 1) * Formulae.HealthPerLevel(stats);
            actor.speed = Formulae.Speed(stats);

            // Default combat stats for any creature
            var unarmed = Formulae.BaseSkill(SkillType.MartialArts, stats) + skills.GetRank(SkillType.MartialArts);
            var combat = new Combat
            {
                hitChance = unarmed,
                damage = new Extent(1 + Formulae.MeleeDamage(stats), 2 + Formulae.MeleeDamage(stats)),
                dodge = Formulae.BaseSkill(SkillType.Dodge, stats) + skills.GetRank(SkillType.Dodge) + (int)(unarmed * .33f),
                armor = 0
            };

            // If creature has natural attack, override
            if (world.ecs.TryGet(entity, out NaturalAttack naturalAttack))
            {
                unarmed = naturalAttack.baseSkill + skills.GetRank(SkillType.MartialArts);
                combat.damage = new Extent(
                    Math.Max(1, naturalAttack.damage.min + Formulae.MeleeDamage(stats)),
                    Math.Max(2, naturalAttack.damage.max + Formulae.MeleeDamage(stats))
                );
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
                    var skillType = item.type.GetWeaponSkill();
                    var skill = Formulae.BaseSkill(skillType, stats) + skills.GetRank(skillType) + attack.chance;
                    combat.wielding = Wielding.SingleWeapon;
                    combat.hitChance = skill;
                    combat.damage = new Extent(
                        attack.damage.min + Formulae.MeleeDamage(stats),
                        attack.damage.max + Formulae.MeleeDamage(stats)
                    );

                    // If no shield, add weapon parry to dodge
                    if (!isShieldEquipped)
                    {
                        combat.dodge += (int)(skill * attack.parry);

                        // If versatile and without shield, add +2 to damage rolls
                        if (attack.style == WeaponStyle.Versatile)
                        {
                            combat.damage = new Extent(combat.damage.min + 2, combat.damage.max + 2);
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

        public static void AwardKillExperience(World world, Entity awardee, Entity killed)
        {
            AwardExperience(world, awardee, GetSpentCharacterPoints(world, killed) / 100);
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
                characterPoints += stats.Get(type).Base * Formulae.GetStatCost(type);
            }

            foreach (var s in skills.set)
            {
                characterPoints += s.value * 2;
            }

            return characterPoints;
        }
    }
}