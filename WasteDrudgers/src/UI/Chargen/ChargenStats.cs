using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;
using WasteDrudgers.State;

namespace WasteDrudgers.UI
{
    public class ChargenStats : IUIComponent
    {
        private ScrollMenu menu;
        private ChargenData data;

        public ChargenStats(ChargenData data)
        {
            menu = new ScrollMenu(22, 6);
            this.data = data;
        }

        public void Run(IContext ctx, World world, Command command)
        {
            var player = world.PlayerData.entity;
            ref var stats = ref world.ecs.GetRef<Stats>(player);
            ref var exp = ref world.ecs.GetRef<Experience>(player);

            var current = (StatType)menu.Selected;
            var cost = Formulae.GetStatCost(current);
            switch (command)
            {
                case Command.MenuUp:
                    menu.Prev();
                    break;
                case Command.MenuDown:
                    menu.Next();
                    break;
                case Command.MenuAccept:
                    if (exp.characterPoints >= cost)
                    {
                        stats[current]++;
                        data.stats[current]++;
                        data.statsSpent += cost;
                        exp.characterPoints -= cost;
                    }
                    break;
                case Command.MenuBack:
                    if (data.stats[current] > 0)
                    {
                        stats[current]--;
                        data.stats[current]--;
                        data.statsSpent -= Formulae.GetStatCost(current);
                        exp.characterPoints += cost;
                    }
                    break;
            }
        }

        public void Draw(IContext ctx, World world, IBlittable layer, int x, int y)
        {
            var theme = ctx.Theme;

            RenderUtils.DrawInfoSheetBox(ctx, layer, x, y + 1);
            RenderUtils.DrawControlHelpBar(ctx, layer, x, y + 24);

            var (menuX, menuY) = (x + 1, y + 8);
            var h = menuY - 2;

            layer.Print(menuX + 3, h, "Attribute", theme.text);
            layer.Print(menuX + 32, h, "Current", theme.text, TextAlignment.Center);
            layer.Print(menuX + 58, h, "Cost", theme.text, TextAlignment.Right);

            var entity = world.PlayerData.entity;
            var stats = world.ecs.GetRef<Stats>(entity);
            var skills = world.ecs.GetRef<Skills>(entity);

            RenderUtils.DrawCharacterSheetInfo(ctx, world, layer, x + 61, y + 2);
            RenderUtils.DrawCharacterPoints(ctx, world, layer, x + 2, y + 23);

            menu.Draw((i) =>
            {
                var stat = ((StatType)i);

                Color c;
                if (data != null && data.stats[stat] > 0)
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
                        layer.SetCellBackground(menuX + x, menuY + i * 2, Data.Colors.bronzeDark);
                    }
                }

                layer.PutChar(menuX + 1, menuY + i * 2, (char)('A' + i), theme.caption);
                layer.Print(menuX + 3, menuY + i * 2, stat.ToString(), c);
                layer.Print(menuX + 18, menuY + i * 2, stat.Abbr(), c);

                var cur = stats[stat].Base;
                layer.Print(menuX + 32, menuY + i * 2, cur.ToString(), c);
                layer.Print(menuX + 58, menuY + i * 2, Formulae.GetStatCost(stat).ToString(), c, TextAlignment.Right);
            });
        }
    }
}