using System;
using System.Linq;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.State;
using WasteDrudgers.UI;

namespace WasteDrudgers.Render
{
    public static class CharacterUI
    {
        public const int STAT_SELECTION_LENGTH = 14;

        private static string[] categories = Enum.GetNames(typeof(CharacterMode));
        private static string[] statNames = Enum.GetNames(typeof(StatType)).Concat(new[] { "Health", "Vigor", "Magic" }).ToArray();
        private static string[] skillNames = Enum.GetNames(typeof(SkillType));

        public static CharacterMode NextCategory(CharacterMode category)
        {
            category++;
            if (category > (CharacterMode)categories.Length - 1)
            {
                category = 0;
            }
            return category;
        }
        public static CharacterMode PrevCategory(CharacterMode category)
        {
            category--;
            if (category < 0)
            {
                category = (CharacterMode)categories.Length - 1;
            }
            return category;
        }

        public static void DrawCharacterSheet(IContext ctx, World world, CharacterScreen state)
        {
            var offsets = RenderUtils.GetTerminalWindowOffsets(ctx);
            var layer = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            layer.SetRenderPosition(offsets.offsetX, offsets.offsetY);

            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;

            var rect = RenderUtils.TerminalWindow(ctx);
            layer.PrintFrame(rect, true);

            var menu = new Rect(51, 0, rect.width - 51, rect.height);
            layer.LineVert(menu.x, 1, rect.height - 2, '║');
            layer.LineHoriz(menu.x + 1, rect.height - 6, menu.width - 2, '═');

            var playerData = world.PlayerData;

            var stats = world.ecs.GetRef<Stats>(playerData.entity);
            var health = world.ecs.GetRef<Pools>(playerData.entity);
            var skills = world.ecs.GetRef<Skills>(playerData.entity);

            DrawInfo(ctx, world, rect.x + 2, rect.y + 1);
            DrawPortrait(ctx, rect.x + 20, rect.y + 1, layer);
            layer.LineHoriz(rect.x + 1, rect.y + 4, rect.width - menu.width - 1, '═');
            layer.LineHoriz(rect.x + 1, rect.y + 5, rect.width - menu.width - 1, '─');

            DrawCalendar(ctx, world, rect.x + 25, rect.y + 1);

            layer.Print(rect.x + 2, rect.y + 5, "Character", ctx.Theme.text);
            DrawCharacter(rect.x + 2, rect.y + 7, 13, layer, ctx.Theme, health);

            layer.Print(rect.x + 19, rect.y + 5, "General", ctx.Theme.text);
            DrawGeneral(ctx, world, rect.x + 19, rect.y + 7);

            layer.Print(rect.x + 36, rect.y + 5, "Melee", ctx.Theme.text);
            DrawMelee(ctx, world, rect.x + 36, rect.y + 7);
            layer.LineHoriz(rect.x + 1, rect.y + 15, rect.width - menu.width - 1, '─');

            DrawStats(rect.x + 2, rect.y + 17, 13, layer, ctx.Theme, stats);

            layer.Print(rect.x + 19, rect.y + 15, "Defense", ctx.Theme.text);
            DrawDefense(ctx, world, rect.x + 19, rect.y + 17);

            layer.Print(rect.x + 36, rect.y + 15, "Ranged", ctx.Theme.text);
            DrawRanged(rect.x + 36, rect.y + 17, layer, ctx.Theme);

            layer.LineVert(rect.x + 17, 1, rect.height - 2, '║');
            layer.LineVert(rect.x + 34, 6, rect.height - 7, '║');

            DrawCategories(ctx, layer, menu.x + 1, menu.y + 1, state.Mode);
            layer.LineHoriz(menu.x + 1, menu.y + 2, menu.width - 2, '═');

            switch (state.Mode)
            {
                case CharacterMode.Stats:
                    DrawStatSelection(layer, ctx.Theme, menu, state.Current.Selected, stats);
                    break;
                case CharacterMode.Skills:
                    var s = (ScrollMenu)state.Current;
                    DrawSkillSelection(layer, ctx.Theme, menu, s.Selected, s.Offset, skills, stats);
                    break;
            }

            layer.Print(menu.x + 2, menu.y + menu.height - 1, $"Points: {state.CharacterPoints}", ctx.Theme.text);
            layer.Print(menu.x + menu.width - 2, menu.y + menu.height - 1, $"Cost: {state.CurrentCost}", ctx.Theme.text, TextAlignment.Right);
        }

        private static void DrawStatSelection(IBlittable layer, Theme theme, Rect menu, int selected, Stats stats)
        {
            layer.LineHoriz(menu.x + 1, menu.y + 3, menu.width - 2, '─');
            layer.Print(menu.x + 3, menu.y + 3, "Primary", theme.text);
            layer.Print(menu.x + 20, menu.y + 3, "Current", theme.text, TextAlignment.Right);
            layer.Print(menu.x + menu.width - 1, menu.y + 3, "Cost", theme.text, TextAlignment.Right);

            layer.LineHoriz(menu.x + 1, menu.y + 12, menu.width - 2, '─');
            layer.Print(menu.x + 3, menu.y + 12, "Secondary", theme.text);
            layer.Print(menu.x + menu.width - 1, menu.y + 12, "Cost", theme.text, TextAlignment.Right);

            for (int i = 0; i < statNames.Length; i++)
            {
                var yOffset = i + ((i > 5) ? 3 : 0);

                layer.PutChar(menu.x + 1, menu.y + 5 + yOffset, (char)('A' + i), theme.text);

                var color = selected == i ? theme.selectedColor : theme.text;
                layer.Print(menu.x + 3, menu.y + 5 + yOffset, statNames[i], color);

                if (i < 6)
                {
                    var type = (StatType)i;
                    layer.Print(menu.x + menu.width - 1, menu.y + 5 + yOffset, Formulae.GetStatCost(type).ToString(), color, TextAlignment.Right);
                    layer.Print(menu.x + 20, menu.y + 5 + yOffset, stats[type].Current.ToString(), color, TextAlignment.Right);
                }
            }
        }

        private static void DrawSkillSelection(IBlittable layer, Theme theme, Rect menu, int selected, int offset, Skills skills, Stats stats)
        {
            layer.LineHoriz(menu.x + 1, menu.y + 3, menu.width - 2, '─');
            layer.Print(menu.x + 3, menu.y + 3, "Skill", theme.text);
            layer.Print(menu.x + 23, menu.y + 3, "Current", theme.text, TextAlignment.Right);
            layer.Print(menu.x + menu.width - 1, menu.y + 3, "Cost", theme.text, TextAlignment.Right);

            for (int i = 0; i < STAT_SELECTION_LENGTH; i++)
            {
                if (i + offset >= skillNames.Length) break;

                var color = selected == i ? theme.selectedColor : theme.text;
                var type = (SkillType)i + offset;
                layer.PutChar(menu.x + 1, menu.y + 5 + i, (char)('A' + i), theme.text);
                layer.Print(menu.x + 3, menu.y + 5 + i, skillNames[i + offset], color);
                layer.Print(menu.x + 23, menu.y + 5 + i, $"{Formulae.BaseSkill(type, stats) + skills.GetRank(type)}%", color, TextAlignment.Right);
                layer.Print(menu.x + menu.width - 1, menu.y + 5 + i, "2", color, TextAlignment.Right);
            }
        }

        private static void DrawInfo(IContext ctx, World world, int x, int y)
        {
            var c = ctx.GetCanvas(RenderLayer.MenuOverlay);
            var playerData = world.PlayerData;
            c.Print(x, y, playerData.name, ctx.Theme.caption);

            var experience = world.ecs.GetRef<Experience>(playerData.entity);

            c.Print(x, y + 1, "Level", ctx.Theme.caption);
            c.Print(x + 13, y + 1, experience.level.ToString(), ctx.Theme.text, TextAlignment.Right);

            c.Print(x, y + 2, "Next", ctx.Theme.caption);
            var next = Formulae.ExperienceNeededForLevel(experience.level + 1) - experience.experience;
            c.Print(x + 13, y + 2, next.ToString(), ctx.Theme.text, TextAlignment.Right);
        }

        private static void DrawPortrait(IContext ctx, int x, int y, IBlittable layer)
        {
            layer.Rect(x, y, 3, 3, ' ', Color.black, Color.black);
            layer.PutChar(x + 1, y + 1, '@', Data.Colors.white);
        }

        private static void DrawCalendar(IContext ctx, World world, int x, int y)
        {
            var c = ctx.GetCanvas(RenderLayer.MenuOverlay);
            var calendar = world.Calendar;

            c.Print(x, y, calendar.GetDayString(), ctx.Theme.text);
            c.Print(x, y + 1, calendar.GetMonthAndYearString(), ctx.Theme.text);
            c.Print(x, y + 2, calendar.GetTimeString(), ctx.Theme.text);
        }

        private static void DrawCharacter(int x, int y, int o, IBlittable layer, Theme theme, Pools health)
        {
            layer.Print(x, y, "Age", theme.caption);
            layer.Print(x, y + 1, "Size", theme.caption);
            layer.Print(x, y + 2, "Renown", theme.caption);

            layer.CaptionValue(theme, x, y + 4, o - 3, "Health", $"{health.health.Current}/{health.health.Max}", TextAlignment.Center);
            layer.CaptionValue(theme, x, y + 5, o - 3, "Vigor", $"{health.vigor.Current}/{health.vigor.Max}", TextAlignment.Center);
            layer.CaptionValue(theme, x, y + 6, o - 3, "Magic", "0/0", TextAlignment.Center);
        }

        private static void DrawGeneral(IContext ctx, World world, int x, int y)
        {
            var c = ctx.GetCanvas(RenderLayer.MenuOverlay);

            var playerData = world.PlayerData;

            var actor = world.ecs.GetRef<Actor>(playerData.entity);
            c.Print(x, y, "Action", ctx.Theme.caption);
            c.Print(x + 14, y, $"{actor.speed}%", ctx.Theme.text, TextAlignment.Right);

            c.Print(x, y + 1, "Movement", ctx.Theme.caption);

            c.Print(x, y + 2, "Vision", ctx.Theme.caption);

            c.Print(x, y + 4, "Heal Rate", ctx.Theme.caption);

            c.Print(x, y + 5, "Lift", ctx.Theme.caption);

            c.Print(x, y + 6, "Reaction", ctx.Theme.caption);
        }

        private static void DrawStats(int x, int y, int o, IBlittable layer, Theme theme, Stats stats)
        {
            for (int i = 0; i < 6; i++)
            {
                var type = (StatType)i;
                layer.CaptionValue(theme, x, y + i, o, type.ToString(), stats[type].Current);
            }
        }

        private static void DrawDefense(IContext ctx, World world, int x, int y)
        {
            var c = ctx.GetCanvas(RenderLayer.MenuOverlay);
            var playerData = world.PlayerData;

            var combat = world.ecs.GetRef<Combat>(playerData.entity);
            c.Print(x, y, "Parry", ctx.Theme.caption);
            c.Print(x + 14, y, $"{combat.dodge}%", ctx.Theme.text, TextAlignment.Right);

            c.Print(x, y + 1, "Armor", ctx.Theme.caption);
            c.Print(x + 13, y + 1, $"{combat.armor}", ctx.Theme.text, TextAlignment.Right);

            c.Print(x, y + 3, "Evasion", ctx.Theme.caption);
            c.Print(x, y + 4, "Physical", ctx.Theme.caption);
            c.Print(x, y + 5, "Mental", ctx.Theme.caption);
        }

        private static void DrawMelee(IContext ctx, World world, int x, int y)
        {
            var c = ctx.GetCanvas(RenderLayer.MenuOverlay);
            var playerData = world.PlayerData;
            var combat = world.ecs.GetRef<Combat>(playerData.entity);
            c.Print(x, y, "Damage", ctx.Theme.caption);
            c.Print(x + 13, y, combat.damage.ToString(), ctx.Theme.text, TextAlignment.Right);
            c.Print(x, y + 1, "To-Hit", ctx.Theme.caption);
            c.Print(x + 14, y + 1, $"{combat.hitChance}%", ctx.Theme.text, TextAlignment.Right);
            c.Print(x, y + 2, "Speed", ctx.Theme.caption);

            c.Print(x, y + 4, "Damage", ctx.Theme.caption);
            c.Print(x, y + 5, "To-Hit", ctx.Theme.caption);
            c.Print(x, y + 6, "Speed", ctx.Theme.caption);
        }

        private static void DrawRanged(int x, int y, IBlittable layer, Theme theme)
        {
            layer.Print(x, y, "Damage", theme.caption);
            layer.Print(x, y + 1, "To-Hit", theme.caption);
            layer.Print(x, y + 2, "Speed", theme.caption);

            layer.Print(x, y + 4, "Range", theme.caption);
            layer.Print(x, y + 5, "Ammo", theme.caption);
        }

        private static void DrawCategories(IContext ctx, IBlittable layer, int x, int y, CharacterMode mode)
        {
            int offset = 0;
            for (int i = 0; i < categories.Length; i++)
            {
                layer.Print(x + offset, y, categories[i], i == (int)mode ? ctx.Theme.selectedColor : ctx.Theme.text);
                offset += categories[i].Length + 1;
            }
        }
    }
}