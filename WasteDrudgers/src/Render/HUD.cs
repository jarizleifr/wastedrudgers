using System.Collections.Generic;
using System.Globalization;
using Blaggard.Common;
using Blaggard.Graphics;
using ManulECS;

using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.Render
{
    // TODO: We should cache hud information somewhere and have dirty flags to prevent unnecessary updates
    public class HUDData
    {
        public readonly Actor actor;
        public readonly Health health;
        public readonly Stats stats;

        public readonly Combat combat;
        public readonly Experience exp;

        public readonly HungerClock hunger;

        public readonly List<StatusInfo> statusInfo;

        public HUDData(World world, PlayerData data)
        {
            actor = world.ecs.GetRef<Actor>(data.entity);
            health = world.ecs.GetRef<Health>(data.entity);
            stats = world.ecs.GetRef<Stats>(data.entity);
            combat = world.ecs.GetRef<Combat>(data.entity);
            exp = world.ecs.GetRef<Experience>(data.entity);
            hunger = world.ecs.GetRef<HungerClock>(data.entity);

            statusInfo = new List<StatusInfo>();

            if (hunger.State == HungerState.Hungry)
            {
                statusInfo.Add(new StatusInfo { text = "Hungry", color = world.database.GetColor("c_skin_light") });
            }

            if (health.fatigued)
            {
                statusInfo.Add(new StatusInfo { text = "Fatigued", color = world.database.GetColor("c_fuchsia_light") });
            }

            world.ecs.Loop((Entity entity, ref ActiveEffect effect, ref PlayerMarker p) =>
            {
                if (effect.effect == SpellEffect.InflictPoison)
                {
                    statusInfo.Add(new StatusInfo { text = "Poisoned", color = world.database.GetColor("c_green_light") });
                }
            });
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
                c.PrintColoredString(0, index, line);
                index++;
            }
        }

        public static void DrawFooter(IContext ctx, World world, Vec2? position = null)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var (_, sidebar, viewport) = ctx.UIData;

            c.ResetColors();
            c.Rect(sidebar.width + 1, c.Height - 1, viewport.width - 1, 1, ' ');

            var text = position == null
                ? LevelUtils.GetDescription(world, world.ecs.FetchResource<PlayerData>().coords)
                : LevelUtils.GetLookDescription(world, position.Value);

            if (text != null)
            {
                c.Print(sidebar.width + 1, c.Height - 1, text, ctx.Theme.white);
            }
        }

        public static IEnumerable<StatusInfo> GetStatuses(World world, Entity player)
        {
            var statusInfo = new List<StatusInfo>();
            var hunger = world.ecs.GetRef<HungerClock>(player);
            if (hunger.State == HungerState.Hungry)
            {
                statusInfo.Add(new StatusInfo { text = "Hungry", color = world.database.GetColor("c_skin_light") });
            }
            var health = world.ecs.GetRef<Health>(player);
            if (health.fatigued)
            {
                statusInfo.Add(new StatusInfo { text = "Fatigued", color = world.database.GetColor("c_fuchsia_light") });
            }
            world.ecs.Loop((Entity entity, ref ActiveEffect effect, ref PlayerMarker p) =>
            {
                if (effect.effect == SpellEffect.InflictPoison)
                {
                    statusInfo.Add(new StatusInfo { text = "Poisoned", color = world.database.GetColor("c_green_light") });
                }
            });
            return statusInfo;
        }

        public static void DrawStatusBar(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var viewport = ctx.UIData.viewport;

            var player = world.ecs.FetchResource<PlayerData>().entity;

            int offset = 2;
            var statusInfo = GetStatuses(world, player);
            foreach (var status in GetStatuses(world, player))
            {
                c.Print(viewport.x + offset, viewport.y, status.text, status.color);
                offset += status.text.Length;
            }

            c.PutChar(viewport.x + 1, viewport.y, '╡');
            c.PutChar(viewport.x + offset, viewport.y, '╞');
        }

        private enum AbbrStats { ST, EN, FI, IN, RE, AW }
        public static void DrawStatsBar(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var viewport = ctx.UIData.viewport;

            var player = world.ecs.FetchResource<PlayerData>().entity;

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
                    _ => ctx.Theme.text
                };
                c.Print(x + 5, y, stats[(StatType)i].Current.ToString(), statColor, TextAlignment.Right);

                if (i < 5)
                {
                    c.PutChar(x + 5, y, '─', ctx.Theme.windowFrame);
                }
            }

            var end = viewport.x + viewport.width;
            var actor = world.ecs.GetRef<Actor>(player);
            c.Print(end - 22, y, "Action", ctx.Theme.caption, TextAlignment.Right);
            c.PutChar(end - 22, y, ':', ctx.Theme.text);
            c.Print(end - 17, y, $"{actor.speed}%", ctx.Theme.text, TextAlignment.Right);
            c.PutChar(end - 17, y, '─', ctx.Theme.windowFrame);

            var clock = world.ecs.GetRef<HungerClock>(player);
            float food = ((float)clock.Total) / 1600f;

            var color = clock.State switch
            {
                HungerState.Hungry => ctx.Theme.critical,
                HungerState.LowFood => ctx.Theme.danger,
                _ => ctx.Theme.text
            };

            c.Print(end - 12, y, "Food", ctx.Theme.caption, TextAlignment.Right);
            c.PutChar(end - 12, y, ':', ctx.Theme.text);
            c.Print(end - 2, y, $"{food.ToString("0.0", CultureInfo.InvariantCulture)} days", color, TextAlignment.Right);
        }

        public static void DrawSidebar(IContext ctx, World world)
        {
            var c = ctx.GetCanvas(RenderLayer.Root);
            var playerData = world.ecs.FetchResource<PlayerData>();
            var player = playerData.entity;

            var targetData = TargetData.Create(world, playerData);

            var rect = ctx.UIData.sidebar;
            int o = rect.width < 14 ? 0 : 1;

            c.DefaultFore = ctx.Theme.windowFrame;
            c.DefaultBack = ctx.Theme.windowBackground;

            DrawTarget(ctx, c, 0, ctx.UIData.log.height + 1, targetData);

            c.Rect(rect.x, rect.y + 3, rect.width, rect.height - 3, ' ');
            c.Print(o, rect.y + 3, playerData.name, ctx.Theme.caption);

            var health = world.ecs.GetRef<Health>(player);
            DrawPlayerPool(ctx, c, o, rect.y + 4, "VIG", health.vigor, ctx.Theme.vigor, ctx.Theme.vigorDark, true);
            DrawPlayerPool(ctx, c, o, rect.y + 5, "HLT", health.health, ctx.Theme.health, ctx.Theme.healthDark, true);

            var combat = world.ecs.GetRef<Combat>(player);
            DrawAttack(c, ctx.Theme, o, rect.y + 7, combat);
            DrawDefense(c, ctx.Theme, o, rect.y + 11, combat);
            //layer.Print(o, rect.y + 22, "¢¶¥[=\"≈π!♀ôôδ", Color.white);
        }

        // FIXME: Target doesn't always clear for some reason, related to level change oddities?
        public static void DrawTarget(IContext ctx, IBlittable layer, int x, int y, TargetData targetData)
        {
            layer.Rect(x, y, 13, 2, ' ', ctx.Theme.windowFrame, ctx.Theme.shadowDark);
            if (targetData != null)
            {
                layer.PutChar(x + 1, y, targetData.renderable.character, targetData.renderable.color);
                var cur = targetData.health.health.Current + targetData.health.vigor.Current;
                var max = targetData.health.health.Max + targetData.health.vigor.Max;
                DrawEnemyPool(ctx, layer, x + 4, y, 9, cur, max);
                layer.Print(x + 4 + 9 / 2, y + 1, $"Hit: {targetData.hitChance}%", ctx.Theme.text, TextAlignment.Center);
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

        public static void DrawExperience(IBlittable layer, Theme theme, int x, int y, Experience exp)
        {
            DrawNumeric(layer, theme, x, y, 12, "Level", exp.level);
            DrawNumeric(layer, theme, x, y + 1, 12, "Next", Formulae.ExperienceNeededForLevel(exp.level + 1) - exp.experience);
        }

        public static void DrawClock(IContext ctx, World world, int hours)
        {
            // TODO: Hide clock in caves unless carrying clock item
            var night = ctx.Theme.shadowDark;
            var day = ctx.Theme.vigorDark;
            var moon = ctx.Theme.white;
            var sun = ctx.Theme.danger;

            ctx.GetCanvas(RenderLayer.Root);
            Cell[] clock = new Cell[]
            {
                new Cell('▒', day, night),
                new Cell('░', day, night),
                new Cell(' ', moon, night),
                new Cell('•', moon, night),
                new Cell(' ', moon, night),
                new Cell('░', day, night),
                new Cell('▒', day, night),
                new Cell('▓', day, night),
                new Cell(' ', sun, day),
                new Cell('☼', sun, day),
                new Cell(' ', sun, day),
                new Cell('▓', day, night),
            };

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
                c.PutChar(x + i, viewport.y, cell.ch, cell.fore, cell.back);
            }
        }

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