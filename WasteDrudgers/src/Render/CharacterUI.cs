using Blaggard.Graphics;

namespace WasteDrudgers.Render
{
    public static class CharacterUI
    {
        public static void DrawCharacterSheet(IContext ctx, World world)
        {
            var offsets = RenderUtils.GetTerminalWindowOffsets(ctx);
            var layer = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            layer.SetRenderPosition(offsets.offsetX, offsets.offsetY);

            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;

            var rect = RenderUtils.TerminalWindow(ctx);
            layer.PrintFrame(rect, true);

            layer.Print(rect.x + rect.width / 2, rect.y, "Character", ctx.Theme.text, TextAlignment.Center);

            var playerData = world.ecs.FetchResource<PlayerData>();

            DrawInfo(rect.x + 5, rect.y + 1, layer, ctx.Theme, playerData);
            DrawStats(rect.x + 1, rect.y + 5, layer, ctx.Theme);
        }

        private static void DrawInfo(int x, int y, IBlittable layer, Theme theme, PlayerData playerData)
        {
            layer.Print(x, y, playerData.name, theme.caption);
        }

        private static void DrawStats(int x, int y, IBlittable layer, Theme theme)
        {
            layer.Print(x, y, "Strength", theme.caption);
            layer.Print(x, y + 1, "Endurance", theme.caption);
            layer.Print(x, y + 2, "Finesse", theme.caption);
            layer.Print(x, y + 3, "Intellect", theme.caption);
            layer.Print(x, y + 4, "Resolve", theme.caption);
            layer.Print(x, y + 5, "Awareness", theme.caption);
        }
    }
}