using System;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;
using WasteDrudgers.UI;

namespace WasteDrudgers.State
{
    [InputDomains("menu")]
    public class CharacterUpgradeState : Scene
    {
        private Tabs tabs;
        private IUIComponent current;
        private Func<ChargenData, IUIComponent>[] stateFactories = new Func<ChargenData, IUIComponent>[]
        {
            (data) => new ChargenStats(data),
            (data) => new ChargenSkills(data),
            (data) => new ChargenTalents(data),
        };

        private string[] captions = { "Stats", "Skills", "Talents" };
        private ChargenData data;

        public override void Initialize(IContext ctx, World world)
        {
            tabs = new Tabs(captions);
            data = new ChargenData();
            current = stateFactories[tabs.Selected](data);
        }

        public override void Update(IContext ctx, World world)
        {
            var root = ctx.QueueCanvas(RenderLayer.Root);
            var rect = RenderUtils.OffsetTerminalWindow(ctx);
            root.SetRenderPosition(0, 0);

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

            tabs.Draw(rect.x, rect.y, root);
            current.Run(ctx, world, command);
            current.Draw(ctx, world, root, rect.x, rect.y);

            if (command == Command.MenuAccept || command == Command.MenuBack)
            {
                UpdateCaptions();
                Creatures.UpdateCreature(world, world.PlayerData.entity);
            }
        }

        private void UpdateCaptions()
        {
            tabs.Captions = new string[]
            {
                $"Stats ({data.statsSpent})",
                $"Skills ({data.skillsSpent})",
                "Talents",
            };
        }
    }
}