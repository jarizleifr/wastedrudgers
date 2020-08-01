using System;
using Blaggard.Common;
using ManulECS;

using WasteDrudgers.Data;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    /// <summary>Collection of methods to deal with Item entities</summary>
    public static class Items
    {
        public static Entity Create(World world, string key, Vec2 pos)
        {
            var rawItem = world.database.GetItem(key);
            return CreateItem(world, rawItem, rawItem.BaseMaterial, pos);
        }

        public static Entity CreateItem(World world, string key, string keyMat, Vec2 pos)
        {
            var raw = world.database.GetItem(key);
            var rawMat = world.database.GetMaterial(keyMat);
            return CreateItem(world, raw, rawMat, pos);
        }

        public static Entity CreateItem(World world, DBItem raw, DBMaterial material, Vec2 pos)
        {
            if (!Enum.TryParse(material.Name, true, out Material mat))
            {
                throw new Exception($"Cannot convert material {material.Name} to a Material enum");
            }

            var entity = world.ecs.Create();
            world.ecs.Assign(entity, new Position { coords = pos });
            world.ecs.Assign(entity, new Renderable
            {
                character = raw.Character,
                glyph = raw.Glyph,
                color = material.Color,
            });
            world.ecs.Assign(entity, new Item
            {
                material = mat,
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

            if (raw.Type.IsWeapon())
            {
                if (raw.Type == ItemType.Shield)
                {
                    world.ecs.Assign(entity, new Shield { baseBlock = raw.BaseSkill });
                }
                else
                {
                    world.ecs.Assign(entity, new Weapon
                    {
                        chance = raw.BaseSkill,
                        min = raw.MinDamage + (material.DamageBonus - raw.BaseMaterial.DamageBonus),
                        max = raw.MaxDamage + (material.DamageBonus - raw.BaseMaterial.DamageBonus),
                        parry = raw.Parry,
                    });
                }
            }

            if (raw.Type.IsArmor())
            {
                world.ecs.Assign(entity, new Defense
                {
                    dodge = raw.Dodge,
                    armor = raw.Armor + (material.ArmorBonus - raw.BaseMaterial.ArmorBonus),
                });
            }

            if (raw.Obfuscated)
            {
                var obfuscated = world.ecs.FetchResource<ObfuscatedNames>();
                if (!obfuscated.IsKnown(raw.Id))
                {
                    world.ecs.Assign(entity, new Obfuscated { });
                }
            }

            if (raw.UseSpell != null)
            {
                var spell = raw.UseSpell;
                world.ecs.Assign<CastOnUse>(entity, new CastOnUse { spellId = spell.Id });
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
                var obfuscated = world.ecs.FetchResource<ObfuscatedNames>();
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
                var rawItem = world.database.GetItem(identity.rawName);

                // Only show material in name if there are more than one material
                if (rawItem.MaterialGroup != null)
                {
                    var material = item.material.ToString().ToLower();
                    displayName = $"{material} {identity.name}";
                }
                return displayName;
            }

            if (world.ecs.Has<Obfuscated>(itemEntity))
            {
                var obfuscated = world.ecs.FetchResource<ObfuscatedNames>();
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
                var obfuscated = world.ecs.FetchResource<ObfuscatedNames>();
                var identity = world.ecs.GetRef<Identity>(itemEntity);
                obfuscated.SetKnown(identity.rawName);
                world.ecs.Remove<Obfuscated>(itemEntity);
            }
        }

        public static void IdentifyInventory(World world)
        {
            var playerData = world.ecs.FetchResource<PlayerData>();
            world.ecs.Loop<Item, InBackpack>((Entity entity, ref Item item, ref InBackpack inBackpack) =>
            {
                if (inBackpack.entity == playerData.entity)
                {
                    IdentifyItem(world, entity);
                }
            });
            world.ecs.Loop<Item, Equipped>((Entity entity, ref Item item, ref Equipped equipped) =>
            {
                if (equipped.entity == playerData.entity)
                {
                    IdentifyItem(world, entity);
                }
            });
        }

        public static void EquipItem(World world, Entity equipper, Entity itemEntity)
        {
            ref var equipItem = ref world.ecs.GetRef<Item>(itemEntity);
            var targetSlot = equipItem.type.GetEquipmentSlot();

            UnequipItemToBackpack(world, equipper, targetSlot);

            var equipping = RemoveItemFromInventory(world, itemEntity);
            ExamineItem(world, equipping);
            world.ecs.Assign<Equipped>(equipping, new Equipped { entity = equipper, slot = targetSlot });

            Creatures.UpdateCreature(world, equipper);
        }

        public static void UnequipItemToBackpack(World world, Entity unequipper, Slot targetSlot)
        {
            world.ecs.Loop<Equipped>((Entity entity, ref Equipped equipped) =>
            {
                if (equipped.slot == targetSlot && equipped.entity == unequipper)
                {
                    world.ecs.Remove<Equipped>(entity);
                    AddItemToInventory(world, unequipper, entity);
                }
            });
            Creatures.UpdateCreature(world, unequipper);
        }

        public static void UseItem(World world, Entity user, Entity itemEntity)
        {
            var identity = world.ecs.GetRef<Identity>(itemEntity);

            if (world.ecs.TryGet(itemEntity, out CastOnUse castOnUse))
            {
                Spells.CastSpellOn(world, null, user, castOnUse.spellId);
            }

            var obfuscated = world.ecs.FetchResource<ObfuscatedNames>();
            obfuscated.SetKnown(identity.rawName);

            var used = RemoveItemFromInventory(world, itemEntity);
            world.ecs.Remove(used);

            Creatures.UpdateCreature(world, user);
        }

        public static void PickUpItem(World world, Entity getter, Entity itemEntity)
        {
            var pos = world.ecs.GetRef<Position>(itemEntity);

            world.ecs.Remove<Position>(itemEntity);
            world.spatial.ClearItemAt(pos.coords, itemEntity);
            AddItemToInventory(world, getter, itemEntity);
        }

        private static void AddItemToInventory(World world, Entity getter, Entity itemEntity)
        {
            var foundInBackpack = false;
            world.ecs.Loop((Entity entity, ref InBackpack b, ref Item item) =>
            {
                if (foundInBackpack) return;
                if (IsSameKindOf(world, itemEntity, entity))
                {
                    foundInBackpack = true;
                    item.count++;
                    world.ecs.Remove(itemEntity);
                }
            });

            if (!foundInBackpack)
            {
                var playerData = world.ecs.FetchResource<PlayerData>();
                world.ecs.Assign<InBackpack>(itemEntity, new InBackpack { entity = getter });
                if (getter == playerData.entity)
                {
                    world.ecs.Assign<PlayerMarker>(itemEntity, new PlayerMarker { });
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

        public static void DropItem(World world, Entity dropper, Entity itemEntity)
        {
            var pos = world.ecs.GetRef<Position>(dropper);

            itemEntity = RemoveItemFromInventory(world, itemEntity);
            world.ecs.AssignOrReplace(itemEntity, new Position { coords = pos.coords });
            world.ecs.Remove<PlayerMarker>(itemEntity);
            world.spatial.PlaceItem(world, pos.coords, itemEntity);
        }

        public static bool IsSameKindOf(World world, Entity item1, Entity item2)
        {
            var id1 = world.ecs.GetRef<Identity>(item1);
            var id2 = world.ecs.GetRef<Identity>(item2);

            var itm1 = world.ecs.GetRef<Item>(item1);
            var itm2 = world.ecs.GetRef<Item>(item2);

            return (id1.rawName == id2.rawName) && (itm1.status == itm2.status) && (itm1.material == itm2.material);
        }
    }
}