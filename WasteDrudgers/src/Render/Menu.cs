using System;
using System.Linq;
using Blaggard.Common;
using Blaggard.Graphics;

namespace WasteDrudgers.Render
{
    public static class Menu
    {
        public static void DrawMenu(IBlittable layer, Theme theme, int x, int y, int selected, params string[] items)
        {
            var menuWidth = items.Max(i => i.Length);
            var rect = new Rect(x - 2 - menuWidth / 2, y - 2, menuWidth + 4, 3 + items.Length * 2);

            layer.DefaultFore = theme.windowFrame;
            layer.DefaultBack = theme.windowBackground;
            layer.PrintFrame(rect, true);

            for (int i = 0; i < items.Length; i++)
            {
                if (selected == i)
                {
                    layer.Print(x, y + i * 2, items[i], theme.selectedColor, TextAlignment.Center);
                }
                else
                {
                    layer.Print(x, y + i * 2, items[i], theme.text, TextAlignment.Center);
                }
            }
            layer.ResetColors();
        }

        public static void DrawConfigMenu(IBlittable layer, Theme theme, int x, int y, int selected, params (string key, string value)[] items)
        {
            var menuWidth = items.Max(i => i.key.Length) + 20;
            var rect = new Rect(x - 2 - menuWidth / 2, y - 2, menuWidth + 4, 3 + items.Length * 2);

            layer.DefaultFore = theme.windowFrame;
            layer.DefaultBack = theme.windowBackground;
            layer.PrintFrame(rect, true);

            for (int i = 0; i < items.Length; i++)
            {
                if (selected == i)
                {
                    layer.Print(rect.x + 2, y + i * 2, items[i].key, theme.selectedColor);
                    layer.Print(rect.x + rect.width - 2, y + i * 2, items[i].value, theme.selectedColor, TextAlignment.Right);
                }
                else
                {
                    layer.Print(rect.x + 2, y + i * 2, items[i].key, theme.text);
                    layer.Print(rect.x + rect.width - 2, y + i * 2, items[i].value, theme.text, TextAlignment.Right);
                }
            }
            layer.ResetColors();
        }

        public static void DrawScrollBar(IContext ctx, IBlittable layer, int x, int y, int height, int offset, int itemsCount)
        {
            var scrollArea = height - 2;
            var tabPos = (float)offset / (itemsCount - 1 - scrollArea - 1);
            var scrollBarPos = (int)((scrollArea - 1) * tabPos);

            layer.PutChar(x, y, '▲');
            layer.PutChar(x, y + height - 1, '▼');
            layer.LineVert(x, y + 1, scrollArea, '▒', ctx.Theme.windowBackground, ctx.Colors.shadow);
            layer.PutChar(x, y + 1 + scrollBarPos, '■', ctx.Theme.windowFrame, ctx.Colors.shadow);
        }

        public static void DrawCompactMenu<T>(IContext ctx, IBlittable layer, Rect rect, int selected, int offset, T[] items) where T : IMenuItem
        {
            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;
            layer.PrintFrame(rect, true);

            var view = rect.Contract(1);
            var scrollBarX = rect.x + rect.width - 1;
            if (items.Length > view.height)
            {
                DrawScrollBar(ctx, layer, scrollBarX, view.y, view.height, offset, items.Length);
            }

            for (int i = 0; i < view.height; i++)
            {
                if (i + offset >= items.Length) break;
                items[i + offset].Render(ctx, layer, rect, i, i == selected);
            }
            layer.ResetColors();
        }

        public static void DrawValueMenu(IBlittable layer, Theme theme, int x, int y, int selected, params (string key, string value)[] items)
        {
            var menuWidth = items.Max(i => i.key.Length) + 20;
            var rect = new Rect(x - 2 - menuWidth / 2, y - 2, menuWidth + 4, 3 + items.Length * 2);

            layer.DefaultFore = theme.windowFrame;
            layer.DefaultBack = theme.windowBackground;
            layer.PrintFrame(rect, true);

            for (int i = 0; i < items.Length; i++)
            {
                if (selected == i)
                {
                    layer.Print(rect.x + 2, y + i * 2, items[i].key, theme.selectedColor);
                    layer.Print(rect.x + rect.width - 2, y + i * 2, items[i].value, theme.selectedColor, TextAlignment.Right);
                }
                else
                {
                    layer.Print(rect.x + 2, y + i * 2, items[i].key, theme.text);
                    layer.Print(rect.x + rect.width - 2, y + i * 2, items[i].value, theme.text, TextAlignment.Right);
                }
            }
            layer.ResetColors();
        }

        public static int Prev(int selected, int length) => --selected < 0 ? length - 1 : selected;
        public static int Next(int selected, int length) => ++selected > length - 1 ? 0 : selected;

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
    }
}