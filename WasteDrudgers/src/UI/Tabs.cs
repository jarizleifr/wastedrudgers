using Blaggard.Graphics;

namespace WasteDrudgers.UI
{
    public class Tabs : Menu
    {
        public string[] Captions { private get; set; }
        public Tabs(params string[] captions) : base(captions.Length) =>
            Captions = captions;

        public void Draw(int x, int y, IBlittable layer)
        {
            layer.DefaultFore = Data.Colors.black;
            layer.DefaultBack = Data.Colors.greyLight;

            var (o, i) = (0, 0);
            foreach (var c in Captions)
            {
                if (i == Index)
                {
                    layer.Print(x + o, y, c, Data.Colors.bronze);
                }
                else
                {
                    layer.Print(x + o, y, c);
                }

                o += c.Length;
                i++;
            }
            layer.LineHoriz(x + o, y, 80 - o, ' ');
        }
    }
}