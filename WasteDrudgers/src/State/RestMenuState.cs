using Blaggard.Graphics;
using WasteDrudgers.UI;

namespace WasteDrudgers.State
{
    [InputDomains("menu")]
    public class RestMenuState : Scene
    {
        private SimpleMenu menu;
        private static string[] items = new[]
        {
            "Upgrade character",
            "Rest for 8 hours",
            "Break camp"
        };

        public override void Initialize(IContext ctx, World world)
        {
            menu = new SimpleMenu(2, TextAlignment.Center, items);
        }

        public override void Update(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuUp:
                    menu.Prev();
                    break;
                case Command.MenuDown:
                    menu.Next();
                    break;
                case Command.MenuAccept:
                    world.SetState(ctx, RunState.CharacterUpgrade);
                    break;
                case Command.GameMenu:
                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            var layer = ctx.QueueCanvas(RenderLayer.Root);
            layer.Clear();
            layer.SetRenderPosition(0, 0);
            menu.Draw(layer, layer.Width / 2, layer.Height / 2, ctx.Theme.text, ctx.Theme.selectedColor);
        }
    }
}