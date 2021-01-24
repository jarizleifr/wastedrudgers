using System;
using WasteDrudgers.Render;
using WasteDrudgers.UI;

namespace WasteDrudgers.State
{
    [InputDomains("menu")]
    public class CharacterSheetState : GameScene
    {
        private Tabs tabs;
        private IUIComponent current;
        private Func<CharacterSheetData, IUIComponent>[] stateFactories = new Func<CharacterSheetData, IUIComponent>[]
        {
            (_) => new CharacterSheetSkills(),
            (data) => new CharacterSheetTalents(data)
        };

        private string[] captions = { "Skills", "Talents" };
        private CharacterSheetData data;

        public override void Initialize(IContext ctx, World world)
        {
            tabs = new Tabs(captions);
            data = new CharacterSheetData(world);
            current = stateFactories[tabs.Selected](data);
        }

        public override void Update(IContext ctx, World world)
        {
            var layer = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.OffsetTerminalWindow(ctx);
            layer.SetRenderPosition(rect.x, rect.y);

            var command = ctx.Command;
            switch (command)
            {
                case Command.MenuLeft:
                    tabs.Prev();
                    current = stateFactories[tabs.Selected](data);
                    break;
                case Command.MenuRight:
                    tabs.Next();
                    current = stateFactories[tabs.Selected](data);
                    break;
                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            tabs.Draw(0, 0, layer);
            current.Run(ctx, world, command);
            current.Draw(ctx, world, layer, 0, 0);
        }
    }
}