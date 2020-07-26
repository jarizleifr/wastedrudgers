using Blaggard.Graphics;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    internal class ConfigState : IRunState
    {
        public int selection;
        public string[] InputDomains { get; set; } = { "menu" };

        private Config tempConfig;

        public void Initialize(IContext ctx, World world)
        {
            tempConfig = ((Config)ctx.Config).Clone();
        }

        public void Run(IContext ctx, World world)
        {
            var items = CreateItems(tempConfig);
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    if (selection == 5)
                    {
                        ctx.ApplyConfig(world, tempConfig);
                        world.SetState(ctx, RunState.MainMenu(2));
                    }
                    else if (selection == 6)
                    {
                        world.SetState(ctx, RunState.MainMenu(2));
                    }
                    break;

                case Command.MenuLeft:
                    switch (selection)
                    {
                        case 0:
                            tempConfig.ScreenWidth--;
                            break;
                        case 1:
                            tempConfig.ScreenHeight--;
                            break;
                        case 2:
                            tempConfig.PixelMult--;
                            break;
                    }
                    break;

                case Command.MenuRight:
                    switch (selection)
                    {
                        case 0:
                            tempConfig.ScreenWidth++;
                            break;
                        case 1:
                            tempConfig.ScreenHeight++;
                            break;
                        case 2:
                            tempConfig.PixelMult++;
                            break;
                    }
                    break;

                case Command.MenuUp:
                    selection = Menu.Prev(selection, items.Length);
                    break;
                case Command.MenuDown:
                    selection = Menu.Next(selection, items.Length);
                    break;

                case Command.Exit:
                    world.SetState(ctx, RunState.MainMenu(2));
                    break;
            }

            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            root.DefaultFore = ctx.Theme.windowFrame;
            root.DefaultBack = ctx.Theme.windowBackground;
            root.Clear();

            root.Print(root.Width / 2, 3, "Game configuration", ctx.Theme.selectedColor, TextAlignment.Center);

            HUD.DrawScreenBorders(root, ctx.Theme);
            Menu.DrawConfigMenu(root, ctx.Theme, root.Width / 2, root.Height / 2 - items.Length + 1, selection, items);
        }

        private (string key, string value)[] CreateItems(Config cfg)
        {
            return new[]
            {
                ("Screen Width", tempConfig.ScreenWidth.ToString()),
                ("Screen Height", tempConfig.ScreenHeight.ToString()),
                ("Pixel Multiplier", tempConfig.PixelMult.ToString()),
                ("Message Log Height", "2"),
                ("Font", "font8x8.png"),
                ("", "Apply"),
                ("", "Discard"),
            };
        }
    }
}