using Blaggard.Graphics;
using WasteDrudgers.State;

namespace WasteDrudgers.UI
{
    public class ChargenOverview : IUIComponent
    {
        private static string[] options = new[] { "Change Portrait", "Change Name", "Start Game" };
        private SimpleMenu menu;
        private ChargenData data;

        public ChargenOverview(ChargenData data)
        {
            menu = new SimpleMenu(2, TextAlignment.Left, options);
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
                case Command.MenuAccept:
                    world.SetState(ctx, RunState.LevelGeneration(world.PlayerData.currentLevel, null, true));
                    break;
            }
        }

        public void Draw(IContext ctx, World world, IBlittable layer, int x, int y)
        {
            layer.DefaultBack = ctx.Theme.windowBackground;
            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.PrintFrame(x, y + 1, 80, 24, true);
            menu.Draw(layer, x + 40, y + 12, ctx.Theme.text, ctx.Theme.selectedColor);
        }
    }
}