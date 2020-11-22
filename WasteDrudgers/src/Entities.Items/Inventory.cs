using System;
using System.Collections.Generic;
using System.Linq;
using Blaggard.Common;
using ManulECS;

namespace WasteDrudgers.Entities
{
    // TODO: The only things we need to cache is sortable properties
    public class ItemWrapper
    {
        public readonly Entity entity;
        public string name;
        public ItemType type;
        public bool equipped;

        public ItemWrapper(Entity entity, string name, ItemType type, bool equipped)
        {
            this.entity = entity;
            this.name = name;
            this.type = type;
            this.equipped = equipped;
        }
    }

    public class Inventory
    {
        private List<ItemWrapper> items;

        public static Inventory FromItemsOnGround(World world, Vec2 position)
        {
            var items = new List<ItemWrapper>();

            if (world.spatial.TryGetItems(position, out var entities))
            {
                foreach (var entity in entities)
                {
                    var name = Items.GetFullName(world, entity);
                    var item = world.ecs.GetRef<Item>(entity);
                    items.Add(new ItemWrapper(entity, name, item.type, false));
                }
            }
            return new Inventory(items);
        }

        public static Inventory FromOwned(World world, Entity owner)
        {
            var items = new List<ItemWrapper>();
            foreach (var e in world.ecs.View<InBackpack, Item, Renderable>())
            {
                ref var inBackpack = ref world.ecs.GetRef<InBackpack>(e);
                ref var item = ref world.ecs.GetRef<Item>(e);
                ref var renderable = ref world.ecs.GetRef<Renderable>(e);

                if (inBackpack.entity == owner)
                {
                    var name = Items.GetFullName(world, e);
                    items.Add(new ItemWrapper(e, name, item.type, false));
                }
            }
            foreach (var e in world.ecs.View<Equipped, Item, Renderable>())
            {
                ref var equipped = ref world.ecs.GetRef<Equipped>(e);
                ref var item = ref world.ecs.GetRef<Item>(e);
                ref var renderable = ref world.ecs.GetRef<Renderable>(e);

                if (equipped.entity == owner)
                {
                    var name = Items.GetFullName(world, e);
                    items.Add(new ItemWrapper(e, name, item.type, true));
                }
            };

            return new Inventory(items);
        }

        private Inventory(IEnumerable<ItemWrapper> items)
        {
            this.items = items.ToList();
            SortByEquipped();
        }

        public int Count => items.Count;

        public ItemWrapper this[int idx] => items[idx];

        public void Filter(Func<ItemWrapper, bool> predicate) => items = items.Where(predicate).ToList();

        public void SortByName() => items.Sort((a, b) => a.name.CompareTo(b.name));
        public void SortByEquipped() => items = items
            .OrderBy((a) => !a.equipped)
            .ThenBy((a) => a.name)
            .ToList();
    }
}