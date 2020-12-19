using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;
using WasteDrudgers.State;

namespace WasteDrudgers.UI
{
    public class ChargenSkills : IUIComponent
    {
        private ScrollMenu menu;
        private ChargenData data;

        public ChargenSkills(ChargenData data)
        {
            menu = new ScrollMenu(22, 10);
            this.data = data;
        }

        public void Run(IContext ctx, World world, Command command)
        {
            var player = world.PlayerData.entity;
            ref var skills = ref world.ecs.GetRef<Skills>(player);
            ref var exp = ref world.ecs.GetRef<Experience>(player);
            var current = (SkillType)menu.Selected;
            switch (command)
            {
                case Command.MenuUp:
                    menu.Prev();
                    break;
                case Command.MenuDown:
                    menu.Next();
                    break;
                case Command.MenuAccept:
                    if (exp.characterPoints >= 2)
                    {
                        skills.Increment(current);
                        data.skills.Increment(current);
                        data.skillsSpent += 2;
                        exp.characterPoints -= 2;
                    }
                    break;
                case Command.MenuBack:
                    if (data.skills.GetRank(current) > 0)
                    {
                        skills.Decrement(current);
                        data.skills.Decrement(current);
                        data.skillsSpent -= 2;
                        exp.characterPoints += 2;
                    }
                    break;
            }
        }

        public void Draw(IContext ctx, World world, IBlittable layer, int x, int y)
        {
            var theme = ctx.Theme;
            RenderUtils.DrawInfoSheetBox(ctx, layer, x, y + 1);
            RenderUtils.DrawControlHelpBar(ctx, layer, x, y + 24);

            var (menuX, menuY) = (x + 1, y + 7);

            layer.Print(menuX + 3, menuY - 1, "Skill", theme.text);
            layer.Print(menuX + 15, menuY - 1, "Attribute", theme.text, TextAlignment.Left);
            layer.Print(menuX + 26, menuY - 1, "Current", theme.text, TextAlignment.Right);
            layer.Print(menuX + 42, menuY - 1, "Cost", theme.text, TextAlignment.Right);

            var entity = world.PlayerData.entity;
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            RenderUtils.DrawCharacterSheetInfo(ctx, world, layer, x + 61, y + 2);
            RenderUtils.DrawCharacterPoints(ctx, world, layer, x + 2, y + 23);

            menu.Draw((i) =>
            {
                var skill = ((SkillType)i);
                var governStr = $"{Formulae.GetGoverningStat(skill).Abbr()}:{Formulae.GetSkillDifficulty(skill).Abbr()}";
                var current = $"{Formulae.BaseSkill(skill, stats) + skills.GetRank(skill)}%";

                Color c;
                if (data.skills.GetRank(skill) > 0)
                {
                    c = (i == menu.Selected) ? Data.Colors.bronzeLight : Data.Colors.white;
                }
                else
                {
                    c = (i == menu.Selected) ? theme.selectedColor : theme.text;
                }

                if (i == menu.Selected)
                {
                    for (int x = 0; x < 59; x++)
                    {
                        layer.SetCellBackground(menuX + x, menuY + i, Data.Colors.bronzeDark);
                    }
                }

                layer.PutChar(menuX + 1, menuY + i, (char)('A' + i), theme.caption);
                layer.Print(menuX + 3, menuY + i, skill.ToString(), c);
                layer.Print(menuX + 15, menuY + i, governStr, c);
                layer.Print(menuX + 40, menuY + i, current, c, TextAlignment.Right);
            });
        }
    }
}