using System;
using System.Globalization;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Render
{
    public static class HUD
    {
        private static Cell[] clock = new Cell[]
        {
            new Cell('▒', Data.Colors.blue, Data.Colors.shadow),
            new Cell('░', Data.Colors.blue, Data.Colors.shadow),
            new Cell(' ', Data.Colors.white, Data.Colors.shadow),
            new Cell('•', Data.Colors.white, Data.Colors.shadow),
            new Cell(' ', Data.Colors.white, Data.Colors.shadow),
            new Cell('░', Data.Colors.blue, Data.Colors.shadow),
            new Cell('▒', Data.Colors.blue, Data.Colors.shadow),
            new Cell('▓', Data.Colors.blue, Data.Colors.shadow),
            new Cell(' ', Data.Colors.bronzeLight, Data.Colors.blue),
            new Cell('☼', Data.Colors.bronzeLight, Data.Colors.blue),
            new Cell(' ', Data.Colors.bronzeLight, Data.Colors.blue),
            new Cell('▓', Data.Colors.blue, Data.Colors.shadow),
        };

        public static void DrawScreenBorders(IBlittable layer, Theme theme)
        {
            layer.DefaultFore = theme.windowFrame;
            layer.DefaultBack = theme.windowBackground;
            layer.PrintFrame(false);
            layer.ResetColors();
        }

        public static void DrawBoxes(IContext ctx)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var (log, sidebar, viewport) = ctx.UIData;

            c.DefaultFore = ctx.Theme.windowFrame;
            c.DefaultBack = ctx.Theme.windowBackground;

            c.LineVert(sidebar.width, log.height, c.Height - log.height, '║');
            c.LineHoriz(0, log.height, log.width, '═');
            c.PrintFrame(viewport);
            c.PutChar(sidebar.width, log.height, '╦');
            c.PutChar(sidebar.width, c.Height - 2, '╠');

            c.PutChar(viewport.x + viewport.width - 10, viewport.y, '╡');
            c.PutChar(viewport.x + viewport.width - 2, viewport.y, '╞');

            c.LineHoriz(viewport.x + 2, viewport.y + viewport.height - 1, viewport.width - 4, ' ', ctx.Theme.text, ctx.Theme.windowBackground);

            c.PutChar(viewport.x + 1, viewport.y + viewport.height - 1, '╡');
            c.PutChar(viewport.x + 37, viewport.y + viewport.height - 1, '╞');

            c.PutChar(viewport.x + 38, viewport.y + viewport.height - 1, '╡');
            c.PutChar(viewport.x + viewport.width - 2, viewport.y + viewport.height - 1, '╞');

            c.LineHoriz(0, log.height + 3, sidebar.width, '─');
            c.PutChar(sidebar.width, log.height + 3, '╢');
        }

        public static void DrawLog(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var log = ctx.UIData.log;

            c.ResetColors();
            c.Rect(log, ' ');

            int index = 0;
            foreach (var line in world.log.GetBuffer())
            {
                int offset = 0;
                foreach (var str in line)
                {
                    c.PrintColoredTextSpan(0 + offset, index, str);
                    offset += str.Length + 1;
                }
                index++;
            }
        }

        public static void DrawFooter(IContext ctx, World world, string description)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var (_, sidebar, viewport) = ctx.UIData;

            c.ResetColors();
            c.Rect(sidebar.width + 1, c.Height - 1, viewport.width - 1, 1, ' ');

            if (description != null)
            {
                c.Print(sidebar.width + 1, c.Height - 1, description, Data.Colors.white);
            }
        }

        public static void DrawStatusBar(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var viewport = ctx.UIData.viewport;
            var player = world.PlayerData.entity;

            var (x, y) = (viewport.x + 2, viewport.y);
            var hunger = world.ecs.GetRef<HungerClock>(player);
            if (hunger.State == HungerState.Hungry)
            {
                c.Print(x, y, Locale.hungry, Data.Colors.skinLight);
                x += Locale.hungry.Length;
            }
            var health = world.ecs.GetRef<Pools>(player);
            if (health.fatigued)
            {
                c.Print(x, y, Locale.fatigued, Data.Colors.fuchsiaLight);
                x += Locale.fatigued.Length;
            }

            if (world.ecs.TryGet<Afflictions>(player, out var afflictions))
            {
                if (afflictions.poison > 0)
                {
                    c.Print(x, y, Locale.poisoned, Data.Colors.greenLight);
                    x += Locale.fatigued.Length;
                }
            }

            c.PutChar(viewport.x + 1, viewport.y, '╡');
            c.PutChar(x, viewport.y, '╞');
        }

        private enum AbbrStats { ST, EN, FI, IN, RE, AW }
        public static void DrawStatsBar(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var viewport = ctx.UIData.viewport;

            var player = world.PlayerData.entity;

            var stats = world.ecs.GetRef<Stats>(player);
            var y = viewport.y + viewport.height - 1;

            for (int i = 0; i < 6; i++)
            {
                var x = viewport.x + 2 + i * 6;
                var type = (StatType)i;
                c.Print(x, y, type.Abbr().ToString(), ctx.Theme.caption);
                c.PutChar(x + 2, y, ':', ctx.Theme.text);

                var stat = stats[type];
                var mod = stat.Mod;
                var statColor = mod switch
                {
                    var _ when mod < 0 => ctx.Theme.critical,
                    var _ when mod > 0 => ctx.Theme.fortified,
                    _ => Data.Colors.white
                };
                c.Print(x + 5, y, stat.Current.ToString(), statColor, Data.Colors.black, TextAlignment.Right);

                if (i < 5)
                {
                    c.PutChar(x + 5, y, '─', ctx.Theme.windowFrame);
                }
            }

            var end = viewport.x + viewport.width;
            var actor = world.ecs.GetRef<Actor>(player);
            c.Print(end - 18, y, Locale.initiative, ctx.Theme.caption, TextAlignment.Right);
            c.PutChar(end - 18, y, ':', ctx.Theme.text);
            c.Print(end - 13, y, $"{actor.speed}%", ctx.Theme.text, TextAlignment.Right);
            c.PutChar(end - 13, y, '─', ctx.Theme.windowFrame);

            var clock = world.ecs.GetRef<HungerClock>(player);
            float food = ((float)clock.Total) / 1600f;

            var color = clock.State switch
            {
                HungerState.Hungry => ctx.Theme.critical,
                HungerState.LowFood => ctx.Theme.danger,
                _ => ctx.Theme.text
            };

            c.Print(end - 8, y, Locale.food, ctx.Theme.caption, TextAlignment.Right);
            c.PutChar(end - 8, y, ':', ctx.Theme.text);
            c.Print(end - 2, y, $"{food.ToString("0.0", CultureInfo.InvariantCulture)}d", color, TextAlignment.Right);
        }

        public static void DrawSidebar(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var playerData = world.PlayerData;
            var player = playerData.entity;

            var rect = ctx.UIData.sidebar;
            int y = rect.y;
            int o = rect.width < 14 ? 0 : 1;

            c.DefaultFore = ctx.Theme.windowFrame;
            c.DefaultBack = ctx.Theme.windowBackground;

            DrawTarget(ctx, world, c, 0, ctx.UIData.log.height + 1, playerData);

            c.Rect(rect.x, rect.y + 3, rect.width, rect.height - 3, ' ');
            c.Print(o, rect.y + 3, playerData.name, ctx.Theme.caption);

            var pools = world.ecs.GetRef<Pools>(player);

            c.PutChar(o + 1, y + 4, '↔', Data.Colors.blue);
            c.Print(o + 8, y + 4, $"{pools.vigor.Current}/{pools.vigor.Base}", Data.Colors.white, TextAlignment.Center);
            RenderUtils.DrawPool(c, o + 4, y + 4, 9, pools.vigor, Data.Colors.blue, Data.Colors.blueDark);

            c.PutChar(o + 1, y + 5, '♥', Data.Colors.redLight);
            c.Print(o + 8, y + 5, $"{pools.health.Current}/{pools.health.Base}", Data.Colors.white, TextAlignment.Center);
            RenderUtils.DrawPool(c, o + 4, y + 5, 9, pools.health, Data.Colors.redLight, Data.Colors.redDark);

            var attack = world.ecs.GetRef<Attack>(player);
            c.Print(o, rect.y + 7, Locale.attack, ctx.Theme.caption);
            c.Print(o + 4, rect.y + 8, $"{attack.hitChance}%", ctx.Theme.text, TextAlignment.Right);
            c.Print(o + 12, rect.y + 8, attack.DamageString, ctx.Theme.text, TextAlignment.Right);

            var defense = world.ecs.GetRef<Defense>(player);
            c.Print(o, rect.y + 11, Locale.parry, ctx.Theme.caption);
            c.Print(o + 13, rect.y + 11, $"{attack.parry}%", ctx.Theme.text, TextAlignment.Right);
            c.Print(o, rect.y + 12, Locale.armor, ctx.Theme.caption);
            c.Print(o + 12, rect.y + 12, defense.armor.ToString(), ctx.Theme.text, TextAlignment.Right);

            c.Print(o, rect.y + 14, Locale.evasion, ctx.Theme.caption);
            c.Print(o + 13, rect.y + 14, $"{defense.evasion}%", ctx.Theme.text, TextAlignment.Right);
            c.Print(o, rect.y + 15, Locale.fortitude, ctx.Theme.caption);
            c.Print(o + 13, rect.y + 15, $"{defense.fortitude}%", ctx.Theme.text, TextAlignment.Right);
            c.Print(o, rect.y + 16, Locale.mental, ctx.Theme.caption);
            c.Print(o + 13, rect.y + 16, $"{defense.mental}%", ctx.Theme.text, TextAlignment.Right);

            //layer.Print(o, rect.y + 22, "¢¶¥[=\"≈π!♀ôôδ", Color.white);
        }

        public static void DrawTarget(IContext ctx, World world, IBlittable layer, int x, int y, PlayerData playerData)
        {
            layer.Rect(x, y, 13, 2, ' ', ctx.Theme.windowFrame, Data.Colors.shadow);

            if (playerData.lastTarget.HasValue)
            {
                var target = playerData.lastTarget.Value;
                if (!world.ecs.IsAlive(target) || world.ecs.Has<Death>(target))
                {
                    playerData.lastTarget = null;
                    return;
                }

                var renderable = world.ecs.GetRef<Renderable>(target);
                var health = world.ecs.GetRef<Pools>(target);

                var attack = world.ecs.GetRef<Attack>(playerData.entity);
                var defense = world.ecs.GetRef<Defense>(target);

                var hitChance = Math.Max(1, (attack.hitChance * (100 - defense.parry)) / 100);
                layer.PutChar(x + 1, y, renderable.character, renderable.color);
                var cur = health.health.Current + health.vigor.Current;
                var max = health.health.Max + health.vigor.Max;

                int amount = (int)(9 * ((float)cur / (float)max));
                layer.LineHoriz(x + 4, y, amount, '=', Data.Colors.white);
                layer.Print(x + 4 + 9 / 2, y + 1, $"Hit: {hitChance}%", ctx.Theme.text, TextAlignment.Center);
            }
        }

        public static void DrawClock(IContext ctx, World world, int hours)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var viewport = ctx.UIData.viewport;
            var x = viewport.x + viewport.width - 9;
            for (int i = 0; i < 7; i++)
            {
                var ch = hours / 2 + i;
                if (ch >= clock.Length)
                {
                    ch -= clock.Length;
                }
                var cell = clock[ch];
                c.SetCell(x + i, viewport.y, cell.ch, cell.fore, cell.back);
            }
        }
    }
}