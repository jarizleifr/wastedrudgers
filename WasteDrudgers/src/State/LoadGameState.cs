using System.IO;
using System.Linq;
using Blaggard.Common;
using Blaggard.FileIO;
using Blaggard.Graphics;
using WasteDrudgers.Level;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    public class SaveWrapper : IMenuItem
    {
        public readonly string filePath;

        public SaveWrapper(string filePath)
        {
            this.filePath = filePath;
        }

        public void Render(IContext ctx, IBlittable layer, Rect rect, int i, bool selected)
        {
            layer.Print(rect.x + 1, rect.y + 1 + i, filePath, selected ? ctx.Theme.selectedColor : ctx.Theme.text);
        }
    }

    // TODO: Show some metadata with saves
    // TODO: Implement save deletion
    internal class LoadGameState : IRunState
    {
        public int selection;
        private SaveWrapper[] items;

        public string[] InputDomains { get; set; } = { "menu" };

        public void Initialize(IContext ctx, World world)
        {
            var path = SerializationUtils.GetSaveFolderPath();
            var dir = new DirectoryInfo(path);

            items = dir.GetFiles().Select(f => new SaveWrapper(f.Name)).ToArray();
        }

        public void Run(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    Load(ctx, world, items[selection].filePath);
                    break;

                case Command.MenuUp:
                    selection = Menu.Prev(selection, items.Length);
                    break;
                case Command.MenuDown:
                    selection = Menu.Next(selection, items.Length);
                    break;

                case Command.Exit:
                    world.SetState(ctx, RunState.MainMenu(1));
                    break;
            }

            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            root.DefaultFore = ctx.Theme.windowFrame;
            root.DefaultBack = ctx.Theme.windowBackground;
            root.Clear();

            HUD.DrawScreenBorders(root, ctx.Theme);

            var menu = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.TerminalWindow(ctx);
            var offsets = RenderUtils.GetTerminalWindowOffsets(ctx);

            menu.SetRenderPosition(offsets.offsetX, offsets.offsetY);

            menu.DefaultFore = ctx.Theme.windowFrame;
            menu.DefaultBack = ctx.Theme.windowBackground;
            menu.PrintFrame(rect, true);

            var menuRect = new Rect(rect.x, rect.y, 20, 25);
            Menu.DrawCompactMenu<SaveWrapper>(ctx, menu, menuRect, selection, items);
        }

        private void Load(IContext ctx, World world, string save)
        {
            world.ecs.Clear();
            var path = SerializationUtils.GetSaveFolderPath();
            var saveFile = Path.Combine(path, save);

            var global = ZipUtils.LoadJsonFromZip(saveFile, "global.json");
            world.ecs.Deserialize(global);
            var globalData = world.ecs.FetchResource<PlayerData>();

            var level = ZipUtils.LoadJsonFromZip(saveFile, globalData.currentLevel + ".json");
            world.ecs.Deserialize(level);

            world.SetState(ctx, RunState.LevelGeneration(globalData.currentLevel, world.ecs.FetchResource<Map>()));
        }
    }
}