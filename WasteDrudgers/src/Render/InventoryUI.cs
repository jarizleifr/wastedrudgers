using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Render
{
    public static class InventoryUI
    {
        public const int INVENTORY_LENGTH = 19;
        private static string[] categories = { "All", "Weapons", "Armor", "Adornments", "Consumables", "Magic", "Tools", "Misc" };

        public static (int s, int o) Prev(int selected, int offset, int view, int length)
        {
            if (selected > 0) selected--;
            else if (selected == 0 && offset > 0) offset--;
            return (selected, offset);
        }
        public static (int s, int o) Next(int selected, int offset, int view, int length)
        {
            if (selected < view - 1) selected++;
            else if (selected == view - 1 && selected + offset < length - 1) offset++;
            return (selected, offset);
        }
        public static int NextCategory(int category)
        {
            category++;
            if (category > categories.Length - 1)
            {
                category = 0;
            }
            return category;
        }
        public static int PrevCategory(int category)
        {
            category--;
            if (category < 0)
            {
                category = categories.Length - 1;
            }
            return category;
        }

        public static void DrawInventory(IContext ctx, World world, int selected, int offset, int category, Inventory inventory, string caption)
        {
            var offsets = RenderUtils.GetTerminalWindowOffsets(ctx);
            var layer = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            layer.SetRenderPosition(offsets.offsetX, offsets.offsetY);

            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;

            var rect = RenderUtils.TerminalWindow(ctx);
            var info = new Rect(rect.width - 22, 0, 22, rect.height);

            layer.PrintFrame(rect, true);
            layer.PrintFrame(info, true);

            layer.LineHoriz(1, 2, rect.width - info.width - 1, '═');
            layer.LineHoriz(1, rect.height - 3, rect.width - info.width - 1, '═');

            layer.PutChar(0, 2, '╠');
            layer.PutChar(info.x, 2, '╣');
            layer.PutChar(0, rect.height - 3, '╠');
            layer.PutChar(info.x, rect.height - 3, '╣');

            layer.PutChar(info.x, 0, '╦');
            layer.PutChar(info.x, rect.height - 1, '╩');

            layer.Print(rect.x + (rect.width - info.width) / 2, rect.y, caption, ctx.Theme.text, TextAlignment.Center);

            DrawCategories(ctx, layer, rect.x + 1, rect.y + 1, category);

            var inv = new Rect(rect.x + 1, rect.y + 4, rect.width - info.width - 1, INVENTORY_LENGTH);

            // Default captions
            layer.LineHoriz(inv.x, inv.y - 1, inv.width, '─', ctx.Theme.text);
            layer.Print(inv.x + 4, inv.y - 1, "Name", ctx.Theme.text);
            layer.Print(inv.x + inv.width, inv.y - 1, "Wgt", ctx.Theme.text, TextAlignment.Right);

            if (category == 1)
            {
                layer.Print(inv.x + inv.width - 6, inv.y - 1, "Dam", ctx.Theme.text, TextAlignment.Right);
                layer.Print(inv.x + inv.width - 11, inv.y - 1, "Base", ctx.Theme.text, TextAlignment.Right);
            }
            else if (category == 2)
            {
                layer.Print(inv.x + inv.width - 6, inv.y - 1, "Arm", ctx.Theme.text, TextAlignment.Right);
                layer.Print(inv.x + inv.width - 11, inv.y - 1, "Phys", ctx.Theme.text, TextAlignment.Right);
            }

            for (int i = 0; i < inv.height; i++)
            {
                if (i + offset >= inventory.Count) break;
                var wrapper = inventory[i + offset];

                var unselected = wrapper.equipped ? ctx.Theme.caption : ctx.Theme.text;
                var color = selected == i ? ctx.Theme.selectedColor : unselected;

                var item = world.ecs.GetRef<Item>(wrapper.entity);

                if (wrapper.equipped)
                {
                    layer.PutChar(rect.x, inv.y + i, '•', ctx.Theme.white);
                }

                layer.PutChar(inv.x, inv.y + i, (char)('A' + i), ctx.Theme.text);
                layer.PutChar(inv.x + 2, inv.y + i, GlyphUtil.GetItemGlyph(ctx, wrapper.type));
                var itemString = item.count > 1 ? $"({item.count}) {wrapper.name}" : wrapper.name;
                layer.Print(inv.x + 4, inv.y + i, itemString, color);

                layer.Print(info.x, inv.y + i, $"{item.weight}", color, TextAlignment.Right);

                if (item.status != IdentificationStatus.Unknown)
                {
                    if (category == 1)
                    {
                        if (world.ecs.TryGet<Weapon>(wrapper.entity, out var attack))
                        {
                            layer.Print(inv.x + inv.width - 6, inv.y + i, StringUtils.AverageDamageToString(attack.Average), color, TextAlignment.Right);
                            layer.Print(inv.x + inv.width - 11, inv.y + i, $"{attack.chance}%", color, TextAlignment.Right);
                        }
                        // TODO: If we implement shield bashes, we can probably just make shields into regular weapons
                        else if (world.ecs.TryGet<Shield>(wrapper.entity, out var shield))
                        {
                            layer.Print(inv.x + inv.width - 11, inv.y + i, $"{shield.baseBlock}%", color, TextAlignment.Right);
                        }
                    }
                    else if (category == 2)
                    {
                        if (world.ecs.TryGet<Defense>(wrapper.entity, out var defense))
                        {
                            layer.Print(inv.x + inv.width - 6, inv.y + i, defense.armor.ToString(), color, TextAlignment.Right);
                            layer.Print(inv.x + inv.width - 11, inv.y + i, $"{defense.dodge}%", color, TextAlignment.Right);
                        }
                    }
                }

                if (selected == i)
                {
                    layer.Print(info.x + 1, info.y + 1, wrapper.name, ctx.Theme.caption);
                }
            }
        }

        private static void DrawCategories(IContext ctx, IBlittable layer, int x, int y, int categorySelected)
        {
            int offset = 0;
            for (int i = 0; i < categories.Length; i++)
            {
                layer.Print(x + offset, y, categories[i], i == categorySelected ? ctx.Theme.selectedColor : ctx.Theme.text);
                offset += categories[i].Length + 1;
            }
        }
    }
}