using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Common;

namespace WasteDrudgers.Entities
{
    /// <summary>Collection of methods to deal with Item entities</summary>
    public static class Items
    {
        public static Entity Create(World world, string key, Vec2 pos)
        {
            var rawItem = Data.GetItem(key);
            var rawMaterial = Data.GetMaterial(rawItem.Material);
            return CreateItem(world, rawItem, rawMaterial, pos);
        }

        public static Entity CreateItem(World world, string key, string keyMat, Vec2 pos)
        {
            var raw = Data.GetItem(key);
            var rawMat = Data.GetMaterial(keyMat);
            return CreateItem(world, raw, rawMat, pos);
        }

        public static Entity CreateItem(World world, DBItem raw, DBMaterial material, Vec2 pos)
        {
            var entity = world.ecs.Create();
            world.ecs.Assign(entity, new Position { coords = pos });
            world.ecs.Assign(entity, new Renderable
            {
                character = raw.Char,
                color = material.Color,
            });
            world.ecs.Assign(entity, new Item
            {
                material = material.Id,
                type = raw.Type,
                count = 1,
                weight = (int)(raw.Weight * material.WeightMult),
            });
            world.ecs.Assign(entity, new Identity
            {
                name = raw.Name,
                rawName = raw.Id,
                description = "",
            });

            var baseMaterial = Data.GetMaterial(raw.Material);
            if (raw.Type.IsWeapon())
            {
                var wpn = (DBWeapon)raw;
                if (raw.Type == ItemType.Shield)
                {
                    world.ecs.Assign(entity, new Shield { baseBlock = wpn.BaseSkill });
                }
                else
                {
                    world.ecs.Assign(entity, new Weapon
                    {
                        chance = wpn.BaseSkill,
                        damage = new Extent(
                            wpn.Damage.min + (material.DamageBonus - baseMaterial.DamageBonus),
                            wpn.Damage.max + (material.DamageBonus - baseMaterial.DamageBonus)
                        ),
                        parry = wpn.Parry,
                    });
                }
            }

            if (raw.Type.IsApparel())
            {
                var apr = (DBApparel)raw;
                world.ecs.Assign(entity, new Defense
                {
                    dodge = apr.Dodge,
                    armor = apr.Armor + (material.ArmorBonus - baseMaterial.ArmorBonus),
                });
            }

            if (raw.Type.IsObfuscated())
            {
                var obfuscated = world.ObfuscatedNames;
                if (!obfuscated.IsKnown(raw.Id))
                {
                    world.ecs.Assign<Obfuscated>(entity);
                }
            }

            if (raw.UseSpell != null)
            {
                world.ecs.Assign<CastOnUse>(entity, new CastOnUse { spellId = raw.UseSpell });
            }

            world.spatial.PlaceItem(world, pos, entity);

            return entity;
        }

        public static string GetName(World world, Entity itemEntity)
        {
            var item = world.ecs.GetRef<Item>(itemEntity);
            var identity = world.ecs.GetRef<Identity>(itemEntity);

            if (world.ecs.Has<Obfuscated>(itemEntity))
            {
                var obfuscated = world.ObfuscatedNames;
                if (!obfuscated.IsKnown(identity.rawName))
                {
                    return obfuscated.GetObfuscatedName(world, item.type, identity.rawName);
                }
            }
            return identity.name;
        }

        public static string GetFullName(World world, Entity itemEntity)
        {
            var item = world.ecs.GetRef<Item>(itemEntity);
            var identity = world.ecs.GetRef<Identity>(itemEntity);

            if (item.status != IdentificationStatus.Unknown)
            {
                var displayName = identity.name;
                var rawItem = Data.GetItem(identity.rawName);

                // Only show material in name if there are more than one material
                if (rawItem.MaterialTags.Count > 0)
                {
                    var rawMaterial = Data.GetMaterial(item.material);
                    var material = rawMaterial.Name.ToLower();
                    displayName = $"{material} {identity.name}";
                }
                return displayName;
            }

            if (world.ecs.Has<Obfuscated>(itemEntity))
            {
                var obfuscated = world.ObfuscatedNames;
                if (!obfuscated.IsKnown(identity.rawName))
                {
                    return obfuscated.GetObfuscatedName(world, item.type, identity.rawName);
                }
            }

            return identity.name;
        }

        public static void ExamineItem(World world, Entity itemEntity)
        {
            ref var item = ref world.ecs.GetRef<Item>(itemEntity);
            if (item.status != IdentificationStatus.Unknown) return;

            item.status = IdentificationStatus.Examined;
        }

        public static void IdentifyItem(World world, Entity itemEntity)
        {
            ref var item = ref world.ecs.GetRef<Item>(itemEntity);
            item.status = IdentificationStatus.Identified;

            if (world.ecs.Has<Obfuscated>(itemEntity))
            {
                var obfuscated = world.ObfuscatedNames;
                var identity = world.ecs.GetRef<Identity>(itemEntity);
                obfuscated.SetKnown(identity.rawName);
                world.ecs.Remove<Obfuscated>(itemEntity);
            }
        }

        public static void IdentifyInventory(World world)
        {
            var playerData = world.PlayerData;
            // TODO: Don't owned items have PlayerMarker? Potential for optimization
            foreach (var e in world.ecs.View<Item, InBackpack>())
            {
                ref var item = ref world.ecs.GetRef<Item>(e);
                ref var inBackpack = ref world.ecs.GetRef<InBackpack>(e);

                if (inBackpack.entity == playerData.entity)
                {
                    IdentifyItem(world, e);
                }
            }
            foreach (var e in world.ecs.View<Item, Equipped>())
            {
                ref var item = ref world.ecs.GetRef<Item>(e);
                ref var equipped = ref world.ecs.GetRef<Equipped>(e);

                if (equipped.entity == playerData.entity)
                {
                    IdentifyItem(world, e);
                }
            }
        }

        public static void EquipItem(World world, Entity equipper, Entity itemEntity)
        {
            ref var equipItem = ref world.ecs.GetRef<Item>(itemEntity);
            var targetSlot = equipItem.type.GetEquipmentSlot();

            UnequipItemToBackpack(world, equipper, targetSlot);

            var equipping = RemoveItemFromInventory(world, itemEntity);
            ExamineItem(world, equipping);
            world.ecs.Assign<Equipped>(equipping, new Equipped { entity = equipper, slot = targetSlot });

            world.ecs.Assign<EventStatsUpdated>(equipper);
        }

        public static void UnequipItemToBackpack(World world, Entity unequipper, Slot targetSlot)
        {
            foreach (var e in world.ecs.View<Equipped>())
            {
                ref var equipped = ref world.ecs.GetRef<Equipped>(e);
                if (equipped.slot == targetSlot && equipped.entity == unequipper)
                {
                    world.ecs.Remove<Equipped>(e);
                    AddItemToInventory(world, unequipper, e);
                }
            }
            world.ecs.Assign<EventStatsUpdated>(unequipper);
        }

        public static void UseItem(World world, Entity user, Entity itemEntity)
        {
            var identity = world.ecs.GetRef<Identity>(itemEntity);

            if (world.ecs.TryGet(itemEntity, out CastOnUse castOnUse))
            {
                Effects.CastSpell(world, user, castOnUse.spellId, null);
            }

            var obfuscated = world.ObfuscatedNames;
            obfuscated.SetKnown(identity.rawName);

            var used = RemoveItemFromInventory(world, itemEntity);
            world.ecs.Remove(used);

            world.ecs.Assign<EventInventoryUpdated>(user);
        }

        public static void PickUpItem(World world, Entity getter, Entity itemEntity)
        {
            var pos = world.ecs.GetRef<Position>(itemEntity);

            world.ecs.Remove<Position>(itemEntity);
            world.spatial.ClearItemAt(pos.coords, itemEntity);
            AddItemToInventory(world, getter, itemEntity);

            world.ecs.Assign<EventStatsUpdated>(getter);
            world.ecs.Assign<EventInventoryUpdated>(getter);
        }

        public static void DropItem(World world, Entity dropper, Entity itemEntity)
        {
            var pos = world.ecs.GetRef<Position>(dropper);

            itemEntity = RemoveItemFromInventory(world, itemEntity);
            world.ecs.AssignOrReplace(itemEntity, new Position { coords = pos.coords });
            world.ecs.Remove<PlayerMarker>(itemEntity);
            world.spatial.PlaceItem(world, pos.coords, itemEntity);

            world.ecs.Assign<EventStatsUpdated>(dropper);
            world.ecs.Assign<EventInventoryUpdated>(dropper);
        }

        private static void AddItemToInventory(World world, Entity getter, Entity itemEntity)
        {
            var foundInBackpack = false;
            foreach (var e in world.ecs.View<InBackpack, Item>())
            {
                ref var item = ref world.ecs.GetRef<Item>(e);
                if (IsSameKindOf(world, itemEntity, e))
                {
                    foundInBackpack = true;
                    item.count++;
                    world.ecs.Remove(itemEntity);
                    break;
                }
            }

            if (!foundInBackpack)
            {
                var playerData = world.PlayerData;
                world.ecs.Assign<InBackpack>(itemEntity, new InBackpack { entity = getter });
                if (getter == playerData.entity)
                {
                    world.ecs.Assign<PlayerMarker>(itemEntity);
                }
            }
        }

        private static Entity RemoveItemFromInventory(World world, Entity itemEntity)
        {
            ref var item = ref world.ecs.GetRef<Item>(itemEntity);

            if (item.count > 1)
            {
                item.count--;
                var clone = world.ecs.Clone(itemEntity);
                ref var cloneItem = ref world.ecs.GetRef<Item>(clone);
                cloneItem.count = 1;
                world.ecs.Remove<InBackpack>(clone);
                return clone;
            }
            else
            {
                world.ecs.Remove<InBackpack>(itemEntity);
                return itemEntity;
            }
        }

        public static bool IsSameKindOf(World world, Entity item1, Entity item2)
        {
            var id1 = world.ecs.GetRef<Identity>(item1);
            var id2 = world.ecs.GetRef<Identity>(item2);

            var itm1 = world.ecs.GetRef<Item>(item1);
            var itm2 = world.ecs.GetRef<Item>(item2);

            return (id1.rawName == id2.rawName) && (itm1.status == itm2.status) && (itm1.material == itm2.material);
        }

        public static int GetTotalNutrition(World world, Entity owner)
        {
            var nutrition = GetRations(world, owner);
            if (world.ecs.TryGet<HungerClock>(owner, out var clock))
            {
                nutrition += clock.nutrition;
            }
            return nutrition;
        }

        public static int GetRations(World world, Entity owner)
        {
            var rations = 0;
            foreach (var e in world.ecs.View<Item, InBackpack>())
            {
                ref var item = ref world.ecs.GetRef<Item>(e);
                ref var inBackpack = ref world.ecs.GetRef<InBackpack>(e);
                if (inBackpack.entity != owner) break;

                if (item.type == ItemType.Food)
                {
                    rations += 800 * item.count;
                }
            }
            return rations;
        }

        public static (int gotNutrition, int rationsRemaining) TryAutoConsume(World world, Entity consumer)
        {
            int gotNutrition = 0;
            int rationsRemaining = 0;

            foreach (var e in world.ecs.View<Item, InBackpack>())
            {
                ref var item = ref world.ecs.GetRef<Item>(e);
                ref var inBackpack = ref world.ecs.GetRef<InBackpack>(e);

                if (inBackpack.entity != consumer) break;

                if (item.type == ItemType.Food)
                {
                    if (gotNutrition == 0)
                    {
                        gotNutrition = 800;
                        var used = RemoveItemFromInventory(world, e);
                        world.ecs.Remove(used);
                        world.ecs.Assign<EventInventoryUpdated>(consumer);
                    }
                    else
                    {
                        rationsRemaining += 800 * item.count;
                    }
                }
            }
            return (gotNutrition, rationsRemaining);
        }

        public static void UpdateFoodLeft(World world)
        {
            foreach (var e in world.ecs.View<HungerClock>())
            {
                ref var clock = ref world.ecs.GetRef<HungerClock>(e);
                clock.food = Items.GetRations(world, e);
            }
        }
    }
}