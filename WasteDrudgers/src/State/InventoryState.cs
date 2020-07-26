using System;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    public class InventoryState : IRunState
    {
        public int selected;
        public int offset;
        public int category;

        public string[] InputDomains { get; set; } = { "menu", "inventory", "selection" };

        private PlayerData playerData;
        private Inventory inventory;

        public void Initialize(IContext ctx, World world)
        {
            playerData = world.ecs.FetchResource<PlayerData>();
            UpdateInventory(world, playerData);
        }

        public void Run(IContext ctx, World world)
        {
            var menu = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.TerminalWindow(ctx);

            int l = inventory.Count;
            int view = Math.Min(l, InventoryUI.INVENTORY_LENGTH);

            switch (ctx.Command)
            {
                // TODO: Implement contextual action for item equip/use/throw/drop
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
                        }
                        Items.DropItem(world, playerData.entity, wrapper.entity);
                        UpdateInventory(world, playerData);
                        UpdateSelection(oldLength, inventory.Count);
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
                    (selected, offset) = InventoryUI.Prev(selected, offset, view, l);
                    break;

                case Command.MenuDown:
                    (selected, offset) = InventoryUI.Next(selected, offset, view, l);
                    break;

                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            Views.DrawGameView(ctx, world);
            InventoryUI.DrawInventory(ctx, world, selected, offset, category, inventory, "Inventory");
        }

        public void ItemAction(IContext ctx, World world, PlayerData playerData, int selection)
        {
            if (inventory.Count == 0) return;

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
                1 => i => i.type.IsWeapon(),
                2 => i => i.type.IsApparel(),
                3 => i => i.type.IsUseable(),
                4 => i => i.type.IsMisc(),
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