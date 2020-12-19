using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.UI
{
    public class CharacterSheetSkills : IUIComponent
    {
        private ScrollMenu menu = new ScrollMenu(22, 10);

        public void Run(IContext ctx, World world, Command command)
        {
            var player = world.PlayerData.entity;
            ref var skills = ref world.ecs.GetRef<Skills>(player);
            var current = (SkillType)menu.Selected;
            switch (command)
            {
                case Command.MenuUp:
                    menu.Prev();
                    break;
                case Command.MenuDown:
                    menu.Next();
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

            var entity = world.PlayerData.entity;
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            RenderUtils.DrawCharacterSheetInfo(ctx, world, layer, x + 61, y + 2);

            menu.Draw((i) =>
            {
                var skill = ((SkillType)i);
                var governStr = $"{Formulae.GetGoverningStat(skill).Abbr()}:{Formulae.GetSkillDifficulty(skill).Abbr()}";
                var current = $"{Formulae.BaseSkill(skill, stats) + skills.GetRank(skill)}%";

                var c = (i == menu.Selected) ? theme.selectedColor : theme.text;

                if (i == menu.Selected)
                {
                    for (int x = 0; x < 59; x++)
                    {
                        layer.SetCellBackground(menuX + x, menuY + i, Data.Colors.bronzeDark);
                    }
                }

                layer.Print(menuX + 3, menuY + i, skill.ToString(), c);
                layer.Print(menuX + 15, menuY + i, governStr, c);
                layer.Print(menuX + 40, menuY + i, current, c, TextAlignment.Right);
            });
        }
    }
}