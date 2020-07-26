using Blaggard.Common;
using Blaggard.Graphics;

namespace WasteDrudgers.Render
{
    public interface IMenuItem
    {
        void Render(IContext ctx, IBlittable layer, Rect rect, int index, bool selected);
    }
}