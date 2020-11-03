using System;
using Blaggard.Common;
using Blaggard.Graphics;

namespace WasteDrudgers.Tests
{
    public class TestContext : IEngineContext
    {
        public UIData UIData { get; private set; }
        public Colors Colors { get; private set; }
        public Theme Theme { get; private set; }
        public IConfig Config { get; private set; }

        public TestContext(World world)
        {
            Config = new Config(80, 25, 1, GraphicsStyle.CP437, false, false);
            Theme = new Theme(world);
        }

        public TextureData TextureData { get; private set; }

        public float DeltaTime { get; private set; }

        public int Width => 80;
        public int Height => 25;

        public void ApplyConfig(World world, Config config) { }

        public void HandleInput(string[] activeDomains) { }
        public Command Command => Command.None;
        public IBlittable GetCanvas(RenderLayer layer) => new TestCanvas();
        public IBlittable QueueCanvas(RenderLayer layer) => new TestCanvas();

        public void Render() { }
        public void IncrementDeltaTime() { }
        public void WaitNextFrame(float time) { }
        public void Cleanup() { }
        public void Dispose() { }
    }

    public class TestCanvas : IBlittable
    {
        public IntPtr Texture => IntPtr.Zero;
        public bool Dirty => false;
        public Rect RenderRect => new Rect(0, 0, 0, 0);
        public Color DefaultFore { get; set; }
        public Color DefaultBack { get; set; }
        public int Width => 80;
        public int Height => 25;
        public void SetRenderPosition(int x, int y) { }
        public void ResetColors() { }
        public void Clear() { }
        public void SetCell(int x, int y, char? ch, Color? fore, Color? back) { }
        public void Render() { }
        public void Dispose() { }
    }
}