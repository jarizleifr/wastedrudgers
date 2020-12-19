using System;
using Blaggard.Common;
using Blaggard.Graphics;

namespace WasteDrudgers.UI
{
    // TODO: Make this into abstract class once menu overhaul is complete
    public class Menu
    {
        public int Selected { get; protected set; }
        public int Count { get; protected set; }

        public virtual int Index => Selected;

        public Menu(int length) => Count = length;

        public virtual int Next() => Selected = ++Selected > Count - 1 ? 0 : Selected;
        public virtual int Prev() => Selected = --Selected < 0 ? Count - 1 : Selected;
    }

    public class SimpleMenu : Menu
    {
        private string[] items;
        private int verticalSpacing;
        private TextAlignment alignment;

        public SimpleMenu(int verticalSpacing, TextAlignment alignment, params string[] items) : base(items.Length)
        {
            this.verticalSpacing = verticalSpacing;
            this.alignment = alignment;
            this.items = items;
        }

        public void Draw(IBlittable layer, int x, int y, Color text, Color selected)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (Index == i)
                {
                    layer.Print(x, y + i * verticalSpacing, items[i], selected, alignment);
                }
                else
                {
                    layer.Print(x, y + i * verticalSpacing, items[i], text, alignment);
                }
            }
        }
    }

    public class ScrollMenu : Menu
    {
        public int Offset { get; protected set; }
        public int View { get; protected set; }

        public override int Index => Selected + Offset;

        public ScrollMenu(int view, int length) : base(length) =>
            (Offset, View) = (0, Math.Min(view, length));

        public override int Next()
        {
            if (Selected < View - 1) Selected++;
            else if (Selected == View - 1 && Selected + Offset < Count - 1) Offset++;
            return Selected;
        }
        public override int Prev()
        {
            if (Selected > 0) Selected--;
            else if (Selected == 0 && Offset > 0) Offset--;
            return Selected;
        }

        public void Draw(Action<int> callback)
        {
            for (int i = 0; i < View; i++)
            {
                if (i + Offset >= Count) break;
                callback.Invoke(i + Offset);
            }
        }
    }
}