using System;

using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    [InputDomains("menu", "inventory", "selection")]
    public class PickUpState : GameScene
    {
        public int selected;
        public int offset;

        private PlayerData playerData;
        private Inventory inventory;

        public override void Initialize(IContext ctx, World world)
        {
            playerData = world.PlayerData;
            inventory = Inventory.FromItemsOnGround(world, playerData.coords);
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
                    PickUp(world, playerData, selected + offset);
                    break;

                case Command command when command >= Command.SelectA && command <= Command.SelectH:
                    PickUp(world, playerData, command - Command.SelectA + offset);
                    break;

                case Command.MenuUp:
                    (selected, offset) = Menu.Prev(selected, offset, view, l);
                    break;

                case Command.MenuDown:
                    (selected, offset) = Menu.Next(selected, offset, view, l);
                    break;

                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            // No point in showing if all items we're picked up
            if (inventory.Count == 0)
            {
                world.SetState(ctx, RunState.AwaitingInput);
                return;
            }

            InventoryUI.DrawInventory(ctx, world, selected, offset, 0, inventory, "Pick up items");
        }

        private void PickUp(World world, PlayerData playerData, int index)
        {
            if (index >= inventory.Count) return;

            var oldLength = inventory.Count;
            var wrapper = inventory[index];
            Items.PickUpItem(world, playerData.entity, wrapper.entity);
            inventory = Inventory.FromItemsOnGround(world, playerData.coords);
            UpdateSelection(oldLength, inventory.Count);
        }

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