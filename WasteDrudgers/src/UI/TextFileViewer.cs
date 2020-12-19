using System.IO;
using Blaggard.Graphics;
using WasteDrudgers.Render;

namespace WasteDrudgers.UI
{
    public class TextFileReader
    {
        private int view;
        private int offset;
        private string[] lines;

        public TextFileReader(string path, int view)
        {
            this.view = view;
            lines = File.ReadAllLines(path);
        }

        public void ScrollUp()
        {
            if (offset > 0) offset--;
        }

        public void ScrollDown()
        {
            if (offset + view < lines.Length) offset++;
        }

        public void Draw(IContext ctx, IBlittable layer, int x, int y)
        {
            layer.DefaultFore = ctx.Theme.text;

            for (int i = 0; i < view; i++)
            {
                if (offset + i >= lines.Length) break;
                layer.Print(x, y + i, lines[offset + i]);
            }
            RenderUtils.DrawScrollBar(ctx, layer, x + 78, y, view, offset, lines.Length);
        }
    }
}