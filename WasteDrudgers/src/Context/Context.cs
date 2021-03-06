using System;
using System.Collections.Generic;
using System.IO;
using Blaggard.Common;
using Blaggard.Graphics;
using Blaggard.Input;

using Newtonsoft.Json;

namespace WasteDrudgers
{
    public interface IContext
    {
        Theme Theme { get; }
        UIData UIData { get; }
        IConfig Config { get; }
        Command Command { get; }
        float DeltaTime { get; }

        int Width { get; }
        int Height { get; }

        void ApplyConfig(World world, Config config);
        IBlittable QueueCanvas(RenderLayer layer);
        IBlittable GetCanvas(RenderLayer layer);
        TextureData TextureData { get; }
    }

    public interface IEngineContext : IContext, IDisposable
    {
        void HandleInput(string[] activeDomains);
        void Render();
        void WaitNextFrame(float time);
        void IncrementDeltaTime();
        void Cleanup();
    }

    public class Context : Blaggard.Context, IEngineContext
    {
        private Display display;
        private Handler<Command> input;

        private LayerRenderer renderer;

        public UIData UIData { get; private set; }
        public Theme Theme { get; private set; }
        public IConfig Config { get; private set; }

        public TextureData TextureData { get; private set; }

        public float DeltaTime { get; private set; }

        public int Width => display.width;
        public int Height => display.height;

        private DateTime lastTime = DateTime.Now;

        public Context(World world) : base()
        {
            Config = new Config(80, 40, 2, GraphicsStyle.CP437, false, false);
            Initialize(world);
        }

        private void Initialize(World world)
        {
            display = new Display(Config.ScreenWidth, Config.ScreenHeight, Config.PixelMult, Config.TerminalMode);
            TextureData = new TextureData(display.GetRenderer());

            UIData = new UIData(display);
            input = new Handler<Command>();
            Theme = new Theme(world);

            renderer = new LayerRenderer(new List<Func<IBlittable>> {
                () => new Canvas(display, Config.ScreenWidth, Config.ScreenHeight),
                () => new Canvas(display, UIData.viewport.width - 2, UIData.viewport.height - 2),
                () => new SpriteCanvas(display, UIData.viewport.width - 2, UIData.viewport.height - 2),
                () => new SparseCanvas(display, 80, 25),
            });
        }

        public void ApplyConfig(World world, Config config)
        {
            renderer.Dispose();
            TextureData.Dispose();
            display.Dispose();
            GC.Collect();

            Config = config;
            Initialize(world);
        }

        public void HandleInput(string[] activeDomains) => input.Handle(activeDomains);
        public Command Command => input.Get();

        public IBlittable GetCanvas(RenderLayer layer) => renderer.Get((int)layer);

        public IBlittable QueueCanvas(RenderLayer layer)
        {
            renderer.SetToRender((int)layer);
            return renderer.Get((int)layer);
        }

        public void Render()
        {
            renderer.Render(display);
            renderer.Get((int)RenderLayer.MenuOverlay).Clear();
        }

        public void IncrementDeltaTime() => DeltaTime = GetDeltaTime();

        public override void Cleanup()
        {
            display.Dispose();
        }
    }

    // TODO: Move theme and UIData to Config

    public struct UIData
    {
        public readonly Rect log;
        public readonly Rect sidebar;
        public readonly Rect viewport;

        public UIData(Display display)
        {
            var logMinHeight = 2;
            var sidebarMinWidth = 13;

            double tilesX = (display.width - sidebarMinWidth) * display.cellWidth;
            double tilesY = (display.height - logMinHeight - 1) * display.cellHeight;

            var viewportWidth = (int)(Math.Floor(tilesX));
            var viewportHeight = (int)(Math.Floor(tilesY));

            var logHeight = display.height - viewportHeight / display.cellHeight - 1;
            var sidebarWidth = display.width - viewportWidth / display.cellWidth;
            var footerHeight = 1;

            log = new Rect(0, 0, display.width, logHeight);
            sidebar = new Rect(0, logHeight + 1, sidebarWidth, display.height - logHeight - 1);
            viewport = new Rect(sidebarWidth, logHeight, display.width - sidebarWidth, display.height - logHeight - footerHeight);
        }

        public void Deconstruct(out Rect log, out Rect sidebar, out Rect viewport) =>
            (log, sidebar, viewport) = (this.log, this.sidebar, this.viewport);
    }

    public interface IConfig
    {
        int ScreenWidth { get; }
        int ScreenHeight { get; }

        int PixelMult { get; }

        GraphicsStyle Style { get; }
        bool TerminalMode { get; }
        bool SimpleTiles { get; }
    }

    public class Config : IConfig
    {
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public int PixelMult { get; set; }

        public GraphicsStyle Style { get; set; }
        public bool TerminalMode { get; set; }
        public bool SimpleTiles { get; set; }

        public Config(int width, int height, int pixelMult, GraphicsStyle style, bool terminalMode, bool simpleTiles)
        {
            ScreenWidth = width;
            ScreenHeight = height;
            PixelMult = pixelMult;
            Style = style;
            TerminalMode = terminalMode;
            SimpleTiles = simpleTiles;
        }

        public Config Clone()
        {
            return new Config(ScreenWidth, ScreenHeight, PixelMult, Style, TerminalMode, SimpleTiles);
        }
    }

    public class Theme
    {
        public readonly Color caption;
        public readonly Color text;
        public readonly Color disabledText;
        public readonly Color selectedColor;
        public readonly Color windowFrame;
        public readonly Color windowBackground;

        public readonly Color critical;
        public readonly Color danger;
        public readonly Color fortified;

        public Theme(World world)
        {
            var theme = JsonConvert.DeserializeObject<Dictionary<string, Color>>(File.ReadAllText("assets/theme.json"));

            caption = theme["caption"];
            text = theme["text"];
            disabledText = theme["disabledText"];
            selectedColor = theme["selectedColor"];
            windowFrame = theme["windowFrame"];
            windowBackground = theme["windowBackground"];

            critical = theme["critical"];
            danger = theme["danger"];
            fortified = theme["fortified"];
        }
    }
}
