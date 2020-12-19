using Blaggard.Graphics;

namespace WasteDrudgers.UI
{
    public interface IUIComponent
    {
        void Run(IContext ctx, World world, Command command);
        void Draw(IContext ctx, World world, IBlittable layer, int x, int y);
    }
}
