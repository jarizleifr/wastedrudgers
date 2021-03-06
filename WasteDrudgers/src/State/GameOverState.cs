using Blaggard.Graphics;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    [InputDomains("menu")]
    public class GameOverState : Scene
    {
        public override void Initialize(IContext ctx, World world)
        {
            // Delete save game on game over
            var playerData = world.PlayerData;
            SerializationUtils.DeleteSave(playerData.name);

            world.ecs.Clear();
            world.log.Clear();
            world.spatial.Clear();
        }

        public override void Update(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                case Command.Exit:
                    world.SetState(ctx, RunState.MainMenu(0));
                    break;
            }

            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            root.DefaultFore = ctx.Theme.windowFrame;
            root.DefaultBack = ctx.Theme.windowBackground;
            root.Clear();

            HUD.DrawScreenBorders(root, ctx.Theme);

            root.Print(root.Width / 2, root.Height / 2 - 2, "Game Over", Data.Colors.red, TextAlignment.Center);
            root.Print(root.Width / 2, root.Height / 2 - 1, "You are dead", Data.Colors.red, TextAlignment.Center);
            root.Print(root.Width / 2, root.Height / 2 + 1, "Press 'Enter' or 'Escape' to return to main menu", ctx.Theme.text, TextAlignment.Center);
        }
    }
}