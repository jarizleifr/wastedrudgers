using System.Collections.Generic;
using System.Linq;
using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Data;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Level
{
    [SerializationProfile("never")]
    public class EntitySpawner
    {
        private int dangerLevel;
        private int min, max;
        private List<DBCreature> creatures;
        private List<DBItem> items;

        public EntitySpawner(IContext ctx, World world, DBLevel level)
        {
            if (level.Creatures.Count != 0)
            {
                creatures = level.Creatures.SelectMany(list => list.Creatures).ToList();
            }

            if (level.Loot.Count != 0)
            {
                items = level.Loot.SelectMany(list => list.Items).ToList();
            }

            dangerLevel = level.DangerLevel;
            min = level.MinSpawn;
            max = level.MaxSpawn;
        }

        public void InitialSpawning(World world)
        {
            if (creatures != null)
            {
                for (int i = 0; i < RNG.Int(min, max); i++)
                {
                    Spawn(world, LevelUtils.GetRandomPassablePositionWithoutCreature(world));
                }
            }

            if (items != null)
            {
                for (int i = 0; i < RNG.Int(min, max); i++)
                {
                    SpawnItem(world, LevelUtils.GetRandomPassablePosition(world));
                }
            }
        }

        public void Spawn(World world, Vec2 position)
        {
            var index = RNG.Int(creatures.Count);
            Creatures.Create(world, creatures[index], position);
        }

        public void SpawnItem(World world, Vec2 position)
        {
            DBItem randomItem = null;
            DBMaterial randomMaterial = null;

            while (randomItem == null)
            {
                var candidateItem = items[RNG.Int(items.Count)];
                var candidateMaterial = candidateItem.BaseMaterial;

                if (candidateItem.MaterialGroup != null)
                {
                    int acc = 0;
                    var random = new List<(DBMaterial material, int randomIndex)>();
                    foreach (var m in candidateItem.MaterialGroup.Materials)
                    {
                        random.Add((m.material, m.probability + acc));
                        acc += m.probability;
                    }
                    var r = RNG.IntInclusive(1, 100);
                    candidateMaterial = random.Find(m => r <= m.randomIndex).material;
                }

                if (candidateItem.DangerLevel + candidateMaterial.DangerLevelModifier <= dangerLevel)
                {
                    randomItem = candidateItem;
                    randomMaterial = candidateMaterial;
                }
            }

            Items.CreateItem(world, randomItem, randomMaterial, position);
        }
    }
}