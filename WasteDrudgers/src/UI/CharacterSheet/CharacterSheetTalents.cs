using System.Collections.Generic;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.UI
{
    public class CharacterSheetTalents : IUIComponent
    {
        private List<DBTalent> talents;

        private ScrollMenu menu;
        private CharacterSheetData data;

        public CharacterSheetTalents(CharacterSheetData data)
        {
            menu = new ScrollMenu(22, data.PickedTalents.Count);
            this.data = data;
        }

        public void Run(IContext ctx, World world, Command command)
        {
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

            layer.Print(menuX + 3, menuY - 1, "Talent", theme.text);

            var entity = world.PlayerData.entity;
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            RenderUtils.DrawCharacterSheetInfo(ctx, world, layer, x + 61, y + 2);

            var currentTalents = Talents.GetOwnedTalentIds(world, entity);

            menu.Draw((i) =>
            {
                var talent = data.PickedTalents[i];

                if (i == menu.Selected)
                {
                    for (int x = 0; x < 59; x++)
                    {
                        layer.SetCellBackground(menuX + x, menuY + i, Data.Colors.bronzeDark);
                    }
                }
                var c = (i == menu.Selected) ? theme.selectedColor : theme.text;
                layer.Print(menuX + 3, menuY + i, talent.Name, c);
            });
        }
    }
}