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

        public static void DrawCompactMenu<T>(IContext ctx, IBlittable layer, Rect rect, int selected, T[] items) where T : IMenuItem
        {
            if (items.Length > rect.height) throw new Exception("Menu item count exceeds allocated space!");

            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;
            layer.PrintFrame(rect, true);

            for (int i = 0; i < items.Length; i++)
            {
                items[i].Render(ctx, layer, rect, i, i == selected);
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
    }
}