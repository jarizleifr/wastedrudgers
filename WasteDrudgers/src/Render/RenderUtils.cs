using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Render
{
    public static class RenderUtils
    {
        ///<summary>Creates a centered 80x25 Rect for terminal sized stuff.</summary>
        public static Rect TerminalWindow(IContext ctx) => new Rect(0, 0, 80, 25);

        public static Rect OffsetTerminalWindow(IContext ctx)
        {
            var (offX, offY) = GetTerminalWindowOffsets(ctx);
            return new Rect(offX, offY, 80, 25);
        }
        public static (int offsetX, int offsetY) GetTerminalWindowOffsets(IContext ctx) =>
            ((ctx.Width - 80) / 2, (ctx.Height - 25) / 2);

        public static void DrawTitleScreen(IBlittable root)
        {
            var split = (int)(root.Height / 1.41f);
            var dark = split / 3;

            root.DefaultBack = Data.Colors.black;
            root.Rect(0, 0, root.Width, dark, ' ');
            root.LineHoriz(0, dark - 1, root.Width, '░', Data.Colors.violetDark);

            root.DefaultBack = Data.Colors.violetDark;
            root.Rect(0, dark, root.Width, split - dark, ' ');

            root.LineHoriz(0, dark, root.Width, '▒', Data.Colors.black);
            root.LineHoriz(0, dark + 1, root.Width, '░', Data.Colors.black);

            root.LineHoriz(0, split - 3, root.Width, '░', Data.Colors.violet);
            root.LineHoriz(0, split - 2, root.Width, '▒', Data.Colors.violet);
            root.LineHoriz(0, split - 1, root.Width, ' ', Data.Colors.violet, Data.Colors.violet);

            root.PutChar(28, 5, '•', Data.Colors.white);
            root.PutChar(74, 16, '•', Data.Colors.white);

            root.DefaultBack = Data.Colors.beigeDark;
            root.Rect(0, split, root.Width, root.Height, ' ');

            root.LineHoriz(0, split, root.Width, '░', Data.Colors.beige);
        }

        public static void DrawScrollBar(IContext ctx, IBlittable layer, int x, int y, int height, int offset, int itemsCount)
        {
            var scrollArea = height - 2;
            var tabPos = (float)offset / (itemsCount - 1 - scrollArea - 1);
            var scrollBarPos = (int)((scrollArea - 1) * tabPos);

            layer.PutChar(x, y, '▲');
            layer.PutChar(x, y + height - 1, '▼');
            layer.LineVert(x, y + 1, scrollArea, '▒', ctx.Theme.windowBackground, Data.Colors.shadow);
            layer.SetCell(x, y + 1 + scrollBarPos, '■', ctx.Theme.windowFrame, Data.Colors.shadow);
        }

        public static void DrawInfoSheetBox(IContext ctx, IBlittable layer, int x, int y)
        {
            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;
            layer.PrintFrame(x, y, 80, 23, true);
            layer.LineHoriz(x + 1, y + 4, 60, '═');
            layer.LineHoriz(x + 1, y + 5, 60, '─');
            layer.LineVert(x + 60, y + 1, 21, '║');
        }

        public static void DrawCharacterSheetInfo(IContext ctx, World world, IBlittable layer, int x, int y)
        {
            var player = world.PlayerData.entity;
            var pools = world.ecs.GetRef<Pools>(player);
            DrawPools(ctx, layer, x, y, pools);

            var stats = world.ecs.GetRef<Stats>(player);
            DrawCompactStats(ctx, layer, x, y + 4, stats);

            layer.DefaultFore = ctx.Theme.caption;
            layer.Print(x, y + 7, "Melee Damage");
            layer.Print(x + 17, y + 7, $"+{Formulae.MeleeDamage(stats)}", ctx.Theme.text, TextAlignment.Right);
            layer.Print(x, y + 8, "Max Carry");

            layer.Print(x, y + 10, "Evasion");
            layer.Print(x, y + 11, "Physical");
            layer.Print(x, y + 12, "Mental");

            layer.Print(x, y + 14, "Initiative");
            layer.Print(x + 18, y + 14, $"{Formulae.Speed(stats)}%", ctx.Theme.text, TextAlignment.Right);
            layer.Print(x, y + 15, "Move Speed");

            layer.Print(x, y + 17, "Vision");
            layer.Print(x, y + 18, "Heal Rate");
            layer.Print(x, y + 19, "Reaction");
        }

        public static void DrawPool(IBlittable layer, int x, int y, int length, Stat stat, Color fore, Color back)
        {
            int amount = (int)(length * ((float)stat.Current / (float)stat.Base));
            for (int i = 0; i < length; i++)
            {
                layer.SetCellBackground(x + i, y, i < amount ? fore : back);
            }
        }

        private static void DrawPools(IContext ctx, IBlittable layer, int x, int y, Pools pools)
        {
            layer.DefaultFore = ctx.Theme.caption;
            layer.Print(x, y, Locale.vigor);
            layer.Print(x, y + 1, Locale.health);
            layer.Print(x, y + 2, Locale.magic);

            for (int i = 0; i < 3; i++)
            {
                var stat = i switch
                {
                    0 => pools.vigor,
                    1 => pools.health,
                    _ => new Stat(0)
                };
                var (fore, back) = i switch
                {
                    0 => (Data.Colors.blue, Data.Colors.blueDark),
                    1 => (Data.Colors.fuchsia, Data.Colors.fuchsiaDark),
                    _ => (Data.Colors.violet, Data.Colors.violetDark),
                };
                var (cur, max) = (stat.Current, stat.Max);
                var l = 9;
                int amount = (int)(l * ((float)cur / (float)max));

                layer.LineHoriz(x + 8, y + i, l, ' ', fore, Data.Colors.black);
                layer.Print(x + 8 + l / 2, y + i, $"{cur}/{max}", fore, TextAlignment.Center);

                for (int j = 0; j < amount; j++)
                {
                    layer.SetCellBackground(x + 8 + j, y + i, back);
                }
            }
        }

        public static void DrawCharacterPoints(IContext ctx, World world, IBlittable layer, int x, int y)
        {
            var exp = world.ecs.GetRef<Experience>(world.PlayerData.entity);
            var i = Locale.characterPoints.Length;
            layer.Print(x, y, Locale.characterPoints);
            var c = exp.characterPoints switch
            {
                0 => Data.Colors.grey,
                _ => ctx.Theme.text,
            };
            layer.PutChar(x + i, y, ':');
            layer.Print(x + i + 1, y, exp.characterPoints.ToString(), c);
        }

        public static void DrawCompactStats(IContext ctx, IBlittable layer, int x, int y, Stats stats)
        {
            for (int i = 0; i < 6; i++)
            {
                var stat = (StatType)i;
                layer.Print(x + i * 3, y, stat.Abbr(), ctx.Theme.caption);
                layer.Print(x + i * 3, y + 1, stats[stat].Base.ToString(), ctx.Theme.text);
            }
        }

        // TODO: Draw a black line for now, figure out a way to make this reflect input config
        public static void DrawControlHelpBar(IContext ctx, IBlittable layer, int x, int y)
        {
            layer.LineHoriz(x, y, 80, ' ', Data.Colors.white, Data.Colors.black);
        }
    }

    public static class LayerExtensions
    {
        public static void CaptionValue(this IBlittable layer, Theme theme, int x, int y, int offset, object caption, object value, TextAlignment alignment = TextAlignment.Right)
        {
            layer.Print(x, y, caption.ToString(), theme.caption);
            layer.Print(x + offset, y, value.ToString(), theme.text, alignment);
        }
    }
}