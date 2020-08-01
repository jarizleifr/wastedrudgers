using Blaggard.Graphics;

namespace WasteDrudgers.Tests
{
    public class TestContext : IEngineContext
    {
        public UIData UIData { get; private set; }
        public Theme Theme { get; private set; }
        public IConfig Config { get; private set; }

        public TextureData TextureData { get; private set; }

        public float DeltaTime { get; private set; }

        public int Width => 80;
        public int Height => 25;

        public void ApplyConfig(World world, Config config) { }

        public void HandleInput(string[] activeDomains) { }
        public Command Command => Command.None;
        public IBlittable QueueCanvas(RenderLayer layer) => null;

        public void Render() { }
        public void IncrementDeltaTime() { }
        public void WaitNextFrame(float time) { }
        public void Cleanup() { }
        public void Dispose() { }
    }
}