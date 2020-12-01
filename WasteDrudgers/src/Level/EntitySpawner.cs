using System.Collections.Generic;
using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Common;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Level
{
    [SerializationProfile("never")]
    public class EntitySpawner
    {
        private int dangerLevel;
        public Extent MonsterSpawns { get; private set; }
        private List<DBCreature> creatures;
        private List<DBItem> items;

        public EntitySpawner(IContext ctx, World world, DBLevel level)
        {
            creatures = Data.GetLevelSpawns(level);
            items = Data.GetLevelLoot(level);

            dangerLevel = level.DangerLevel;
            MonsterSpawns = level.Monsters ?? new Extent(0, 0);
        }

        public void InitialSpawning(World world, Extent? itemSpawns)
        {
            if (creatures != null)
            {
                for (int i = 0; i < RNG.Extent(MonsterSpawns); i++)
                {
                    Spawn(world, LevelUtils.GetRandomPassablePositionWithoutCreature(world));
                }
            }

            if (itemSpawns.HasValue && items != null)
            {
                for (int i = 0; i < RNG.Extent(itemSpawns.Value); i++)
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
                var candidateMaterial = Data.GetMaterial(candidateItem.Material);

                if (candidateItem.MaterialTags.Count > 0)
                {
                    var materials = Data.GetItemMaterials(candidateItem);
                    candidateMaterial = materials[RNG.Int(materials.Count)];
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