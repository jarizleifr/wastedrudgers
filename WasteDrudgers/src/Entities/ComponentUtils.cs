using System.Linq;
using ManulECS;

namespace WasteDrudgers.Entities
{
    public static class ComponentUtils
    {
        public static void ApplyProfession(World world, Entity entity, DBCreature raw)
        {
            var pointsLeft = 0;
            ref var stats = ref world.ecs.GetRef<Stats>(entity);
            ref var pools = ref world.ecs.GetRef<Pools>(entity);
            ref var skills = ref world.ecs.GetRef<Skills>(entity);

            foreach (var profession in raw.Professions)
            {
                var split = profession.Split(":");
                var rawProfession = Data.GetProfession(split[0]);
                var points = int.Parse(split[1]) + pointsLeft;
                while (points >= 2)
                {
                    var available = rawProfession.Where(p => p.GetCost() <= points).ToList();

                    var costs20 = available.Where(p => p.GetCost() == 20).ToList();
                    var costs10 = available.Where(p => p.GetCost() == 10).ToList();
                    var costs5 = available.Where(p => p.GetCost() == 5).ToList();
                    var costs2 = available.Where(p => p.GetCost() == 2).ToList();

                    ProfessionFocus focus;

                    if (points >= 20)
                    {
                        var weight = RNG.Int(100);
                        if (costs2.Count > 0 && weight < 50)
                        {
                            focus = costs2[RNG.Int(costs2.Count)];
                        }
                        else if (costs5.Count > 0)
                        {
                            focus = costs5[RNG.Int(costs5.Count)];
                        }
                        else if (costs10.Count > 0)
                        {
                            focus = costs10[RNG.Int(costs10.Count)];
                        }
                        else if (costs20.Count > 0)
                        {
                            focus = costs20[RNG.Int(costs20.Count)];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (points >= 10)
                    {
                        var weight = RNG.Int(100);
                        if (costs2.Count > 0 && weight < 50)
                        {
                            focus = costs2[RNG.Int(costs2.Count)];
                        }
                        else if (costs5.Count > 0)
                        {
                            focus = costs5[RNG.Int(costs5.Count)];
                        }
                        else if (costs10.Count > 0)
                        {
                            focus = costs10[RNG.Int(costs10.Count)];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (points >= 5)
                    {
                        var weight = RNG.Int(100);
                        if (costs2.Count > 0 && weight < 50)
                        {
                            focus = costs2[RNG.Int(costs2.Count)];
                        }
                        else if (costs5.Count > 0)
                        {
                            focus = costs5[RNG.Int(costs5.Count)];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (points >= 2 && costs2.Count > 0)
                    {
                        focus = costs2[RNG.Int(costs2.Count)];
                    }
                    else
                    {
                        break;
                    }

                    ApplyFocus(focus, ref stats, ref pools, ref skills);
                    points -= focus.GetCost();
                }
                pointsLeft += points;
            }
        }

        private static void ApplyFocus(ProfessionFocus focus, ref Stats stats, ref Pools pools, ref Skills skills)
        {
            if (focus.IsStat())
            {
                var type = focus.GetStat();
                stats[type]++;
            }
            else if (focus.IsPool())
            {
                var type = focus.GetPool();
                switch (type)
                {
                    case PoolType.Vigor:
                        pools.vigor.Base += 1;
                        break;
                    case PoolType.Health:
                        pools.health.Base += 1;
                        break;
                }
            }
            else
            {
                var type = focus.GetSkill();
                skills.Increment(type);
            }
        }

        public static int GetLevel(World world, Entity entity)
        {
            var points = Creatures.GetSpentCharacterPoints(world, entity);
            points -= 800;
            points /= 15;

            return points <= 0 ? 1 : points;
        }
    }
}