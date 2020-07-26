using Blaggard.Common;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    internal class AwaitingInputState : IRunState
    {
        private PlayerData playerData;

        public string[] InputDomains { get; set; } = { "game" };

        public void Initialize(IContext ctx, World world) => playerData = world.ecs.FetchResource<PlayerData>();

        public void Run(IContext ctx, World world)
        {
            Systems.EffectSystem(ctx, world);
            Views.DrawGameView(ctx, world);

            var nextState = ctx.Command switch
            {
                Command.MoveSouthWest => Move(world, Direction.SouthWest),
                Command.MoveSouth => Move(world, Direction.South),
                Command.MoveSouthEast => Move(world, Direction.SouthEast),
                Command.MoveWest => Move(world, Direction.West),
                Command.MoveEast => Move(world, Direction.East),
                Command.MoveNorthWest => Move(world, Direction.NorthWest),
                Command.MoveNorth => Move(world, Direction.North),
                Command.MoveNorthEast => Move(world, Direction.NorthEast),
                Command.Operate => Operate(world),
                Command.Inventory => RunState.Inventory(0, 0),
                Command.CharacterSheet => RunState.CharacterSheet(),
                Command.GetItem => GetItem(world),
                Command.Look => RunState.Look(playerData.coords),
                Command.Exit => RunState.EscapeMenu(0),
                _ => this
            };

            world.SetState(ctx, nextState);
        }

        private IRunState Operate(World world)
        {
            if (world.spatial.TryGetFeature(playerData.coords, out var feature))
            {
                var portal = world.ecs.GetRef<Portal>(feature);
                playerData.turns++;
                return RunState.LevelTransition(portal.targetLevel);
            }
            return this;
        }

        private IRunState Move(World world, Direction direction)
        {
            world.ecs.Assign(playerData.entity, new IntentionMove { transform = Vec2.FromDirection(direction) });
            playerData.turns++;
            return RunState.Ticking;
        }

        private IRunState GetItem(World world)
        {
            if (world.spatial.ItemsCountAt(playerData.coords) == 1)
            {
                world.ecs.Assign<IntentionGetItem>(playerData.entity, new IntentionGetItem { });
                return RunState.Ticking;
            }
            else if (world.spatial.ItemsCountAt(playerData.coords) > 1)
            {
                return RunState.PickUp(0, 0);
            }
            return this;
        }
    }
}