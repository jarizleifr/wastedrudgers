using WasteDrudgers.Render;
using WasteDrudgers.UI;

namespace WasteDrudgers.State
{
    public class CharacterSheetState : IRunState
    {
        public string[] InputDomains { get; set; } = { "menu" };

        private CharacterScreen screen;
        private PlayerData playerData;

        public void Initialize(IContext ctx, World world)
        {
            playerData = world.PlayerData;

            screen = new CharacterScreen()
            {
                CharacterPoints = world.ecs.GetRef<Experience>(playerData.entity).characterPoints
            };
        }

        public void Run(IContext ctx, World world)
        {
            var menu = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.TerminalWindow(ctx);

            switch (ctx.Command)
            {
                case Command.MenuLeft:
                    screen.Prev();
                    break;
                case Command.MenuRight:
                    screen.Next();
                    break;
                case Command.MenuAccept:
                    screen.Buy(world, playerData);
                    break;
                case Command.MenuUp:
                    screen.Current.Prev();
                    break;
                case Command.MenuDown:
                    screen.Current.Next();
                    break;
                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            Views.DrawGameView(ctx, world);
            CharacterUI.DrawCharacterSheet(ctx, world, screen);
        }
    }
}