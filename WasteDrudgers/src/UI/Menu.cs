using System;

namespace WasteDrudgers.UI
{
    public class Menu
    {
        public int Selected { get; protected set; }
        public int Count { get; protected set; }

        public virtual int Index => Selected;

        public Menu(int length) => Count = length;

        public virtual void Next() => Selected = ++Selected > Count - 1 ? 0 : Selected;
        public virtual void Prev() => Selected = --Selected < 0 ? Count - 1 : Selected;
    }

    public class ScrollMenu : Menu
    {
        public int Offset { get; protected set; }
        public int View { get; protected set; }

        public override int Index => Selected + Offset;

        public ScrollMenu(int view, int length) : base(length) =>
            (Offset, View) = (0, Math.Min(view, length));

        public override void Next()
        {
            if (Selected < View - 1) Selected++;
            else if (Selected == View - 1 && Selected + Offset < Count - 1) Offset++;
        }
        public override void Prev()
        {
            if (Selected > 0) Selected--;
            else if (Selected == 0 && Offset > 0) Offset--;
        }
    }
}