using System.Collections.Generic;

using Blaggard.Common;
using Blaggard.Graphics;

using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.Render
{
    public class HUDData
    {
        public readonly Actor actor;
        public readonly Health health;
        public readonly Stats stats;

        public readonly Combat combat;

        public HUDData(World world, PlayerData data)
        {
            actor = world.ecs.GetRef<Actor>(data.entity);
            health = world.ecs.GetRef<Health>(data.entity);
            stats = world.ecs.GetRef<Stats>(data.entity);
            combat = world.ecs.GetRef<Combat>(data.entity);
        }
    }

    public static class HUD
    {
        public static void DrawScreenBorders(IBlittable layer, Theme theme)
        {
            layer.DefaultFore = theme.windowFrame;
            layer.DefaultBack = theme.windowBackground;
            layer.PrintFrame(false);
            layer.ResetColors();
        }

        public static void Draw(IContext ctx, IBlittable layer, World world, PlayerData playerData)
        {
            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;

            layer.LineVert(ctx.UIData.sidebar.width, ctx.UIData.log.height, layer.Height - ctx.UIData.log.height, '║');
            layer.LineHoriz(0, ctx.UIData.log.height, ctx.UIData.log.width, '═');
            layer.PrintFrame(ctx.UIData.viewport);

            layer.PutChar(ctx.UIData.sidebar.width, ctx.UIData.log.height, '╦');
            layer.PutChar(ctx.UIData.sidebar.width, layer.Height - 2, '╠');

            HUD.DrawSidebar(ctx, layer, world, playerData);
            HUD.DrawFooter(ctx, layer, LevelUtils.GetDescription(world, playerData.coords));
            HUD.DrawLog(layer, ctx.UIData, world.log.GetBuffer());
        }

        public static void DrawLog(IBlittable layer, UIData uiData, IEnumerable<ColoredString> lines)
        {
            layer.ResetColors();
            layer.Rect(uiData.log, ' ');

            int index = 0;
            foreach (var line in lines)
            {
                layer.PrintColoredString(0, index, line);
                index++;
            }
        }

        public static void DrawFooter(IContext ctx, IBlittable layer, string text)
        {
            layer.ResetColors();
            layer.Rect(ctx.UIData.sidebar.width + 1, layer.Height - 1, ctx.UIData.viewport.width - 1, 1, ' ');
            if (text != null)
            {
                layer.Print(ctx.UIData.sidebar.width + 1, layer.Height - 1, text, ctx.Theme.white);
            }
        }

        public static void DrawSidebar(IContext ctx, IBlittable layer, World world, PlayerData playerData)
        {
            var data = new HUDData(world, playerData);
            var targetData = TargetData.Create(world, playerData);

            var rect = ctx.UIData.sidebar;
            int o = rect.width < 14 ? 0 : 1;

            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;
            layer.Rect(rect, ' ');
            DrawTarget(ctx, layer, rect.width + o, ctx.UIData.log.height, targetData);

            layer.Print(o, rect.y + 3, playerData.name, ctx.Theme.caption);
            DrawPlayerPool(ctx, layer, o, rect.y + 4, "VIG", data.health.vigor, ctx.Theme.vigor, ctx.Theme.vigorDark, true);
            DrawPlayerPool(ctx, layer, o, rect.y + 5, "HLT", data.health.health, ctx.Theme.health, ctx.Theme.healthDark, true);

            DrawAttack(layer, ctx.Theme, o, rect.y + 7, data.combat);
            DrawDefense(layer, ctx.Theme, o, rect.y + 11, data.combat);
            DrawStats(layer, ctx.Theme, o, rect.y + 14, data.stats);
            DrawPercentage(layer, ctx.Theme, o, rect.y + 18, 13, "Speed", data.actor.speed);

            //layer.Print(o, rect.y + 22, "¢¶¥[=\"≈π!♀ôôδ", Color.white);

            DrawCustomValue(layer, ctx.Theme, o, rect.y + 19, 13, "Food", "½ day");
        }

        // FIXME: Target doesn't always clear for some reason, related to level change oddities?
        public static void DrawTarget(IContext ctx, IBlittable layer, int x, int y, TargetData targetData)
        {
            /*
            layer.Rect(x, y, 13, 2, ' ', ctx.Theme.windowFrame, ctx.Theme.shadowDark);
            if (targetData != null)
            {
                layer.PutChar(x + 1, y, targetData.renderable.character, targetData.renderable.color);
                var cur = targetData.health.health.Current + targetData.health.vigor.Current;
                var max = targetData.health.health.Max + targetData.health.vigor.Max;
                DrawEnemyPool(ctx, layer, x + 4, y, 9, cur, max);
                layer.Print(x + 4 + 9 / 2, y + 1, $"Hit: {targetData.hitChance}%", ctx.Theme.text, TextAlignment.Center);
            }
            */
            if (targetData != null)
            {
                layer.PutChar(x + 2, y, targetData.renderable.character, targetData.renderable.color);
                layer.PutChar(x + 3, y, ':', ctx.Theme.text);
                var cur = targetData.health.health.Current + targetData.health.vigor.Current;
                var max = targetData.health.health.Max + targetData.health.vigor.Max;
                DrawEnemyPool(ctx, layer, x + 4, y, 9, cur, max);
                layer.Print(x + 14, y, $"Hit: {targetData.hitChance}%", ctx.Theme.text);
            }
        }

        public static void DrawAttack(IBlittable layer, Theme theme, int x, int y, Combat combat)
        {
            var wieldString = combat.wielding switch
            {
                Wielding.Unarmed => "Unarmed",
                Wielding.SingleWeapon => "Weapon",
                Wielding.DualWield => "Weapons"
            };
            layer.Print(x, y, wieldString, theme.caption);
            //layer.Print(x + 9, y, "(]{‼", Color.white);
            DrawPercentage(layer, theme, x, y + 1, 4, "", combat.hitChance);
            DrawCustomValue(layer, theme, x, y + 1, 13, "", $"{combat.minDamage}─{combat.maxDamage}");
        }

        /*
        for (int i = 0; i < values.Length; i++)
        {
            //ctx.Canvas.Print(x + offset, y + i, values[i], TextAlignment.Right);
        }*/

        /*
        public static void DrawClock(Context ctx, int hours)
        {
            string clockString = "   •   ░▒▓▒░   ☼   ░▒▓▒░";
            int x = ctx.Canvas.Width - 9;
            int y = ctx.UIData.log.height;

            byte[] chars = Encoding.GetEncoding(437).GetBytes(clockString);

            Color fore, back;
            if (hours > 6 && hours < 18)
            {
                fore = Color.white;
                back = Color.grey;
            }
            else
            {
                fore = Color.white;
                back = Color.black;
            }

            for (int i = 0; i < 7; i++)
            {
                var ch = hours + i;
                if (ch >= clockString.Length)
                {
                    ch -= clockString.Length;
                }
                ctx.Canvas.PutCharEx(x + i, y, chars[ch], fore, back);
            }
        }*/

        private static void DrawEnemyPool(IContext ctx, IBlittable layer, int x, int y, int l, int cur, int max)
        {
            int amount = (int)(l * ((float)cur / (float)max));
            layer.LineHoriz(x, y, amount, '=', ctx.Theme.white);
        }

        private static void DrawPool(IContext ctx, IBlittable layer, int x, int y, int l, int cur, int max, Color fore, Color back, bool text = false)
        {
            int amount = (int)(l * ((float)cur / (float)max));

            layer.LineHoriz(x, y, l, ' ', fore, ctx.Theme.black);
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

            layer.LineHoriz(x + 4, y, l, ' ', fore, ctx.Theme.black);
            layer.LineHoriz(x + 4, y, amount, ' ', fore, back);

            if (text)
            {
                layer.Print(x + 4 + l / 2, y, string.Format("{0}/{1}", cur, max), fore, TextAlignment.Center);
            }
        }

        private static void DrawStats(IBlittable layer, Theme theme, int x, int y, Stats stats)
        {
            DrawNumeric(layer, theme, x, y, 6, "Str", stats.strength.Current);
            DrawNumeric(layer, theme, x, y + 1, 6, "End", stats.endurance.Current);
            DrawNumeric(layer, theme, x, y + 2, 6, "Fin", stats.finesse.Current);

            DrawNumeric(layer, theme, x + 7, y, 6, "Int", stats.intellect.Current);
            DrawNumeric(layer, theme, x + 7, y + 1, 6, "Res", stats.resolve.Current);
            DrawNumeric(layer, theme, x + 7, y + 2, 6, "Awa", stats.awareness.Current);
        }

        private static void DrawDefense(IBlittable layer, Theme theme, int x, int y, Combat combat)
        {
            DrawPercentage(layer, theme, x, y, 13, "Dodge", combat.dodge);
            DrawNumeric(layer, theme, x, y + 1, 12, "Armor", combat.armor);
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