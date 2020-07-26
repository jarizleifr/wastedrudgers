using WasteDrudgers.Render;
namespace WasteDrudgers.State
{
    internal class EscapeMenuState : IRunState
    {
        public int selection;
        private string[] items = { "Save and Exit to Menu", "Save and Quit Game", "Retire Character", "Return to Game" };

        public string[] InputDomains { get; set; } = { "menu" };

        public void Run(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    var state = selection switch
                    {
                        0 => RunState.SaveGame("testsave", RunState.MainMenu(0)),
                        1 => RunState.SaveGame("testsave", null),
                        2 => RunState.MainMenu(0),
                        3 => RunState.AwaitingInput,
                        _ => this
                    };
                    world.SetState(ctx, state);
                    break;

                case Command.MenuUp:
                    selection = Menu.Prev(selection, items.Length);
                    break;
                case Command.MenuDown:
                    selection = Menu.Next(selection, items.Length);
                    break;

                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            var offsets = RenderUtils.GetTerminalWindowOffsets(ctx);
            Views.DrawGameView(ctx, world);

            var layer = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            layer.Clear();
            layer.SetRenderPosition(offsets.offsetX, offsets.offsetY);
            Menu.DrawMenu(layer, ctx.Theme, layer.Width / 2, layer.Height / 2, selection, items);
        }
    }
}