using System.Collections.Generic;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.UI
{
    public class ChargenTalents : IUIComponent
    {
        private List<DBTalent> talents;

        private ScrollMenu menu;
        private ChargenData data;

        public ChargenTalents(ChargenData data)
        {
            talents = Data.GetAllTalents();
            menu = new ScrollMenu(22, talents.Count);
            this.data = data;
        }

        public void Run(IContext ctx, World world, Command command)
        {
            var player = world.PlayerData.entity;
            var currentTalents = Effects.GetOwnedTalentIds(world, player);
            var current = talents[menu.Selected];
            ref var exp = ref world.ecs.GetRef<Experience>(player);
            switch (command)
            {
                case Command.MenuUp:
                    menu.Prev();
                    break;
                case Command.MenuDown:
                    menu.Next();
                    break;
                case Command.MenuAccept:
                    if (!currentTalents.Contains(current.Id))
                    {
                        if (exp.characterPoints >= current.Cost)
                        {
                            Effects.AddTalent(world, current.Id, player);
                            exp.characterPoints -= current.Cost;
                        }
                    }
                    else
                    {
                        Effects.RemoveTalent(world, current.Id, player);
                        exp.characterPoints += current.Cost;
                    }
                    break;
                case Command.MenuBack:
                    if (currentTalents.Contains(current.Id))
                    {
                        Effects.RemoveTalent(world, current.Id, player);
                        exp.characterPoints += current.Cost;
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

            layer.Print(menuX + 3, menuY - 1, "Talent", theme.text);
            layer.Print(menuX + 36, menuY - 1, "Prerequisites", theme.text, TextAlignment.Left);
            layer.Print(menuX + 58, menuY - 1, "Cost", theme.text, TextAlignment.Right);

            var entity = world.PlayerData.entity;
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            RenderUtils.DrawCharacterSheetInfo(ctx, world, layer, x + 61, y + 2);
            RenderUtils.DrawCharacterPoints(ctx, world, layer, x + 2, y + 23);

            var currentTalents = Effects.GetOwnedTalentIds(world, entity);

            menu.Draw((i) =>
            {
                var talent = talents[i];

                Color c;
                if (currentTalents.Contains(talent.Id))
                {
                    c = (i == menu.Selected) ? Data.Colors.bronzeLight : Data.Colors.white;
                }
                else
                {
                    c = (i == menu.Selected) ? theme.selectedColor : theme.text;
                }
                // TODO: grey out talents that you don't meet requirements for

                if (i == menu.Selected)
                {
                    for (int x = 0; x < 59; x++)
                    {
                        layer.SetCellBackground(menuX + x, menuY + i, Data.Colors.bronzeDark);
                    }
                }

                layer.PutChar(menuX + 1, menuY + i, (char)('A' + i), theme.caption);
                layer.Print(menuX + 3, menuY + i, talent.Name, c);
                layer.Print(menuX + 58, menuY + i, talent.Cost.ToString(), c, TextAlignment.Right);
            });
        }
    }
}