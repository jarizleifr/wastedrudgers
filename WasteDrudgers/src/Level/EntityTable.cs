using System;
using System.Collections.Generic;
using System.Linq;
using Blaggard.Common;
using ManulECS;

using WasteDrudgers.Entities;

namespace WasteDrudgers.Level
{
    // TODO: Should be called probably "SpatialEntities" or something like that, to point out this is space -> entity relation, and not an "entity" thing
    [SerializationProfile("never")]
    public class EntityTable
    {
        private Dictionary<Vec2, Entity> creatures = new Dictionary<Vec2, Entity>();
        private Dictionary<Vec2, Entity> features = new Dictionary<Vec2, Entity>();
        private Dictionary<Vec2, HashSet<Entity>> items = new Dictionary<Vec2, HashSet<Entity>>();

        public void Clear()
        {
            creatures.Clear();
            features.Clear();
            items.Clear();
        }

        public void Populate(World world)
        {
            Clear();

            world.ecs.Loop((Entity entity, ref Position pos, ref Portal portal) =>
            {
                features[pos.coords] = entity;
            });

            world.ecs.Loop((Entity entity, ref Position pos, ref Actor actor) =>
            {
                creatures[pos.coords] = entity;
            });

            world.ecs.Loop((Entity entity, ref Position pos, ref Item item) =>
            {
                PlaceItem(world, pos.coords, entity);
            });
        }

        public bool TryGetFeature(Vec2 key, out Entity entity) => features.TryGetValue(key, out entity);

        public void SetFeature(Vec2 key, Entity entity)
        {
            if (!features.TryAdd(key, entity))
            {
                throw new Exception($"Trying to place a Feature at {key}, but a Feature already exists there!");
            }
        }

        public bool TryGetCreature(Vec2 key, out Entity entity) => creatures.TryGetValue(key, out entity);

        public void SetCreature(Vec2 key, Entity entity)
        {
            if (!creatures.TryAdd(key, entity))
            {
                throw new Exception($"Trying to place a Creature at {key}, but a Creature already exists there!");
            }
        }

        public bool TryMoveCreature(Entity entity, Vec2 pos, Vec2 newPos)
        {
            if (TryGetCreature(newPos, out var e)) return false;

            creatures.Remove(pos);
            creatures.Add(newPos, entity);
            return true;
        }

        public void ClearCreatureAt(Vec2 pos) => creatures.Remove(pos);

        public IEnumerable<Vec2> GetPositionsWithItems() => items.Keys;
        public Renderable GetItemsRenderable(Vec2 position, World world) => world.ecs.GetRef<Renderable>
        (
            items[position].OrderByDescending((i) => world.ecs.GetRef<Item>(i).weight).First()
        );

        public bool TryGetItems(Vec2 key, out HashSet<Entity> entities) => items.TryGetValue(key, out entities);

        public Entity GetItemOrThrow(Vec2 key)
        {
            if (items.TryGetValue(key, out var entities))
            {
                return entities.First();
            }
            throw new Exception($"Item not found at position {key}!");
        }

        public void PlaceItem(World world, Vec2 key, Entity item)
        {
            if (TryGetItems(key, out var entities))
            {
                foreach (var i in entities)
                {
                    if (Items.IsSameKindOf(world, item, i))
                    {
                        world.ecs.Remove(item);
                        ref var onGround = ref world.ecs.GetRef<Item>(i);
                        onGround.count++;
                        return;
                    }
                }
                entities.Add(item);
            }
            else
            {
                items.Add(key, new HashSet<Entity>() { item });
            }
        }

        public int ItemsCountAt(Vec2 pos) => items.TryGetValue(pos, out var entities) ? entities.Count : 0;

        public void ClearItemAt(Vec2 pos, Entity entity)
        {
            items[pos].Remove(entity);
            if (items[pos].Count == 0)
            {
                items.Remove(pos);
            }
        }

        public bool ClearItemsAt(Vec2 pos) => items.Remove(pos);
    }
}