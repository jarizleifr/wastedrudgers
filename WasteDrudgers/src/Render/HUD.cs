using System;
using System.Collections.Generic;
using System.Globalization;
using Blaggard.Common;
using Blaggard.Graphics;
using ManulECS;

using WasteDrudgers.Entities;
using WasteDrudgers.Level;

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

            foreach (var e in world.ecs.View<ActiveEffect, PlayerMarker>())
            {
                ref var a = ref world.ecs.GetRef<ActiveEffect>(e);
                if (a.effect.Type == EffectType.InflictPoison)
                {
                    c.Print(x, y, Locale.poisoned, Data.Colors.greenLight);
                    x += Locale.poisoned.Length;
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
                c.Print(x, y, ((AbbrStats)i).ToString(), ctx.Theme.caption);
                c.PutChar(x + 2, y, ':', ctx.Theme.text);

                var mod = stats[(StatType)i].Mod;
                var statColor = mod switch
                {
                    var _ when mod < 0 => ctx.Theme.critical,
                    var _ when mod > 0 => ctx.Theme.fortified,
                    _ => Data.Colors.white
                };
                c.Print(x + 5, y, stats[(StatType)i].Current.ToString(), statColor, Data.Colors.black, TextAlignment.Right);

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
            int o = rect.width < 14 ? 0 : 1;

            c.DefaultFore = ctx.Theme.windowFrame;
            c.DefaultBack = ctx.Theme.windowBackground;

            DrawTarget(ctx, world, c, 0, ctx.UIData.log.height + 1, playerData);

            c.Rect(rect.x, rect.y + 3, rect.width, rect.height - 3, ' ');
            c.Print(o, rect.y + 3, playerData.name, ctx.Theme.caption);

            var health = world.ecs.GetRef<Pools>(player);
            DrawPlayerPool(ctx, c, o, rect.y + 4, "VIG", health.vigor, Data.Colors.white, Data.Colors.blue, true);
            DrawPlayerPool(ctx, c, o, rect.y + 5, "HLT", health.health, Data.Colors.white, Data.Colors.redLight, true);

            var combat = world.ecs.GetRef<Combat>(player);
            DrawAttack(c, ctx.Theme, o, rect.y + 7, combat);
            DrawDefense(c, ctx.Theme, o, rect.y + 11, combat);
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

                var attacker = world.ecs.GetRef<Combat>(playerData.entity);
                var defender = world.ecs.GetRef<Combat>(target);

                var hitChance = Math.Max(1, (attacker.hitChance * (100 - defender.dodge)) / 100);
                layer.PutChar(x + 1, y, renderable.character, renderable.color);
                var cur = health.health.Current + health.vigor.Current;
                var max = health.health.Max + health.vigor.Max;
                DrawEnemyPool(ctx, layer, x + 4, y, 9, cur, max);
                layer.Print(x + 4 + 9 / 2, y + 1, $"Hit: {hitChance}%", ctx.Theme.text, TextAlignment.Center);
            }
        }

        public static void DrawAttack(IBlittable layer, Theme theme, int x, int y, Combat combat)
        {
            var wieldString = combat.wielding switch
            {
                Wielding.Unarmed => Locale.unarmed,
                Wielding.SingleWeapon => Locale.weapon,
                Wielding.DualWield => Locale.weapons,
            };
            layer.Print(x, y, wieldString, theme.caption);
            //layer.Print(x + 9, y, "(]{‼", Color.white);
            DrawPercentage(layer, theme, x, y + 1, 4, "", combat.hitChance);
            DrawCustomValue(layer, theme, x, y + 1, 13, "", combat.damage.ToString());
        }

        public static void DrawExperience(IBlittable layer, Theme theme, int x, int y, Experience exp)
        {
            DrawNumeric(layer, theme, x, y, 12, Locale.level, exp.level);
            DrawNumeric(layer, theme, x, y + 1, 12, Locale.next, Formulae.ExperienceNeededForLevel(exp.level + 1) - exp.experience);
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

        private static void DrawEnemyPool(IContext ctx, IBlittable layer, int x, int y, int l, int cur, int max)
        {
            int amount = (int)(l * ((float)cur / (float)max));
            layer.LineHoriz(x, y, amount, '=', Data.Colors.white);
        }

        private static void DrawPool(IContext ctx, IBlittable layer, int x, int y, int l, int cur, int max, Color fore, Color back, bool text = false)
        {
            int amount = (int)(l * ((float)cur / (float)max));

            layer.LineHoriz(x, y, l, ' ', fore, Data.Colors.black);
            layer.LineHoriz(x, y, amount, ' ', fore, back);

            if (text)
            {
                layer.Print(x + l / 2, y, string.Format("{0}/{1}", cur, max), fore, TextAlignment.Center);
            }
        }

        private static void DrawPlayerPool(IContext ctx, IBlittable layer, int x, int y, string caption, Stat stat, Color fore, Color back, bool text = false)
        {
            layer.Print(x, y, caption, ctx.Theme.text);
            //DrawPool(ctx, layer, x + 4, y, 9, stat.Current, stat.Max, color, ctx.Theme.black, true);
            var cur = stat.Current;
            var max = stat.Max;
            var l = 9;
            int amount = (int)(l * ((float)cur / (float)max));

            layer.LineHoriz(x + 4, y, l, ' ', fore, Data.Colors.black);
            layer.LineHoriz(x + 4, y, amount, ' ', fore, back);

            if (text)
            {
                layer.Print(x + 4 + l / 2, y, string.Format("{0}/{1}", cur, max), fore, TextAlignment.Center);
            }
        }

        private static void DrawDefense(IBlittable layer, Theme theme, int x, int y, Combat combat)
        {
            DrawPercentage(layer, theme, x, y, 13, Locale.dodge, combat.dodge);
            DrawNumeric(layer, theme, x, y + 1, 12, Locale.armor, combat.armor);
        }

        // TODO: Drawing colors for stat numbers.
        // TODO: Show modifiers via color
        private static void DrawNumeric(IBlittable layer, Theme theme, int x, int y, int offset, string caption, int value)
        {
            layer.Print(x, y, caption, theme.caption);
            layer.Print(x + offset, y, value.ToString(), theme.text, TextAlignment.Right);
        }

        private static void DrawPercentage(IBlittable layer, Theme theme, int x, int y, int offset, string caption, int value)
        {
            layer.Print(x, y, caption, theme.caption);
            layer.Print(x + offset, y, $"{value}%", theme.text, TextAlignment.Right);
        }

        private static void DrawCustomValue(IBlittable layer, Theme theme, int x, int y, int offset, string caption, string value)
        {
            layer.Print(x, y, caption, theme.caption);
            layer.Print(x + offset, y, value, theme.text, TextAlignment.Right);
        }

        private static void DrawModifier(IBlittable layer, Theme theme, int x, int y, int offset, string caption, int value)
        {
            layer.Print(x, y, caption, theme.caption);
            layer.Print(x + offset, y, StringUtils.ModifierToString(value), theme.text, TextAlignment.Right);
        }
    }
}