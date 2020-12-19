using Blaggard.Graphics;
using WasteDrudgers.Render;
using WasteDrudgers.UI;

namespace WasteDrudgers.State
{
    internal class MainMenuState : IRunState
    {
        public int selection;
        private string[] items = { "New Game", "Load Game", "Configure", "Quit Game" };

        public string[] InputDomains { get; set; } = { "menu" };

        private SimpleMenu menu;

        public void Initialize(IContext ctx, World world)
        {
            menu = new SimpleMenu(2, TextAlignment.Left, items);
        }

        public void Run(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    world.SetState(ctx, menu.Selected switch
                    {
                        0 => RunState.NewGame,
                        1 => RunState.LoadGame(0),
                        2 => RunState.Config(0),
                        _ => null
                    });
                    break;

                case Command.MenuUp:
                    menu.Prev();
                    break;
                case Command.MenuDown:
                    menu.Next();
                    break;

                case Command.Exit:
                    world.SetState(ctx, null);
                    break;
            }

            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            RenderUtils.DrawTitleScreen(root);

            root.DefaultBack = ctx.Theme.windowBackground;
            root.DefaultFore = ctx.Theme.windowFrame;
            root.PrintFrame(7, -1, 16, root.Height + 2, true);
            menu.Draw(root, 10, 4, ctx.Theme.text, ctx.Theme.selectedColor);

            root.DefaultFore = ctx.Theme.caption;
            root.Print(9, 22, "Created by");
            root.Print(9, 23, "Antti Joutsi");
            root.Print(9, 24, "2016-2020");
        }
    }
}