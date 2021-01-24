using System;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    [InputDomains("menu", "inventory", "selection")]
    public class InventoryState : GameScene
    {
        public int selected;
        public int offset;
        public InventoryCategory category;

        private PlayerData playerData;
        private Inventory inventory;

        private bool droppedItem;
        private bool changedEquipment;

        public override void Initialize(IContext ctx, World world)
        {
            playerData = world.PlayerData;
            UpdateInventory(world, playerData);
            droppedItem = false;
            changedEquipment = false;
        }

        public override void Update(IContext ctx, World world)
        {
            var menu = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.TerminalWindow(ctx);

            int l = inventory.Count;
            int view = Math.Min(l, InventoryUI.INVENTORY_LENGTH);

            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    ItemAction(ctx, world, playerData, selected + offset);
                    break;

                case Command command when command >= Command.SelectA && command <= Command.SelectH:
                    ItemAction(ctx, world, playerData, command - Command.SelectA + offset);
                    break;

                case Command.DropItem:
                    var oldLength = inventory.Count;
                    if (oldLength > 0)
                    {
                        var wrapper = inventory[selected + offset];
                        if (wrapper.equipped)
                        {
                            Items.UnequipItemToBackpack(world, playerData.entity, wrapper.type.GetEquipmentSlot());
                            changedEquipment = true;
                        }
                        droppedItem = true;
                        Items.DropItem(world, playerData.entity, wrapper.entity);
                        UpdateInventory(world, playerData);
                        UpdateSelection(oldLength, inventory.Count);
                        FilterInventory();
                    }
                    break;

                case Command.MenuLeft:
                    UpdateInventory(world, playerData);
                    selected = 0; offset = 0;
                    category = InventoryUI.PrevCategory(category);
                    FilterInventory();

                    break;
                case Command.MenuRight:
                    UpdateInventory(world, playerData);
                    selected = 0; offset = 0;
                    category = InventoryUI.NextCategory(category);
                    FilterInventory();
                    break;

                case Command.MenuUp:
                    (selected, offset) = Menu.Prev(selected, offset, view, l);
                    break;

                case Command.MenuDown:
                    (selected, offset) = Menu.Next(selected, offset, view, l);
                    break;

                case Command.Exit:
                    if (droppedItem)
                    {
                        Items.UpdateFoodLeft(world);
                    }
                    if (changedEquipment)
                    {
                        Creatures.UpdateCreature(world, playerData.entity);
                    }
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            InventoryUI.DrawInventory(ctx, world, selected, offset, category, inventory, "Inventory");
        }

        public void ItemAction(IContext ctx, World world, PlayerData playerData, int selection)
        {
            if (inventory.Count == 0 || selection >= inventory.Count) return;

            var oldLength = inventory.Count;
            var wrapper = inventory[selection];

            if (wrapper.type.IsEquipable())
            {
                if (wrapper.equipped)
                {
                    Items.UnequipItemToBackpack(world, playerData.entity, wrapper.type.GetEquipmentSlot());
                }
                else
                {
                    Items.EquipItem(world, playerData.entity, wrapper.entity);
                }
                changedEquipment = true;
                UpdateInventory(world, playerData);
            }
            else if (wrapper.type.IsUseable())
            {
                Items.IdentifyItem(world, inventory[selected + offset].entity);
                world.ecs.Assign(playerData.entity, new IntentionUseItem { item = wrapper.entity });
                world.SetState(ctx, RunState.Ticking);
            }

            UpdateSelection(oldLength, inventory.Count);
            FilterInventory();
        }

        private void FilterInventory()
        {
            Func<ItemWrapper, bool> predicate = category switch
            {
                InventoryCategory.Weapons => i => i.type.IsWeapon(),
                InventoryCategory.Armor => i => i.type.IsArmor(),
                InventoryCategory.Adornments => i => i.type.IsAdornment(),
                InventoryCategory.Consumables => i => i.type.IsConsumable(),
                InventoryCategory.Magic => i => i.type.IsMagic(),
                InventoryCategory.Tools => i => i.type.IsTool(),
                InventoryCategory.Misc => i => i.type.IsMisc(),
                _ => null,
            };
            if (predicate != null) inventory.Filter(predicate);
        }

        private void UpdateInventory(World world, PlayerData playerData) =>
            inventory = Inventory.FromOwned(world, playerData.entity);

        private void UpdateSelection(int oldLength, int newLength)
        {
            var actual = oldLength;
            while (actual > newLength)
            {
                if (selected + offset >= newLength)
                {
                    if (offset > 0)
                    {
                        offset--;
                    }
                    else
                    {
                        selected--;
                    }
                    actual--;
                }
                else if (actual != newLength)
                {
                    if (offset > 0)
                    {
                        offset--;
                    }
                    actual--;
                }
            }
        }
    }
}