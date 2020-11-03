using System;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Render
{
    public static class InventoryUI
    {
        public const int INVENTORY_LENGTH = 18;
        private static string[] categories = Enum.GetNames(typeof(InventoryCategory));

        public static InventoryCategory NextCategory(InventoryCategory category)
        {
            category++;
            if (category > (InventoryCategory)categories.Length - 1)
            {
                category = 0;
            }
            return category;
        }
        public static InventoryCategory PrevCategory(InventoryCategory category)
        {
            category--;
            if (category < 0)
            {
                category = (InventoryCategory)categories.Length - 1;
            }
            return category;
        }

        public static void DrawInventory(IContext ctx, World world, int selected, int offset, InventoryCategory category, Inventory inventory, string caption)
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

            var inv = new Rect(rect.x + 1, rect.y + 4, rect.width - info.width - 1, INVENTORY_LENGTH);

            if (inventory.Count > inv.height)
            {
                Menu.DrawScrollBar(ctx, layer, inv.x + inv.width, inv.y - 1, inv.height + 1, offset, inventory.Count + 1);
            }

            DrawCategories(ctx, layer, rect.x + 1, rect.y + 1, category);
            DrawHeader(ctx, layer, inv, category);

            for (int i = 0; i < inv.height; i++)
            {
                if (i + offset >= inventory.Count) break;

                var wrapper = inventory[i + offset];
                var isSelected = i == selected;

                var item = world.ecs.GetRef<Item>(wrapper.entity);

                DrawItem(ctx, layer, world, i, inv, item, wrapper, category, isSelected);
                if (isSelected)
                {
                    DrawItemInfo(ctx, layer, world, info, item, wrapper);
                    DrawStats(ctx, layer, world, inv, wrapper, category);
                }
            }
        }

        private static void DrawItem(IContext ctx, IBlittable layer, World world, int i, Rect inv, Item item, ItemWrapper wrapper, InventoryCategory category, bool isSelected)
        {
            if (wrapper.equipped)
            {
                layer.PutChar(inv.x - 1, inv.y + i, '•', ctx.Colors.white);
            }

            layer.PutChar(inv.x, inv.y + i, (char)('A' + i), ctx.Theme.text);
            layer.PutChar(inv.x + 2, inv.y + i, GlyphUtil.GetItemGlyph(ctx, wrapper.type));

            var itemString = item.count > 1 ? $"({item.count}) {wrapper.name}" : wrapper.name;

            var unselectedColor = wrapper.equipped ? ctx.Theme.caption : ctx.Theme.text;
            var color = isSelected ? ctx.Theme.selectedColor : unselectedColor;

            layer.Print(inv.x + 4, inv.y + i, itemString, color);
            layer.Print(inv.x + inv.width, inv.y + i, $"{item.weight}", color, TextAlignment.Right);

            if (item.status != IdentificationStatus.Unknown)
            {
                switch (category)
                {
                    case InventoryCategory.Weapons:
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
                        break;
                    case InventoryCategory.Armor:
                        if (world.ecs.TryGet<Defense>(wrapper.entity, out var defense))
                        {
                            layer.Print(inv.x + inv.width - 6, inv.y + i, defense.armor.ToString(), color, TextAlignment.Right);
                            layer.Print(inv.x + inv.width - 11, inv.y + i, $"{defense.dodge}%", color, TextAlignment.Right);
                        }
                        break;
                }
            }
        }

        private static void DrawStats(IContext ctx, IBlittable layer, World world, Rect inv, ItemWrapper wrapper, InventoryCategory category)
        {
            // TODO: Stat differences, encumbrance
        }

        private static void DrawItemInfo(IContext ctx, IBlittable layer, World world, Rect info, Item item, ItemWrapper wrapper)
        {
            var rawMaterial = world.database.GetMaterial(item.material);

            layer.Rect(info.x + 1, info.y + 1, 3, 3, ' ', Color.black, Color.black);
            layer.PutChar(info.x + 2, info.y + 2, GlyphUtil.GetItemGlyph(ctx, wrapper.type), rawMaterial.Color);

            if (item.status == IdentificationStatus.Unknown)
            {
                layer.Print(info.x + 5, info.y + 1, "Unidentified", ctx.Theme.caption);
                layer.Print(info.x + 5, info.y + 2, item.type.ToString(), ctx.Theme.caption);
            }
            else
            {
                layer.Print(info.x + 5, info.y + 2, item.type.ToString(), ctx.Theme.caption);

                if (world.ecs.TryGet(wrapper.entity, out CastOnUse castOnUse))
                {
                    var rawSpell = world.database.GetSpell(castOnUse.spellId);
                    layer.Print(info.x + 1, info.y + 5, rawSpell.Name, ctx.Theme.caption);
                }

                if (item.type.IsEquipable())
                {
                    layer.Print(info.x + 5, info.y + 1, StringUtils.CapitalizeString(rawMaterial.Name), ctx.Theme.caption);

                    if (item.type.IsWeapon())
                    {
                        if (world.ecs.TryGet<Weapon>(wrapper.entity, out var attack))
                        {
                            layer.Print(info.x + 1, info.y + 5, "Attack", ctx.Theme.caption);
                            layer.Print(info.x + 14, info.y + 5, $"{attack.chance}%", ctx.Theme.text, TextAlignment.Right);
                            layer.Print(info.x + info.width - 1, info.y + 5, $"{attack.min}-{attack.max}", ctx.Theme.text, TextAlignment.Right);
                        }
                    }
                    else if (item.type.IsArmor())
                    {
                        if (world.ecs.TryGet<Defense>(wrapper.entity, out var defense))
                        {
                            layer.Print(info.x + 1, info.y + 5, "Defense", ctx.Theme.caption);
                            layer.Print(info.x + 14, info.y + 5, $"{defense.dodge}%", ctx.Theme.text, TextAlignment.Right);
                            layer.Print(info.x + info.width - 1, info.y + 5, $"{defense.armor}", ctx.Theme.text, TextAlignment.Right);
                        }
                    }
                }
            }
        }

        private static void DrawHeader(IContext ctx, IBlittable layer, Rect inv, InventoryCategory category)
        {
            // Captions for all items
            layer.LineHoriz(inv.x, inv.y - 1, inv.width, '─', ctx.Theme.text);
            layer.Print(inv.x + 4, inv.y - 1, "Name", ctx.Theme.text);
            layer.Print(inv.x + inv.width, inv.y - 1, "Wgt", ctx.Theme.text, TextAlignment.Right);

            switch (category)
            {
                case InventoryCategory.Weapons:
                    layer.Print(inv.x + inv.width - 6, inv.y - 1, "Dam", ctx.Theme.text, TextAlignment.Right);
                    layer.Print(inv.x + inv.width - 11, inv.y - 1, "Base", ctx.Theme.text, TextAlignment.Right);
                    break;
                case InventoryCategory.Armor:
                    layer.Print(inv.x + inv.width - 6, inv.y - 1, "Arm", ctx.Theme.text, TextAlignment.Right);
                    layer.Print(inv.x + inv.width - 11, inv.y - 1, "Phys", ctx.Theme.text, TextAlignment.Right);
                    break;
            }
        }

        private static void DrawCategories(IContext ctx, IBlittable layer, int x, int y, InventoryCategory categorySelected)
        {
            int offset = 0;
            for (int i = 0; i < categories.Length; i++)
            {
                layer.Print(x + offset, y, categories[i], i == (int)categorySelected ? ctx.Theme.selectedColor : ctx.Theme.text);
                offset += categories[i].Length + 1;
            }
        }
    }
}