using Blaggard.Graphics;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    internal class MainMenuState : IRunState
    {
        public int selection;
        private string[] items = { "Start a New Quest", "Continue an Existing Adventure", "Configure", "Forfeit Your Destiny" };
        private const string title = "WASTE DRUDGERS";
        private const string credit = "Created by Antti Joutsi 2016-2020";

        public string[] InputDomains { get; set; } = { "menu" };

        public void Run(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    world.SetState(ctx, selection switch
                    {
                        0 => RunState.NewGame,
                        1 => RunState.LoadGame(0),
                        2 => RunState.Config(0),
                        _ => null
                    });
                    break;

                case Command.MenuUp:
                    selection = Menu.Prev(selection, items.Length);
                    break;
                case Command.MenuDown:
                    selection = Menu.Next(selection, items.Length);
                    break;

                case Command.Exit:
                    world.SetState(ctx, null);
                    break;
            }

            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            root.DefaultFore = ctx.Theme.windowFrame;
            root.DefaultBack = ctx.Theme.windowBackground;
            root.Clear();

            root.Print(root.Width / 2, 3, title, ctx.Theme.selectedColor, TextAlignment.Center);
            root.Print(root.Width / 2, root.Height - 3, credit, ctx.Theme.selectedColor, TextAlignment.Center);

            HUD.DrawScreenBorders(root, ctx.Theme);
            Menu.DrawMenu(root, ctx.Theme, root.Width / 2, root.Height / 2 - items.Length + 1, selection, items);
        }
    }
}