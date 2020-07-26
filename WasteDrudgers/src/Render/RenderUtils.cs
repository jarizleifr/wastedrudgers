using Blaggard.Common;

namespace WasteDrudgers.Render
{
    public static class RenderUtils
    {
        ///<summary>Creates a centered 80x25 Rect for terminal sized stuff.</summary>
        public static Rect TerminalWindow(IContext ctx) => new Rect(0, 0, 80, 25);

        public static (int offsetX, int offsetY) GetTerminalWindowOffsets(IContext ctx) =>
            ((ctx.Width - 80) / 2, (ctx.Height - 25) / 2);
    }
}